using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using ConsoleWrapper _ = new ConsoleWrapper();
string tree = """

                  *
                 / \
                /  O\
               /     \
               / O   \
              /      +\
             /+   O    \
             /      +  \
            /   O       \
           /+        O   \
          /---------------\
                 | |
                 | |
                 ___

        He's making a List<T>
        He's checking it twice
     people.Where(p => p.IsNice())
      C# Clause is coming to town

    """;

AddCheer();
Console.WriteLine(tree);
Console.ReadLine();

void AddCheer()
{
    int cVar = 0x96dbfe;
    int cMethod = 0xdcdca6;
    int cKeyword = 0x4894b4;
    int cType = 0x4ec9b0;
    int cPine = 0x3c992c;
    int cWood = 0x804f3c;
    int cStar = 0xf0d437;
    int cSharp = 0x8165e1;

    int[] cBaubles = [0xa71719, 0x8045d0, 0x0c55ed, 0xdcb91b, 0x56cb90];
    int[] cLights = [0x2cdbd0, 0xe667c8, 0xcf331b, 0x1144ed];

    tree = tree
        .AddColor("*", cStar)
        .AddColor("O", cBaubles)
        .AddColor("+", cLights, true)
        .AddColor("/", cPine)
        .AddColor("\\", cPine)
        .AddColor("-", cPine)
        .AddColor("|", cWood)
        .AddColor("_", cWood)
        .AddColor("C#", cSharp)
        .AddColor("p", cVar)
        .AddColor("eo", cVar)
        .AddColor("le", cVar)
        .AddColor("Where", cMethod)
        .AddColor("List", cType)
        .AddColor("T", cKeyword)
        .AddColor("IsNice", cMethod);
}

internal static class Extensions
{
    /// <summary>
    /// Adds color to the string.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <param name="rgb"></param>
    /// <param name="blinking"></param>
    /// <returns></returns>
    public static string AddColor(this string str, string value, int rgb, bool blinking = false)
    {
        return Coloring.Color(str, value, rgb, blinking);
    }

    /// <summary>
    /// Adds colors to the string.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <param name="rgb"></param>
    /// <param name="blinking"></param>
    /// <returns></returns>
    public static string AddColor(this string str, string value, int[] rgb, bool blinking = false)
    {
        return Coloring.Color(str, value, rgb, blinking);
    }
}

/// <summary>
/// Contains logic for adding color sequences to strings.
/// </summary>
internal static class Coloring
{
    const char escape = (char)27;
    private static readonly string resetModes = $"{escape}[m";

    /// <summary>
    /// Wraps <paramref name="value"/> in <paramref name="str"/> with the specified <paramref name="rgb"/> color, and optionally enables blinking mode.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <param name="rgb"></param>
    /// <param name="blinking"></param>
    /// <returns></returns>
    public static string Color(string str, string value, int rgb, bool blinking = false)
    {
        string sequence = GetSequence(rgb, blinking);
        return str.Replace(value, sequence + value + resetModes);
    }

    /// <summary>
    /// Wraps <paramref name="value"/> in <paramref name="str"/> with the specified <paramref name="rgb"/> colors, and optionally enables blinking mode.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <param name="rgb"></param>
    /// <param name="blinking"></param>
    /// <returns></returns>
    public static string Color(string str, string value, int[] rgb, bool blinking = false)
    {
        int index = 0;
        return Regex.Replace(str, Regex.Escape(value), x => GetSequence(rgb[index++], blinking) + value + resetModes);
    }

    /// <summary>
    /// Returns a sequence that sets the foreground color to <paramref name="rgb"/>, and optionally enables blinking mode (not supported on ConHost).
    /// </summary>
    /// <param name="rgb"></param>
    /// <param name="blinking"></param>
    /// <returns></returns>
    private static string GetSequence(int rgb, bool blinking)
    {
        byte r = (byte)(rgb >> 16);
        byte g = (byte)(rgb >> 8);
        byte b = (byte)rgb;

        string sequence = $"{escape}[38;2;{r};{g};{b}m";
        if (blinking)
        {
            sequence += $"{escape}[5m";
        }
        return sequence;
    }
}

/// <summary>
/// Ensures the Windows console mode is set and reset.
/// </summary>
internal class ConsoleWrapper : IDisposable
{
    private readonly uint originalConsoleMode;

    public ConsoleWrapper()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            originalConsoleMode = Native.SetConsoleMode();
        }
    }

    public void Dispose()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            Native.ResetConsoleMode(originalConsoleMode);
        }
    }
}

/// <summary>
/// P/Invoke for Windows, to toggle console sequence processing.
/// </summary>
internal static partial class Native
{
    private const string kernel32 = "kernel32.dll";
    private const int STD_OUTPUT_HANDLE = -11;
    private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [LibraryImport(kernel32, EntryPoint = "GetStdHandle", SetLastError = false)]
    private static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport(kernel32, EntryPoint = "GetConsoleMode", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [LibraryImport(kernel32, EntryPoint = "SetConsoleMode", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    /// <summary>
    /// Enables sequence processing (necessary if ConHost is being used).
    /// </summary>
    public static uint SetConsoleMode()
    {
        nint outputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        GetConsoleMode(outputHandle, out uint originalConsoleModes);
        SetConsoleMode(outputHandle, originalConsoleModes | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        return originalConsoleModes;
    }

    /// <summary>
    /// Sets the console mode to <paramref name="originalConsoleModes"/>.
    /// </summary>
    public static void ResetConsoleMode(uint originalConsoleModes)
    {
        nint outputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        SetConsoleMode(outputHandle, originalConsoleModes);
    }
}
