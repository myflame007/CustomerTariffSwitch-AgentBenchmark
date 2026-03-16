using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Data;

public class DecisionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;

    public DecisionRepository(string? overridePath = null)
    {
        _filePath = overridePath ?? Path.Combine(FindProjectRoot(), "Output", "decisions.json");
    }

    public HashSet<string> LoadProcessedRequestIds()
    {
        var decisions = LoadDecisions();
        return new HashSet<string>(
            decisions.Select(d => d.RequestId),
            StringComparer.OrdinalIgnoreCase);
    }

    public void AppendDecisions(IReadOnlyCollection<RequestDecision> newDecisions)
    {
        var existing = LoadDecisions();
        var existingIds = new HashSet<string>(
            existing.Select(d => d.RequestId),
            StringComparer.OrdinalIgnoreCase);

        var toAdd = newDecisions.Where(d => !existingIds.Contains(d.RequestId)).ToList();
        if (toAdd.Count == 0 && existing.Count > 0)
            return;

        existing.AddRange(toAdd);

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var tmpPath = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(existing, JsonOptions);
        File.WriteAllText(tmpPath, json);
        File.Move(tmpPath, _filePath, overwrite: true);
    }

    private List<RequestDecision> LoadDecisions()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<RequestDecision>>(json, JsonOptions) ?? [];
    }

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (dir.GetFiles("*.slnx").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
