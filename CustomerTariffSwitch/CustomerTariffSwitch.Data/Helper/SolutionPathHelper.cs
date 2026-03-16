namespace CustomerTariffSwitch.Data.Helper;

internal static class SolutionPathHelper
{
    // Walks up from AppContext.BaseDirectory until it finds a directory that
    // contains the given markerFolderName, then returns that parent directory
    internal static string FindRootByMarker(string markerFolderName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, markerFolderName)))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not find a directory containing '{markerFolderName}' folder by walking up from '{AppContext.BaseDirectory}'.");
    }

    // Walks up from AppContext.BaseDirectory until it finds a directory that
    // contains a file matching the given pattern (e.g. "*.sln"), then returns that directory
    internal static string FindRootByFilePattern(string filePattern)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (current.GetFiles(filePattern).Length > 0)
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not find a directory containing '{filePattern}' by walking up from '{AppContext.BaseDirectory}'.");
    }
}
