using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CustomerTariffSwitch.Data.Helpers;
using CustomerTariffSwitch.Models.Models;
using Microsoft.Extensions.Logging;

namespace CustomerTariffSwitch.Data.Services;

public class DecisionRepository
{
    private const string DecisionsFileName = "decisions.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ILogger<DecisionRepository> _logger;

    public DecisionRepository(ILogger<DecisionRepository> logger)
    {
        _logger = logger;
    }

    public string GetDecisionsFilePath()
    {
        return Path.Combine(SolutionPathHelper.GetOutputPath(), DecisionsFileName);
    }

    public HashSet<string> LoadProcessedRequestIds()
    {
        var filePath = GetDecisionsFilePath();

        if (!File.Exists(filePath))
            return new HashSet<string>(StringComparer.Ordinal);

        var json = File.ReadAllText(filePath, Encoding.UTF8);
        var decisions = JsonSerializer.Deserialize<List<RequestDecision>>(json, JsonOptions);

        if (decisions is null)
            return new HashSet<string>(StringComparer.Ordinal);

        return new HashSet<string>(decisions.Select(d => d.RequestId), StringComparer.Ordinal);
    }

    public IReadOnlyList<RequestDecision> LoadExistingDecisions()
    {
        var filePath = GetDecisionsFilePath();

        if (!File.Exists(filePath))
            return [];

        var json = File.ReadAllText(filePath, Encoding.UTF8);
        return JsonSerializer.Deserialize<List<RequestDecision>>(json, JsonOptions) ?? [];
    }

    public void SaveDecisions(IReadOnlyList<RequestDecision> allDecisions)
    {
        var filePath = GetDecisionsFilePath();
        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);

        var tempFilePath = Path.Combine(directory, $"{DecisionsFileName}.{Guid.NewGuid():N}.tmp");

        try
        {
            var json = JsonSerializer.Serialize(allDecisions, JsonOptions);
            var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(tempFilePath, json, utf8NoBom);
            File.Move(tempFilePath, filePath, overwrite: true);

            _logger.LogInformation("  Decisions written to: {Path}", filePath);
        }
        catch
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            throw;
        }
    }
}
