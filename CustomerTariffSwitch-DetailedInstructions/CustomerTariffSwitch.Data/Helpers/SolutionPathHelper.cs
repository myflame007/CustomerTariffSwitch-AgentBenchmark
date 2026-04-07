namespace CustomerTariffSwitch.Data.Helpers;

public static class SolutionPathHelper
{
    private static readonly string[] SolutionFileNames = ["CustomerTariffSwitch.sln", "CustomerTariffSwitch.slnx"];
    private const string InputFolderName = "Input Files";
    private const string OutputFolderName = "Output";

    public static string GetSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (SolutionFileNames.Any(name => File.Exists(Path.Combine(directory.FullName, name))))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find solution file in any parent directory of '{AppContext.BaseDirectory}'.");
    }

    public static string GetInputFilesPath()
    {
        return Path.Combine(GetSolutionRoot(), InputFolderName);
    }

    public static string GetOutputPath()
    {
        var outputPath = Path.Combine(GetSolutionRoot(), OutputFolderName);
        Directory.CreateDirectory(outputPath);
        return outputPath;
    }
}
