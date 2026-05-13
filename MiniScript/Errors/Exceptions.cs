namespace MiniScript.Errors;

public class MiniScriptException(int line, int column, string message) : Exception(message)
{
    public int Line { get; } = line;
    public int Column { get; } = column;
}

public class LexerException(int line, int column, string message)
    : MiniScriptException(line, column, message);

public class ParserException(Lexer.Token token, string message)
    : MiniScriptException(token.Line, token.Column, message)
{
    public Lexer.Token Token { get; } = token;
}

public class RuntimeException(Lexer.Token token, string message)
    : MiniScriptException(token.Line, token.Column, message)
{
    public Lexer.Token Token { get; } = token;
}

public class ReturnException(object? value) : Exception
{
    public object? Value { get; } = value;
}
