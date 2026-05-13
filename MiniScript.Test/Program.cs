using MiniScript.Lexer;
using MiniScript.Parser;
using MiniScript.Runtime;

namespace MiniScript.Test;

public class Program
{
    private static readonly Interpreter _interpreter = new();

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: MiniScript.exe script.ms");
            System.Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string path)
    {
        string source = File.ReadAllText(path);
        Run(source);
    }

    private static void RunPrompt()
    {
        Console.WriteLine("MiniScript REPL");
        Console.WriteLine(
            "Note: Single-line statements only in REPL. Blocks (if/func/while) require file execution."
        );

        while (true)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null)
            {
                break;
            }

            Run(line);
        }
    }

    private static void Run(string source)
    {
        try
        {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();

            Parser.Parser parser = new(tokens);
            List<Stmt> statements = parser.Parse();

            _interpreter.Interpret(statements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
