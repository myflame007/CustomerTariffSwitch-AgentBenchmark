namespace CustomerTariffSwitch.Models.Models;

public record Tariff
{
    public required string TariffId { get; init; }
    public required string Name { get; init; }
    public required bool RequiresSmartMeter { get; init; }
    public required decimal BaseMonthlyGross { get; init; }
}
