using CustomerTariffSwitch.Models;
using CustomerTariffSwitch.Parsing;
using CustomerTariffSwitch.Services;

namespace CustomerTariffSwitch.Tests;

#region SLA Calculator Tests

public class SlaCalculatorTests
{
    [Fact]
    public void Standard_Sla_Adds_48_Hours()
    {
        var requested = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.FromHours(2));
        var due = SlaCalculator.ComputeSlaDueDate(requested, isPremium: false, needsMeterUpgrade: false);
        Assert.Equal(new DateTimeOffset(2025, 6, 17, 10, 0, 0, TimeSpan.FromHours(2)), due);
    }

    [Fact]
    public void Premium_Sla_Adds_24_Hours()
    {
        var requested = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.FromHours(2));
        var due = SlaCalculator.ComputeSlaDueDate(requested, isPremium: true, needsMeterUpgrade: false);
        Assert.Equal(new DateTimeOffset(2025, 6, 16, 10, 0, 0, TimeSpan.FromHours(2)), due);
    }

    [Fact]
    public void Meter_Upgrade_Adds_12_Extra_Hours()
    {
        var requested = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.FromHours(2));
        var due = SlaCalculator.ComputeSlaDueDate(requested, isPremium: false, needsMeterUpgrade: true);
        // 48 + 12 = 60h
        Assert.Equal(new DateTimeOffset(2025, 6, 17, 22, 0, 0, TimeSpan.FromHours(2)), due);
    }

    [Fact]
    public void Dst_Spring_Forward_March_2025()
    {
        // Europe/Vienna: clocks go forward at 2025-03-30 02:00 CET → 03:00 CEST
        // Request at 01:15 CET (+01:00), +24h premium
        var requested = new DateTimeOffset(2025, 3, 30, 1, 15, 0, TimeSpan.FromHours(1));
        var due = SlaCalculator.ComputeSlaDueDate(requested, isPremium: true, needsMeterUpgrade: false);
        // 24h in local time: 2025-03-31 01:15 CEST (+02:00)
        Assert.Equal(new DateTimeOffset(2025, 3, 31, 1, 15, 0, TimeSpan.FromHours(2)), due);
    }

    [Fact]
    public void Dst_Fall_Back_October_2025()
    {
        // Europe/Vienna: clocks go back at 2025-10-26 03:00 CEST → 02:00 CET
        // Request at 02:05 CEST (+02:00), +48h standard
        var requested = new DateTimeOffset(2025, 10, 26, 2, 5, 0, TimeSpan.FromHours(2));
        var due = SlaCalculator.ComputeSlaDueDate(requested, isPremium: false, needsMeterUpgrade: false);
        // 48h in local time: 2025-10-28 02:05 CET (+01:00)
        Assert.Equal(new DateTimeOffset(2025, 10, 28, 2, 5, 0, TimeSpan.FromHours(1)), due);
    }
}

#endregion

#region Tariff Switch Processor Tests

public class TariffSwitchProcessorTests
{
    private static List<Customer> DefaultCustomers() =>
    [
        new() { CustomerId = "C001", Name = "Anna", HasUnpaidInvoice = false, Sla = "Premium", MeterType = "Smart" },
        new() { CustomerId = "C002", Name = "Bob",  HasUnpaidInvoice = true,  Sla = "Standard", MeterType = "Classic" },
        new() { CustomerId = "C003", Name = "Cara", HasUnpaidInvoice = false, Sla = "Standard", MeterType = "Classic" },
    ];

    private static List<Tariff> DefaultTariffs() =>
    [
        new() { TariffId = "T-ECO",   Name = "Eco",   RequiresSmartMeter = true,  BaseMonthlyGross = 29.90m },
        new() { TariffId = "T-BASIC", Name = "Basic", RequiresSmartMeter = false, BaseMonthlyGross = 24.50m },
    ];

    private static TariffSwitchProcessor CreateProcessor() => new(DefaultCustomers(), DefaultTariffs());

    [Fact]
    public void Scenario1_Standard_Approved()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "C003", "T-BASIC", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Approved, result.Status);
        Assert.Null(result.FollowUpAction);
    }

    [Fact]
    public void Scenario2_Premium_Approved()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "C001", "T-ECO", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Approved, result.Status);
        Assert.Null(result.FollowUpAction); // C001 already has smart meter
    }

    [Fact]
    public void Scenario3_MeterUpgrade_Required()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "C003", "T-ECO", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Approved, result.Status);
        Assert.Equal("Schedule meter upgrade", result.FollowUpAction);
    }

    [Fact]
    public void Scenario4_UnpaidInvoice_Rejected()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "C002", "T-BASIC", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Rejected, result.Status);
        Assert.Contains("Unpaid invoice", result.RejectionReason!);
    }

    [Fact]
    public void Scenario5_UnknownCustomer_Rejected()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "CXXX", "T-BASIC", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Rejected, result.Status);
        Assert.Contains("Unknown customer", result.RejectionReason!);
    }

    [Fact]
    public void Scenario6_UnknownTariff_Rejected()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "C001", "T-NONE", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Rejected, result.Status);
        Assert.Contains("Unknown tariff", result.RejectionReason!);
    }

    [Fact]
    public void Scenario7_InvalidTimestamp_Rejected()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "C001", "T-BASIC", "not-a-date"));
        Assert.Equal(RequestStatus.Rejected, result.Status);
        Assert.Contains("Invalid request data", result.RejectionReason!);
    }

    [Fact]
    public void Scenario7_EmptyFields_Rejected()
    {
        var proc = CreateProcessor();
        var result = proc.Process(new RawRequest("R1", "", "T-BASIC", "2025-06-15T10:00:00+02:00"));
        Assert.Equal(RequestStatus.Rejected, result.Status);
        Assert.Contains("Invalid request data", result.RejectionReason!);
    }
}

#endregion

#region Processing Store Tests

public class ProcessingStoreTests
{
    [Fact]
    public void Tracks_Processed_Ids()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store = new ProcessingStore(path);
            Assert.False(store.IsProcessed("R1"));

            store.Add(new ProcessingResult { RequestId = "R1", Status = RequestStatus.Approved });
            Assert.True(store.IsProcessed("R1"));
            Assert.False(store.IsProcessed("R2"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Persists_And_Reloads_Across_Instances()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        try
        {
            var store1 = new ProcessingStore(path);
            store1.Add(new ProcessingResult { RequestId = "R1", Status = RequestStatus.Approved });
            store1.Save();

            var store2 = new ProcessingStore(path);
            Assert.True(store2.IsProcessed("R1"));
            Assert.Single(store2.Results);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

#endregion
