namespace CustomerTariffSwitch.Models;

public record Customer
{
    public string CustomerId { get; init; } = "";
    public string Name { get; init; } = "";
    public bool HasUnpaidInvoice { get; init; }
    public string Sla { get; init; } = "";
    public string MeterType { get; init; } = "";

    public bool IsPremium => Sla.Equals("Premium", StringComparison.OrdinalIgnoreCase);
    public bool HasSmartMeter => MeterType.Equals("Smart", StringComparison.OrdinalIgnoreCase);
}
