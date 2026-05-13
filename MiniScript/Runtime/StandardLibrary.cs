namespace MiniScript.Runtime;

public static class StandardLibrary
{
    public static void Inject(Interpreter interpreter)
    {
        // ----------------------------------------------------
        // MATH LIBRARY
        // ----------------------------------------------------
        Dictionary<object, object?> mathLib = new()
        {
            ["pi"] = Math.PI,
            ["e"] = Math.E,
            ["abs"] = new BuiltinFunction(1, args => Math.Abs(Convert.ToDouble(args[0]))),
            ["floor"] = new BuiltinFunction(1, args => Math.Floor(Convert.ToDouble(args[0]))),
            ["ceil"] = new BuiltinFunction(1, args => Math.Ceiling(Convert.ToDouble(args[0]))),
            ["round"] = new BuiltinFunction(1, args => Math.Round(Convert.ToDouble(args[0]))),
            ["sqrt"] = new BuiltinFunction(1, args => Math.Sqrt(Convert.ToDouble(args[0]))),
            ["pow"] = new BuiltinFunction(
                2,
                args => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))
            ),
            ["min"] = new BuiltinFunction(
                2,
                args => Math.Min(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))
            ),
            ["max"] = new BuiltinFunction(
                2,
                args => Math.Max(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))
            ),
        };
        interpreter.Globals.Define("math", mathLib);

        // ----------------------------------------------------
        // STRING LIBRARY
        // ----------------------------------------------------
        Dictionary<object, object?> stringLib = new()
        {
            ["to_upper"] = new BuiltinFunction(1, args => args[0]?.ToString()?.ToUpper()),
            ["to_lower"] = new BuiltinFunction(1, args => args[0]?.ToString()?.ToLower()),
            ["trim"] = new BuiltinFunction(1, args => args[0]?.ToString()?.Trim()),
            ["substring"] = new BuiltinFunction(
                3,
                args =>
                {
                    string str = args[0]?.ToString() ?? "";
                    int startIndex = Convert.ToInt32(args[1]);
                    int length = Convert.ToInt32(args[2]);

                    if (startIndex < 0 || startIndex >= str.Length)
                    {
                        return "";
                    }

                    if (startIndex + length > str.Length)
                    {
                        length = str.Length - startIndex;
                    }

                    return str.Substring(startIndex, length);
                }
            ),
            ["replace"] = new BuiltinFunction(
                3,
                args =>
                {
                    string str = args[0]?.ToString() ?? "";
                    string oldStr = args[1]?.ToString() ?? "";
                    string newStr = args[2]?.ToString() ?? "";
                    return str.Replace(oldStr, newStr);
                }
            ),
            ["contains"] = new BuiltinFunction(
                2,
                args =>
                {
                    string str = args[0]?.ToString() ?? "";
                    string searchStr = args[1]?.ToString() ?? "";
                    return str.Contains(searchStr);
                }
            ),
            ["split"] = new BuiltinFunction(
                2,
                args =>
                {
                    string str = args[0]?.ToString() ?? "";
                    string separator = args[1]?.ToString() ?? "";

                    // convert C# string array back to MiniScript List<object?>
                    return str.Split(separator).Cast<object?>().ToList();
                }
            ),
        };
        interpreter.Globals.Define("string", stringLib);
    }
}
