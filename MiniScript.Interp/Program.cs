using System.Text;
using MiniScript.Errors;
using MiniScript.Lexer;
using MiniScript.Parser;
using MiniScript.Runtime;

namespace MiniScript.Interp;

public class Program
{
    private static readonly Interpreter _interpreter = new();
    private static bool _hadError = false;
    private static bool _hadRuntimeError = false;

    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length == 0)
        {
            if (File.Exists("samples/test.ms"))
            {
                RunFile("samples/test.ms", []);
            }
            else
            {
                RunPrompt();
            }
            return 0;
        }

        string? filePath = null;
        bool startRepl = false;
        List<string> scriptArgs = [];

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-r":
                case "--repl":
                    startRepl = true;
                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    if (!arg.StartsWith('-') && filePath == null)
                    {
                        filePath = arg;
                        scriptArgs = [.. args.Skip(i + 1)];
                        i = args.Length;
                    }
                    break;
            }
        }

        if (startRepl)
        {
            RunPrompt();
        }
        else if (filePath != null)
        {
            RunFile(filePath, scriptArgs);
        }
        else
        {
            Console.Error.WriteLine("Error: No script file provided.");
            PrintUsage();
            return 64;
        }

        return _hadError ? 65 : (_hadRuntimeError ? 70 : 0);
    }

    private static void RunFile(string path, List<string> args)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Error: File not found at '{path}'");
            return;
        }

        try
        {
            _interpreter.Arguments = args;
            string source = File.ReadAllText(path);
            Run(source, path);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"Error reading file: {e.Message}");
        }
    }

    private static void RunPrompt()
    {
        Console.Clear();
        Console.WriteLine("MiniScript REPL (Version 1.0)");
        Console.WriteLine("Type 'exit()' or press Ctrl+C to quit.");
        Console.WriteLine("-------------------------------------");

        _interpreter.Arguments = [];

        while (true)
        {
            _hadError = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("ms > ");
            Console.ResetColor();

            string? line = Console.ReadLine();
            if (line == null || line.Trim() == "exit()")
            {
                break;
            }

            Run(line, "repl");
        }
    }

    private static void Run(string source, string? path = null)
    {
        try
        {
            // lexing
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();

            // parsing
            Parser.Parser parser = new(tokens);
            List<Stmt> statements = parser.Parse();

            // stop if there was a syntax error
            if (_hadError)
            {
                return;
            }

            // execution
            _interpreter.Run(statements, path);
        }
        catch (RuntimeException ex)
        {
            ReportError(source, ex);
            _hadRuntimeError = true;
        }
        catch (MiniScriptException ex)
        {
            ReportError(source, ex);
            _hadError = true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n[Internal System Error]: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("MiniScript Language Tool");
        Console.WriteLine("Usage: MiniScript [script.ms] [options]");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  -r, --repl    Start interactive mode");
        Console.WriteLine("  -h, --help    Show this help message");
    }

    private static void ReportError(string source, MiniScriptException ex)
    {
        string errorType = ex switch
        {
            LexerException => "Lexical Error",
            ParserException => "Syntax Error",
            RuntimeException => "Runtime Error",
            _ => "Error",
        };

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[{errorType}] {ex.Message}");
        Console.ResetColor();

        string[] lines = source.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        if (ex.Line > 0 && ex.Line <= lines.Length)
        {
            string errorLine = lines[ex.Line - 1];
            Console.WriteLine($"  Line {ex.Line}: {errorLine}");

            // caret pointer
            // handle tab characters by replicating them in the pointer string
            StringBuilder pointer = new StringBuilder("          "); // offset for "Line X: "
            for (int i = 0; i < ex.Column - 1; i++)
            {
                pointer.Append(errorLine[i] == '\t' ? '\t' : ' ');
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(pointer.ToString() + "^--- here");
            Console.ResetColor();
        }
    }
}
