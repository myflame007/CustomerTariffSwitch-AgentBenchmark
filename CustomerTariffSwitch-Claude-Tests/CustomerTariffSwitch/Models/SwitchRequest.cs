namespace CustomerTariffSwitch.Models;

public class SwitchRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string TargetTariffId { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
}
