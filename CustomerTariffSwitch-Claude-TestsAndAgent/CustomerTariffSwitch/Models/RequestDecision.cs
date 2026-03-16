namespace CustomerTariffSwitch.Models;

public class RequestDecision
{
    public string RequestId { get; set; } = string.Empty;
    public DecisionStatus Status { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public string? CustomerName { get; set; }
    public string? FollowUpAction { get; set; }

    public static RequestDecision Approved(string requestId, string customerName, DateTimeOffset dueAt, string? followUpAction = null)
    {
        return new RequestDecision
        {
            RequestId = requestId,
            Status = DecisionStatus.Approved,
            CustomerName = customerName,
            DueAt = dueAt,
            FollowUpAction = followUpAction
        };
    }

    public static RequestDecision Rejected(string requestId, string reason)
    {
        return new RequestDecision
        {
            RequestId = requestId,
            Status = DecisionStatus.Rejected,
            Reason = reason
        };
    }
}
