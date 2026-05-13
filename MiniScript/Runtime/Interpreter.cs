using System.Text;
using MiniScript.Errors;
using MiniScript.Lexer;
using MiniScript.Parser;

namespace MiniScript.Runtime;

public class Interpreter
{
    public Environment Globals { get; } = new();
    private Environment _environment;

    private readonly HashSet<string> _importedFiles = [];
    private string _currentDirectory = Directory.GetCurrentDirectory();

    public Interpreter()
    {
        _environment = Globals;

        // Base Global Functions
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

        StandardLibrary.Inject(this);
    }

    public void Run(List<Stmt> statements, string? path = null)
    {
        _currentDirectory = Directory.GetCurrentDirectory();

        if (path != null)
        {
            _currentDirectory =
                Path.GetDirectoryName(Path.GetFullPath(path)) ?? Directory.GetCurrentDirectory();
        }

        foreach (Stmt stmt in statements)
        {
            Execute(stmt);
        }
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
            case ForStmt f:
                ExecuteFor(f);
                break;
            case ImportStmt i:
                ExecuteImport(i);
                break;
            case TryStmt t:
                ExecuteTry(t);
                break;
        }
    }

    private void ExecuteImport(ImportStmt stmt)
    {
        // resolve the path
        string cleanPath = stmt.Path.Trim('"');
        string fullPath = Path.GetFullPath(Path.Combine(_currentDirectory, cleanPath));
        fullPath = Path.GetFullPath(fullPath);

        if (!File.Exists(fullPath))
        {
            throw new RuntimeException(
                stmt.Keyword,
                $"the file {stmt.Path} could not be found in {fullPath}"
            );
        }

        if (_importedFiles.Contains(fullPath))
        {
            return;
        }

        _importedFiles.Add(fullPath);

        string source = File.ReadAllText(fullPath);

        // saves the previous directory for later restoration (in case the import contains other imports).
        string previousDir = _currentDirectory;
        _currentDirectory = Path.GetDirectoryName(fullPath)!;

        try
        {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser.Parser parser = new(tokens);
            List<Stmt> statements = parser.Parse();

            Environment previousEnv = _environment;
            _environment = Globals;

            try
            {
                foreach (Stmt s in statements)
                {
                    Execute(s);
                }
            }
            finally
            {
                _environment = previousEnv;
            }
        }
        finally
        {
            _currentDirectory = previousDir; // restore the original directory
        }
    }

    private void ExecuteTry(TryStmt stmt)
    {
        try
        {
            Execute(stmt.TryBlock);
        }
        catch (RuntimeException ex)
        {
            // creates a new scope for the catch block.
            Environment catchEnv = new(_environment);

            if (stmt.ErrorVar != null)
            {
                // define the error variable with the exception message.
                catchEnv.Define(stmt.ErrorVar.Lexeme, ex.Message);
            }

            Environment previous = _environment;
            try
            {
                _environment = catchEnv;
                Execute(stmt.CatchBlock);
            }
            finally
            {
                _environment = previous;
            }
        }
    }

    private void ExecuteFor(ForStmt stmt)
    {
        object? iterable = Evaluate(stmt.Iterable);

        if (iterable is List<object?> list)
        {
            foreach (object? item in list)
            {
                ExecuteForIteration(stmt.Variable, item, stmt.Body);
            }
        }
        else if (iterable is Dictionary<object, object?> dict)
        {
            // by default, iterating a dictionary gives you the keys
            foreach (object key in dict.Keys)
            {
                ExecuteForIteration(stmt.Variable, key, stmt.Body);
            }
        }
        else if (iterable is string str)
        {
            // iterating over a string gives you each character as a string
            foreach (char c in str)
            {
                ExecuteForIteration(stmt.Variable, c.ToString(), stmt.Body);
            }
        }
        else
        {
            // to get the token for the error, I could pass it in the AST,
            // but I can just use the variable token for position info
            throw new RuntimeException(
                stmt.Variable,
                "Target is not iterable. You can only iterate over arrays, dictionaries, and strings."
            );
        }
    }

    private void ExecuteForIteration(Token variable, object? value, Stmt body)
    {
        // Create a new environment for EACH iteration so the loop variable
        // doesn't leak into the global scope or get mixed up.
        Environment iterationEnv = new(_environment);
        iterationEnv.Define(variable.Lexeme, value);

        Environment previous = _environment;
        try
        {
            _environment = iterationEnv;
            Execute(body);
        }
        finally
        {
            _environment = previous;
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
            ArrayExpr a => EvaluateArray(a),
            IndexExpr i => EvaluateIndex(i),
            DictionaryExpr d => EvaluateDictionary(d),
            SetPropertyExpr s => EvaluateSetProperty(s),
            SetExpr s => EvaluateSet(s),
            GetExpr g => EvaluateGet(g),
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

    private object? EvaluateArray(ArrayExpr expr)
    {
        List<object?> elements = [];
        foreach (Expr element in expr.Elements)
        {
            elements.Add(Evaluate(element));
        }
        return elements;
    }

    private object? EvaluateIndex(IndexExpr expr)
    {
        object? callee = Evaluate(expr.Callee);
        object? index = Evaluate(expr.Index);

        if (callee is List<object?> list)
        {
            int i = (int)(double)index!;
            return list[i];
        }
        if (callee is Dictionary<object, object?> dict)
        {
            if (index != null && dict.TryGetValue(index, out object? value))
            {
                return value;
            }

            return null;
        }
        throw new RuntimeException(expr.Bracket, "Only arrays and dictionaries can be indexed.");
    }

    private object? EvaluateDictionary(DictionaryExpr expr)
    {
        Dictionary<object, object?> dict = [];
        foreach (KeyValuePair<Expr, Expr> entry in expr.Entries)
        {
#pragma warning disable CS8625
            object? key =
                Evaluate(entry.Key)
                ?? throw new RuntimeException(null, "Dictionary key cannot be null.");
#pragma warning restore CS8625
            dict[key] = Evaluate(entry.Value);
        }
        return dict;
    }

    private object? EvaluateSetProperty(SetPropertyExpr expr)
    {
        object? obj = Evaluate(expr.Object);

        if (obj is Dictionary<object, object?> dict)
        {
            object? value = Evaluate(expr.Value);
            dict[expr.Name.Lexeme] = value;
            return value;
        }

        throw new RuntimeException(
            expr.Name,
            "Only dictionariess support property assignment via dot notation."
        );
    }

    private object? EvaluateSet(SetExpr expr)
    {
        object? callee = Evaluate(expr.Callee);
        object? index = Evaluate(expr.Index);
        object? value = Evaluate(expr.Value);

        if (callee is List<object?> list)
        {
            int i = (int)(double)index!;
            list[i] = value;
            return value;
        }

        if (callee is Dictionary<object, object?> dict)
        {
            if (index == null)
            {
                throw new RuntimeException(expr.Bracket, "Key cannot be null.");
            }

            dict[index] = value;
            return value;
        }

        throw new RuntimeException(
            expr.Bracket,
            "Only arrays and dictionaries support index assignment."
        );
    }

    private object? EvaluateGet(GetExpr expr)
    {
        object? obj = Evaluate(expr.Object);

        // list methods
        if (obj is List<object?> list)
        {
            return expr.Name.Lexeme switch
            {
                "length" => (double)list.Count,
                "push" => new BuiltinFunction(
                    1,
                    args =>
                    {
                        list.Add(args[0]);
                        return null;
                    }
                ),
                "remove" => new BuiltinFunction(1, args => list.Remove(args[0])),
                "pop" => new BuiltinFunction(
                    0,
                    _ =>
                    {
                        object? last = list[^1];
                        list.RemoveAt(list.Count - 1);
                        return last;
                    }
                ),
                "clear" => new BuiltinFunction(
                    0,
                    _ =>
                    {
                        list.Clear();
                        return null;
                    }
                ),
                _ => throw new RuntimeException(
                    expr.Name,
                    $"List has no method '{expr.Name.Lexeme}'."
                ),
            };
        }

        // string methods
        if (obj is string str)
        {
            return expr.Name.Lexeme switch
            {
                "length" => (double)str.Length,
                "to_upper" => new BuiltinFunction(0, _ => str.ToUpper()),
                "to_lower" => new BuiltinFunction(0, _ => str.ToLower()),
                "trim" => new BuiltinFunction(0, _ => str.Trim()),
                "contains" => new BuiltinFunction(
                    1,
                    args => str.Contains(args[0]?.ToString() ?? "")
                ),
                "split" => new BuiltinFunction(
                    1,
                    args => str.Split(args[0]?.ToString() ?? "").Cast<object?>().ToList()
                ),
                "replace" => new BuiltinFunction(
                    2,
                    args => str.Replace(args[0]?.ToString() ?? "", args[1]?.ToString() ?? "")
                ),
                _ => throw new RuntimeException(
                    expr.Name,
                    $"String has no method '{expr.Name.Lexeme}'."
                ),
            };
        }

        // dictionary properties
        if (obj is Dictionary<object, object?> dict)
        {
            if (dict.TryGetValue(expr.Name.Lexeme, out object? value))
            {
                return value;
            }

            // dynamic method for dictionaries
            if (expr.Name.Lexeme == "keys")
            {
                return new BuiltinFunction(0, _ => dict.Keys.ToList<object?>());
            }

            throw new RuntimeException(expr.Name, $"Property '{expr.Name.Lexeme}' not found.");
        }

        throw new RuntimeException(
            expr.Name,
            "Only collections and strings support member access."
        );
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
