using PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Console entry point the <c>tools/scripts/localization/generate-locale.script.ts</c>
/// wrapper invokes. The generator itself is in
/// <see cref="LocaleFileGenerator"/>; this file is just the CLI glue
/// so the script can stay a thin call site.
/// </summary>
internal static class Program
{
    private const int ExitOk = 0;
    private const int ExitDrift = 1;
    private const int ExitFailure = 2;

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            System.Console.Error.WriteLine("Usage: PCDynasty.LocaleGenerator <output-folder> [--check]");
            return ExitFailure;
        }

        string folder = args[0];
        bool check = args.Length > 1 && args[1] == "--check";
        LocaleFileGenerator generator = new(folder, dryRun: check, overwrite: !check);

        try
        {
            IReadOnlyList<string> written = generator.Generate(ShippedLocaleFiles.All);
            foreach (string path in written)
            {
                System.Console.WriteLine(path);
            }

            return ExitOk;
        }
        catch (InvalidOperationException exception) when (check)
        {
            System.Console.Error.WriteLine(exception.Message);
            return ExitDrift;
        }
        catch (Exception exception)
        {
            System.Console.Error.WriteLine(exception.Message);
            return ExitFailure;
        }
    }
}
