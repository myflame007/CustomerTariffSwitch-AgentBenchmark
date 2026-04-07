using CustomerTariffSwitch.Models.Enums;
using CustomerTariffSwitch.Models.Models;
using CustomerTariffSwitch.Services;

namespace CustomerTariffSwitch.Tests.Unit;

public class RequestValidationServiceTests
{
    private readonly RequestValidationService _sut;
    private readonly IReadOnlyDictionary<string, Customer> _customers;
    private readonly IReadOnlyDictionary<string, Tariff> _tariffs;

    public RequestValidationServiceTests()
    {
        _sut = new RequestValidationService(new SlaCalculationService());

        _customers = new Dictionary<string, Customer>(StringComparer.Ordinal)
        {
            ["C001"] = new Customer
            {
                CustomerId = "C001", Name = "Anna Maier",
                HasUnpaidInvoice = false, SLA = SlaLevel.Premium, MeterType = MeterType.Smart
            },
            ["C002"] = new Customer
            {
                CustomerId = "C002", Name = "Stadtcafé GmbH",
                HasUnpaidInvoice = true, SLA = SlaLevel.Standard, MeterType = MeterType.Classic
            },
            ["C003"] = new Customer
            {
                CustomerId = "C003", Name = "Jamal Idris",
                HasUnpaidInvoice = false, SLA = SlaLevel.Standard, MeterType = MeterType.Classic
            }
        };

        _tariffs = new Dictionary<string, Tariff>(StringComparer.Ordinal)
        {
            ["T-ECO"] = new Tariff
            {
                TariffId = "T-ECO", Name = "ÖkoStrom",
                RequiresSmartMeter = true, BaseMonthlyGross = 29.90m
            },
            ["T-BASIC"] = new Tariff
            {
                TariffId = "T-BASIC", Name = "Basis",
                RequiresSmartMeter = false, BaseMonthlyGross = 24.50m
            }
        };
    }

    [Fact]
    public void Scenario3_TariffRequiresSmartMeter_CustomerHasClassic_ApprovedWithFollowUp()
    {
        var request = CreateRequest("R100", "C003", "T-ECO");

        var result = _sut.ProcessRequest(request, _customers, _tariffs);

        Assert.Equal(DecisionStatus.Approved, result.Status);
        Assert.Equal("Schedule meter upgrade", result.FollowUpAction);
        Assert.NotNull(result.FollowUpDueAt);
    }

    [Fact]
    public void Scenario4_UnpaidInvoice_Rejected()
    {
        var request = CreateRequest("R100", "C002", "T-BASIC");

        var result = _sut.ProcessRequest(request, _customers, _tariffs);

        Assert.Equal(DecisionStatus.Rejected, result.Status);
        Assert.Equal("Unpaid invoice", result.Reason);
    }

    [Fact]
    public void Scenario5_UnknownCustomer_Rejected()
    {
        var request = CreateRequest("R100", "C999", "T-BASIC");

        var result = _sut.ProcessRequest(request, _customers, _tariffs);

        Assert.Equal(DecisionStatus.Rejected, result.Status);
        Assert.Equal("Unknown customer", result.Reason);
        Assert.Null(result.CustomerName);
    }

    [Fact]
    public void Scenario6_UnknownTariff_Rejected()
    {
        var request = CreateRequest("R100", "C001", "T-UNKNOWN");

        var result = _sut.ProcessRequest(request, _customers, _tariffs);

        Assert.Equal(DecisionStatus.Rejected, result.Status);
        Assert.Equal("Unknown tariff", result.Reason);
        Assert.Equal("Anna Maier", result.CustomerName);
    }

    [Fact]
    public void ScenarioOrdering_UnpaidInvoiceTakesPriorityOverMeterUpgrade()
    {
        // C002 has unpaid invoice AND Classic meter; T-ECO requires Smart
        var request = CreateRequest("R100", "C002", "T-ECO");

        var result = _sut.ProcessRequest(request, _customers, _tariffs);

        Assert.Equal(DecisionStatus.Rejected, result.Status);
        Assert.Equal("Unpaid invoice", result.Reason);
    }

    [Fact]
    public void ApprovedPremiumSla_SmartMeterAlreadyInstalled_NoFollowUp()
    {
        // C001 has Premium SLA, Smart meter
        var request = CreateRequest("R100", "C001", "T-ECO");

        var result = _sut.ProcessRequest(request, _customers, _tariffs);

        Assert.Equal(DecisionStatus.Approved, result.Status);
        Assert.Null(result.FollowUpAction);
        Assert.Null(result.FollowUpDueAt);
    }

    [Fact]
    public void MalformedRequest_RejectedWithInvalidRequestData()
    {
        var result = _sut.ProcessMalformedRequest("R999");

        Assert.Equal(DecisionStatus.Rejected, result.Status);
        Assert.Equal("Invalid request data", result.Reason);
        Assert.Null(result.CustomerName);
    }

    private static SwitchRequest CreateRequest(string requestId, string customerId, string tariffId)
    {
        return new SwitchRequest
        {
            RequestId = requestId,
            CustomerId = customerId,
            TargetTariffId = tariffId,
            RequestedAt = DateTimeOffset.Parse("2025-06-15T10:00:00+02:00")
        };
    }
}
