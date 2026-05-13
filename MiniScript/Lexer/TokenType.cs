namespace MiniScript.Lexer;

public enum TokenType
{
    // single-character
    Dot,
    LeftParen,
    RightParen,
    Comma,
    Minus,
    Plus,
    Star,
    Slash,
    Modulo,
    Colon,
    LeftBrace,
    RightBrace,
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
    For,
    In,
    Func,
    Return,
    True,
    False,
    Null,
    Var,
    Try,
    Catch,

    // formatting & structure
    Indent,
    Dedent,
    Newline,
    EOF,

    // Special
    Import,
}
