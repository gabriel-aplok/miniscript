using System.Text;
using MiniScript.Errors;
using MiniScript.Lexer;
using MiniScript.Parser;

namespace MiniScript.Runtime;

public class Interpreter
{
    public Environment Globals { get; } = new();
    private Environment _environment;

    public Interpreter()
    {
        _environment = Globals;
        // inject built-ins
        Globals.Define(
            "print",
            new BuiltinFunction(
                1,
                args =>
                {
                    Console.WriteLine(args[0]);
                    return null;
                }
            )
        );
        Globals.Define(
            "input",
            new BuiltinFunction(
                0,
                _ =>
                {
                    return Console.ReadLine();
                }
            )
        );
        Globals.Define(
            "clock",
            new BuiltinFunction(0, _ => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0)
        );
        Globals.Define(
            "sqrt",
            new BuiltinFunction(1, args => Math.Sqrt(Convert.ToDouble(args[0])))
        );
    }

    public void Interpret(List<Stmt> statements)
    {
        // try
        // {
        foreach (Stmt stmt in statements)
        {
            Execute(stmt);
        }
        // }
        // catch (RuntimeException ex)
        // {
        //     Console.WriteLine($"Runtime Error [{ex.Token.Line}]: {ex.Message}");
        // }
    }

    private void Execute(Stmt stmt)
    {
        switch (stmt)
        {
            case BlockStmt b:
                ExecuteBlock(b, new Environment(_environment));
                break;
            case ExpressionStmt e:
                Evaluate(e.Expression);
                break;
            case FunctionStmt f:
                _environment.Define(f.Name.Lexeme, new MiniScriptFunction(f, _environment));
                break;
            case IfStmt i:
                if (IsTruthy(Evaluate(i.Condition)))
                {
                    Execute(i.ThenBranch);
                }
                else if (i.ElseBranch != null)
                {
                    Execute(i.ElseBranch);
                }

                break;
            case ReturnStmt r:
                throw new ReturnException(r.Value != null ? Evaluate(r.Value) : null);
            case VarStmt v:
                _environment.Define(
                    v.Name.Lexeme,
                    v.Initializer != null ? Evaluate(v.Initializer) : null
                );
                break;
            case WhileStmt w:
                while (IsTruthy(Evaluate(w.Condition)))
                {
                    Execute(w.Body);
                }

                break;
        }
    }

    public void ExecuteBlock(BlockStmt block, Environment environment)
    {
        Environment previous = _environment;
        try
        {
            _environment = environment;
            foreach (Stmt stmt in block.Statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    private object? Evaluate(Expr expr)
    {
        return expr switch
        {
            AssignExpr a => EvaluateAssign(a),
            BinaryExpr b => EvaluateBinary(b),
            CallExpr c => EvaluateCall(c),
            LiteralExpr l => l.Value,
            LogicalExpr l => EvaluateLogical(l),
            UnaryExpr u => EvaluateUnary(u),
            VariableExpr v => _environment.Get(v.Name),
            InterpolatedStringExpr i => EvaluateInterpolation(i),
            _ => throw new NotImplementedException(),
        };
    }

    private object? EvaluateAssign(AssignExpr a)
    {
        object? value = Evaluate(a.Value);
        _environment.Assign(a.Name, value);
        return value;
    }

    private object? EvaluateCall(CallExpr c)
    {
        object? callee = Evaluate(c.Callee);
        List<object?> args = c.Arguments.Select(Evaluate).ToList();

        if (callee is not ICallable callable)
        {
            throw new RuntimeException(c.Paren, "Can only call functions.");
        }

        if (args.Count != callable.Arity)
        {
            throw new RuntimeException(
                c.Paren,
                $"Expected {callable.Arity} arguments but got {args.Count}."
            );
        }

        return callable.Call(this, args);
    }

    private object? EvaluateLogical(LogicalExpr expr)
    {
        object? left = Evaluate(expr.Left);

        if (expr.Operator.Type == TokenType.Or)
        {
            // short-circuit: if left is true, return left
            if (IsTruthy(left))
            {
                return left;
            }
        }
        else // TokenType.And
        {
            // short-circuit: if left is false, return left
            if (!IsTruthy(left))
            {
                return left;
            }
        }

        return Evaluate(expr.Right);
    }

    private object? EvaluateUnary(UnaryExpr u)
    {
        object? right = Evaluate(u.Right);
        return u.Operator.Type switch
        {
            TokenType.Minus => -(double)CheckNumber(u.Operator, right),
            TokenType.Bang => !IsTruthy(right),
            TokenType.Not => !IsTruthy(right),
            _ => null,
        };
    }

    private object? EvaluateInterpolation(InterpolatedStringExpr expr)
    {
        StringBuilder builder = new();

        foreach (Expr part in expr.parts)
        {
            object? value = Evaluate(part);

            // convert null to empty string or the word "null" based on preference
            builder.Append(value?.ToString() ?? "null");
        }

        return builder.ToString();
    }

    private object? EvaluateBinary(BinaryExpr b)
    {
        object? left = Evaluate(b.Left);
        object? right = Evaluate(b.Right);

        switch (b.Operator.Type)
        {
            case TokenType.Plus:
                if (left is double d1 && right is double d2)
                {
                    return d1 + d2;
                }

                if (left is string s1 && right is string s2)
                {
                    return s1 + s2;
                }

                if (left is string s3)
                {
                    return s3 + right;
                }

                throw new RuntimeException(b.Operator, "Operands must be two numbers or strings.");
            case TokenType.Minus:
                return (double)CheckNumber(b.Operator, left)
                    - (double)CheckNumber(b.Operator, right);
            case TokenType.Slash:
                return (double)CheckNumber(b.Operator, left)
                    / (double)CheckNumber(b.Operator, right);
            case TokenType.Star:
                return (double)CheckNumber(b.Operator, left)
                    * (double)CheckNumber(b.Operator, right);
            case TokenType.Modulo:
                return (double)CheckNumber(b.Operator, left)
                    % (double)CheckNumber(b.Operator, right);
            case TokenType.Greater:
                return (double)CheckNumber(b.Operator, left)
                    > (double)CheckNumber(b.Operator, right);
            case TokenType.GreaterEqual:
                return (double)CheckNumber(b.Operator, left)
                    >= (double)CheckNumber(b.Operator, right);
            case TokenType.Less:
                return (double)CheckNumber(b.Operator, left)
                    < (double)CheckNumber(b.Operator, right);
            case TokenType.LessEqual:
                return (double)CheckNumber(b.Operator, left)
                    <= (double)CheckNumber(b.Operator, right);
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return IsEqual(left, right);
        }
        return null;
    }

    private bool IsTruthy(object? obj)
    {
        return obj switch
        {
            null => false,
            bool b => b,
            double d => d != 0,
            string s => s.Length > 0,
            _ => true,
        };
    }

    private bool IsEqual(object? a, object? b)
    {
        return a == null ? b == null : a.Equals(b);
    }

    private object CheckNumber(Token op, object? operand)
    {
        return operand is double d
            ? d
            : throw new RuntimeException(op, "Operand must be a number.");
    }
}
