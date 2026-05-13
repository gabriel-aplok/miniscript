namespace MiniScript.Errors;

public class RuntimeException(Lexer.Token token, string message) : Exception(message)
{
    public Lexer.Token Token { get; } = token;
}

public class ReturnException(object? value) : Exception
{
    public object? Value { get; } = value;
}
