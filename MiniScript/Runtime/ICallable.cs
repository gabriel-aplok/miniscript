namespace MiniScript.Runtime;

public interface ICallable
{
    public int Arity { get; }
    public object? Call(Interpreter interpreter, List<object?> arguments);
}

// built-in function wrapper
public class BuiltinFunction(int arity, Func<List<object?>, object?> func) : ICallable
{
    public int Arity { get; } = arity;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        return func(arguments);
    }
}

// user-defined function wrapper
public class MiniScriptFunction(Parser.FunctionStmt declaration, Environment closure) : ICallable
{
    public int Arity => declaration.Params.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment env = new(closure);
        for (int i = 0; i < declaration.Params.Count; i++)
        {
            env.Define(declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
#pragma warning disable CS8604 // Possible null reference argument.
            interpreter.ExecuteBlock(declaration.Body as Parser.BlockStmt, env);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        catch (Errors.ReturnException returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }
}
