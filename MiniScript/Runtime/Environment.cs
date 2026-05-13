using MiniScript.Errors;
using MiniScript.Lexer;

namespace MiniScript.Runtime;

public class Environment(Environment? enclosing = null)
{
    public Environment? Enclosing { get; } = enclosing;
    private readonly Dictionary<string, object?> _values = [];

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }
        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
