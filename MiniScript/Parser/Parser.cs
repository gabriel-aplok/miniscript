using MiniScript.Lexer;

namespace MiniScript.Parser;

public class Parser(List<Token> tokens)
{
    private int _current = 0;

    public List<Stmt> Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
        {
            // skip unnecessary newlines at top level
            if (Match(TokenType.Newline))
            {
                continue;
            }

            statements.Add(Declaration());
        }
        return statements;
    }

    private Stmt Declaration()
    {
        if (Match(TokenType.Func))
        {
            return FunctionDeclaration();
        }

        if (Match(TokenType.Var))
        {
            return VarDeclaration();
        }

        return Statement();
    }

    private Stmt FunctionDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expect function name.");
        Consume(TokenType.LeftParen, "Expect '(' after function name.");
        List<Token> parameters = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        Consume(TokenType.Colon, "Expect ':' before function body.");
        Consume(TokenType.Newline, "Expect newline after ':'.");

        Stmt body = Block();
        return new FunctionStmt(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expect variable name.");
        Expr? initializer = null;
        if (Match(TokenType.Equal))
        {
            initializer = Expression();
        }

        ConsumeNewlineOrEOF("Expect newline after variable declaration.");
        return new VarStmt(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.If))
        {
            return IfStatement();
        }

        if (Match(TokenType.While))
        {
            return WhileStatement();
        }

        if (Match(TokenType.Return))
        {
            return ReturnStatement();
        }

        return ExpressionStatement();
    }

    private Stmt IfStatement()
    {
        Expr condition = Expression();
        Consume(TokenType.Colon, "Expect ':' after if condition.");
        Consume(TokenType.Newline, "Expect newline after ':'.");

        Stmt thenBranch = Block();
        Stmt? elseBranch = null;

        if (Match(TokenType.Else))
        {
            Consume(TokenType.Colon, "Expect ':' after else.");
            Consume(TokenType.Newline, "Expect newline after ':'.");
            elseBranch = Block();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private Stmt WhileStatement()
    {
        Expr condition = Expression();
        Consume(TokenType.Colon, "Expect ':' after while condition.");
        Consume(TokenType.Newline, "Expect newline after ':'.");
        Stmt body = Block();
        return new WhileStmt(condition, body);
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;
        if (!Check(TokenType.Newline) && !IsAtEnd())
        {
            value = Expression();
        }

        ConsumeNewlineOrEOF("Expect newline after return value.");
        return new ReturnStmt(keyword, value);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        ConsumeNewlineOrEOF("Expect newline after expression.");
        return new ExpressionStmt(expr);
    }

    private Stmt Block()
    {
        Consume(TokenType.Indent, "Expect indentation before block.");
        List<Stmt> statements = [];
        while (!Check(TokenType.Dedent) && !IsAtEnd())
        {
            if (Match(TokenType.Newline))
            {
                continue;
            }

            statements.Add(Declaration());
        }
        Consume(TokenType.Dedent, "Expect dedent after block.");
        return new BlockStmt(statements);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Equality();
        if (Match(TokenType.Equal))
        {
            Token equals = Previous();
            Expr value = Assignment();
            if (expr is VariableExpr v)
            {
                return new AssignExpr(v.Name, value);
            }

            throw new Exception($"Parser error: Invalid assignment target at line {equals.Line}");
        }
        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();
        while (Match(TokenType.EqualEqual, TokenType.BangEqual))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();
        while (
            Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual)
        )
        {
            Token op = Previous();
            Expr right = Term();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();
        while (Match(TokenType.Minus, TokenType.Plus))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();
        while (Match(TokenType.Slash, TokenType.Star, TokenType.Modulo))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            Token op = Previous();
            Expr right = Unary();
            return new UnaryExpr(op, right);
        }
        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();
        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }
        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }
        Token paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");
        return new CallExpr(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False))
        {
            return new LiteralExpr(false);
        }

        if (Match(TokenType.True))
        {
            return new LiteralExpr(true);
        }

        if (Match(TokenType.Null))
        {
            return new LiteralExpr(null);
        }

        if (Match(TokenType.Number, TokenType.String))
        {
            return new LiteralExpr(Previous().Literal);
        }

        if (Match(TokenType.Identifier))
        {
            return new VariableExpr(Previous());
        }

        if (Match(TokenType.LeftParen))
        {
            Expr expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return expr;
        }
        throw new Exception($"Parser error at line {Peek().Line}: Unexpected token {Peek().Type}");
    }

    // Helpers
    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private void ConsumeNewlineOrEOF(string message)
    {
        if (Check(TokenType.Newline) || Check(TokenType.EOF))
        {
            Advance();
        }
        else
        {
            throw new Exception($"Parser error: {message}");
        }
    }

    private Token Consume(TokenType type, string message)
    {
        return Check(type)
            ? Advance()
            : throw new Exception($"Parser error at line {Peek().Line}: {message}");
    }

    private bool Check(TokenType type)
    {
        return IsAtEnd() ? false : Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens[_current];
    }

    private Token Previous()
    {
        return tokens[_current - 1];
    }
}
