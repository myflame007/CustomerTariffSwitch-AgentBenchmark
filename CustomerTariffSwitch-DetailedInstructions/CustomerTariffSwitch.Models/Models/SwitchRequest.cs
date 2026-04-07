namespace CustomerTariffSwitch.Models.Models;

public record SwitchRequest
{
    public required string RequestId { get; init; }
    public required string CustomerId { get; init; }
    public required string TargetTariffId { get; init; }
    public required DateTimeOffset RequestedAt { get; init; }
}
