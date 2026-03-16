namespace CustomerTariffSwitch.Models;

public enum RequestStatus
{
    Approved,
    Rejected,
    Skipped
}

public class ProcessingResult
{
    public required string RequestId { get; init; }
    public required RequestStatus Status { get; init; }
    public string? RejectionReason { get; init; }
    public string? CustomerId { get; init; }
    public string? TargetTariffId { get; init; }
    public DateTimeOffset? RequestedAt { get; init; }
    public DateTimeOffset? SlaDueDate { get; init; }
    public string? FollowUpAction { get; init; }
    public DateTimeOffset ProcessedAt { get; init; } = DateTimeOffset.UtcNow;
}
