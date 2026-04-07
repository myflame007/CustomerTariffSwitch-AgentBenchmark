namespace CustomerTariffSwitch.Models.Models;

public record SlaDeadline
{
    public required DateTimeOffset DueAt { get; init; }
    public DateTimeOffset? FollowUpDueAt { get; init; }
    public string? FollowUpAction { get; init; }
}
