using CustomerTariffSwitch.Models.Enums;

namespace CustomerTariffSwitch.Models.Models;

public record Customer
{
    public required string CustomerId { get; init; }
    public string? Name { get; init; }
    public required bool HasUnpaidInvoice { get; init; }
    public required SlaLevel SLA { get; init; }
    public required MeterType MeterType { get; init; }
}
