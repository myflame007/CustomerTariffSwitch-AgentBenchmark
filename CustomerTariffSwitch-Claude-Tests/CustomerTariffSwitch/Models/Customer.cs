namespace CustomerTariffSwitch.Models;

public class Customer
{
    public string CustomerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasUnpaidInvoice { get; set; }
    public SLALevel Sla { get; set; }
    public MeterType MeterType { get; set; }
}
