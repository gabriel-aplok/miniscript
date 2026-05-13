using MiniScript.Errors;
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
            // just to test lol RunPrompt();
            RunFile("samples/test.ms");
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
        catch (MiniScriptException ex)
        {
            ReportError(source, ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected System Error: {ex.Message}");
        }
    }

    private static void ReportError(string source, MiniScriptException ex)
    {
        string errorType = ex switch
        {
            LexerException => "Lexer Error",
            ParserException => "Parser Error",
            RuntimeException => "Runtime Error",
            _ => "Error",
        };

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[{errorType}] Line {ex.Line}:{ex.Column} -> {ex.Message}");
        Console.ResetColor();

        // split source to find the specific line text
        string[] lines = source.Split('\n');
        if (ex.Line <= lines.Length)
        {
            // replace carriage returns for clean printing
            string errorLine = lines[ex.Line - 1].Replace("\r", "");
            Console.WriteLine(errorLine);

            // create the pointer string: spaces up to the column, then the pointer
            // column is 1-based, so I need to use (column - 1) spaces
            string pointer = new string(' ', ex.Column - 1) + "^";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(pointer);
            Console.ResetColor();
        }
        Console.WriteLine();
    }
}
