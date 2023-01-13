using System.CodeDom.Compiler;
using Pegasus.Compiler;
using Pegasus.Properties;

namespace Pegasus.Console;

internal class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return -1;
        }

        var errorCount = 0;
        foreach (var arg in args)
        {
            var errors = new List<CompilerError>();
            CompileManager.CompileFile(arg, null, errors.Add);
            ShowErrors(errors);
            errorCount += errors.Count;
        }

        return errorCount;
    }

    private static void ShowErrors(List<CompilerError> errors)
    {
        var startingColor = System.Console.ForegroundColor;

        foreach (var e in errors)
        {
            System.Console.ForegroundColor = e.IsWarning ? ConsoleColor.Yellow : ConsoleColor.Red;
            System.Console.WriteLine(e);
        }

        System.Console.ForegroundColor = startingColor;
    }

    private static void ShowUsage()
    {
        System.Console.WriteLine(Pegasus.Console.Properties.Resources.Usage);
    }
}
