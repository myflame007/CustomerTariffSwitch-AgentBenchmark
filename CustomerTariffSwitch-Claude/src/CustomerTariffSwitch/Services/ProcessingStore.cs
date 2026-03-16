using System.Text.Json;
using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Services;

/// <summary>
/// Persists processing results to a JSON file and tracks which request IDs
/// have already been processed, enabling idempotent runs.
/// </summary>
public sealed class ProcessingStore
{
    private readonly string _resultsPath;
    private readonly List<ProcessingResult> _results;
    private readonly HashSet<string> _processedIds;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ProcessingStore(string resultsPath)
    {
        _resultsPath = resultsPath;

        if (File.Exists(resultsPath))
        {
            var json = File.ReadAllText(resultsPath);
            _results = JsonSerializer.Deserialize<List<ProcessingResult>>(json, JsonOptions) ?? [];
        }
        else
        {
            _results = [];
        }

        _processedIds = new HashSet<string>(
            _results.Select(r => r.RequestId), StringComparer.Ordinal);
    }

    public bool IsProcessed(string requestId) => _processedIds.Contains(requestId);

    public void Add(ProcessingResult result)
    {
        _results.Add(result);
        _processedIds.Add(result.RequestId);
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_resultsPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_results, JsonOptions);
        File.WriteAllText(_resultsPath, json);
    }

    public IReadOnlyList<ProcessingResult> Results => _results.AsReadOnly();
}
