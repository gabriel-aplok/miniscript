namespace MiniScript.Lexer;

public record Token(TokenType Type, string Lexeme, object? Literal, int Line, int Column)
{
    public override string ToString()
    {
        return $"{Type} '{Lexeme}' {Literal}";
    }
}
