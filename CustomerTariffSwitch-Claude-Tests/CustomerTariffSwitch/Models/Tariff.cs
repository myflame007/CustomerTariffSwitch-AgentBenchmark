namespace CustomerTariffSwitch.Models;

public class Tariff
{
    public string TariffId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool RequiresSmartMeter { get; set; }
    public decimal BaseMonthlyGross { get; set; }
}
