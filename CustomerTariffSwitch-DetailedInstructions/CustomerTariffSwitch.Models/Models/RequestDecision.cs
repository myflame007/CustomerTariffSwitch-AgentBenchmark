using System.Text.Json.Serialization;
using CustomerTariffSwitch.Models.Enums;

namespace CustomerTariffSwitch.Models.Models;

public record RequestDecision
{
    public required string RequestId { get; init; }

    public required DecisionStatus Status { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustomerName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DueAt { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FollowUpAction { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FollowUpDueAt { get; init; }
}
