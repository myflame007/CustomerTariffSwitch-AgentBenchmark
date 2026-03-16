using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Services;

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

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (dir.GetFiles("*.slnx").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    public HashSet<string> LoadProcessedRequestIds()
    {
        var decisions = LoadDecisions();
        return new HashSet<string>(decisions.Select(d => d.RequestId), StringComparer.OrdinalIgnoreCase);
    }

    public void AppendDecisions(List<RequestDecision> decisions)
    {
        var existing = LoadDecisions();
        var existingIds = new HashSet<string>(existing.Select(d => d.RequestId), StringComparer.OrdinalIgnoreCase);

        var newDecisions = decisions.Where(d => !existingIds.Contains(d.RequestId)).ToList();
        if (newDecisions.Count == 0) return;

        existing.AddRange(newDecisions);

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
}
