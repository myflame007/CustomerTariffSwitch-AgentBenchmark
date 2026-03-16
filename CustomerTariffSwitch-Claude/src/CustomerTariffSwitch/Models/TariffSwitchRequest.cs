namespace CustomerTariffSwitch.Models;

public record TariffSwitchRequest
{
    public string RequestId { get; init; } = "";
    public string CustomerId { get; init; } = "";
    public string TargetTariffId { get; init; } = "";
    public DateTimeOffset RequestedAt { get; init; }
}
