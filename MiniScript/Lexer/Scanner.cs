using System.Globalization;
using MiniScript.Errors;

namespace MiniScript.Lexer;

public class Scanner(string source)
{
    private readonly List<Token> _tokens = [];
    private readonly Stack<int> _indentStack = new([0]);
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;
    private int _column = 1;

    private static readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "and", TokenType.And },
        { "or", TokenType.Or },
        { "not", TokenType.Not },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "for", TokenType.For },
        { "in", TokenType.In },
        { "while", TokenType.While },
        { "func", TokenType.Func },
        { "return", TokenType.Return },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "null", TokenType.Null },
        { "var", TokenType.Var },
        { "try", TokenType.Try },
        { "catch", TokenType.Catch },
        { "finally", TokenType.Finally },
        { "throw", TokenType.Throw },
        { "import", TokenType.Import },
    };

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        // emit remaining dedents
        while (_indentStack.Count > 1)
        {
            _indentStack.Pop();
            _tokens.Add(new Token(TokenType.Dedent, "", null, _line, _column));
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line, _column));
        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '%':
                AddToken(TokenType.Modulo);
                break;
            case ':':
                AddToken(TokenType.Colon);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case '[':
                AddToken(TokenType.LeftBracket);
                break;
            case ']':
                AddToken(TokenType.RightBracket);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                }
                else
                {
                    AddToken(TokenType.Slash);
                }

                break;
            case '#':
                while (Peek() != '\n' && !IsAtEnd())
                {
                    Advance(); // comments
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                break; // ignore inline whitespace
            case '\n':
                HandleNewline();
                break;
            case '"':
                LexString('"');
                break;
            case '\'':
                LexString('\'');
                break;
            default:
                if (char.IsDigit(c))
                {
                    LexNumber();
                }
                else if (char.IsLetter(c) || c == '_')
                {
                    LexIdentifier();
                }
                else
                {
                    throw new LexerException(_line, _column, $"Unexpected character '{c}'");
                }

                break;
        }
    }

    private void HandleNewline()
    {
        AddToken(TokenType.Newline);
        _line++;
        _column = 1;

        // count spaces for indentation
        // TODO: support for tabs and other whitespace
        int spaces = 0;
        while (Peek() == ' ')
        {
            spaces++;
            Advance();
        }

        // skip the empty lines or comment lines lol
        if (Peek() == '\n' || Peek() == '\r' || Peek() == '#')
        {
            return;
        }

        int currentIndent = _indentStack.Peek();

        if (spaces > currentIndent)
        {
            _indentStack.Push(spaces);
            AddToken(TokenType.Indent);
        }
        else if (spaces < currentIndent)
        {
            while (_indentStack.Peek() > spaces)
            {
                _indentStack.Pop();
                AddToken(TokenType.Dedent);
            }
            if (_indentStack.Peek() != spaces)
            {
                throw new LexerException(_line, _column, "Inconsistent indentation");
            }
        }
    }

    private void LexString(char quoteType)
    {
        while (Peek() != quoteType && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
                _column = 1;
            }
            Advance();
        }

        if (IsAtEnd())
        {
            throw new LexerException(_line, _column, "Unterminated string");
        }

        Advance(); // the closing "
        string value = source[(_start + 1)..(_current - 1)];
        AddToken(TokenType.String, value);
    }

    private void LexNumber()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();
            while (char.IsDigit(Peek()))
            {
                Advance();
            }
        }
        double value = double.Parse(source[_start.._current], CultureInfo.InvariantCulture);
        AddToken(TokenType.Number, value);
    }

    private void LexIdentifier()
    {
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            Advance();
        }

        string text = source[_start.._current];
        TokenType type = _keywords.GetValueOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }

    private char Advance()
    {
        _column++;
        return source[_current++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || source[_current] != expected)
        {
            return false;
        }

        _current++;
        _column++;
        return true;
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : source[_current];
    }

    private char PeekNext()
    {
        return _current + 1 >= source.Length ? '\0' : source[_current + 1];
    }

    private bool IsAtEnd()
    {
        return _current >= source.Length;
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        _tokens.Add(new Token(type, source[_start.._current], literal, _line, _column));
    }
}
