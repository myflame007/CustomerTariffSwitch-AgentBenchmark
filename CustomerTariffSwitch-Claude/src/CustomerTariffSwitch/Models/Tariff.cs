namespace CustomerTariffSwitch.Models;

public record Tariff
{
    public string TariffId { get; init; } = "";
    public string Name { get; init; } = "";
    public bool RequiresSmartMeter { get; init; }
    public decimal BaseMonthlyGross { get; init; }
}
