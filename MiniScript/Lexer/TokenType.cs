namespace MiniScript.Lexer;

public enum TokenType
{
    // single-character
    LeftParen,
    RightParen,
    Comma,
    Minus,
    Plus,
    Star,
    Slash,
    Modulo,
    Colon,
    LeftBracket,
    RightBracket,

    // one or two character
    Bang,
    BangEqual,
    Equal,
    EqualEqual,
    Greater,
    GreaterEqual,
    Less,
    LessEqual,

    // literals
    Identifier,
    String,
    Number,

    // keywords
    And,
    Or,
    Not,
    If,
    Else,
    While,
    Func,
    Return,
    True,
    False,
    Null,
    Var,

    // formatting & structure
    Indent,
    Dedent,
    Newline,
    EOF,
}
