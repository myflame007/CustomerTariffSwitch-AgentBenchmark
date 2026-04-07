using CustomerTariffSwitch.Models.Enums;
using CustomerTariffSwitch.Models.Models;

namespace CustomerTariffSwitch.Services;

public class RequestValidationService
{
    private const string ReasonUnpaidInvoice = "Unpaid invoice";
    private const string ReasonUnknownCustomer = "Unknown customer";
    private const string ReasonUnknownTariff = "Unknown tariff";
    private const string ReasonInvalidRequestData = "Invalid request data";

    private readonly SlaCalculationService _slaCalculationService;

    public RequestValidationService(SlaCalculationService slaCalculationService)
    {
        _slaCalculationService = slaCalculationService;
    }

    public RequestDecision ProcessMalformedRequest(string requestId)
    {
        return CreateRejection(requestId, ReasonInvalidRequestData, customerName: null);
    }

    public RequestDecision ProcessRequest(
        SwitchRequest request,
        IReadOnlyDictionary<string, Customer> customers,
        IReadOnlyDictionary<string, Tariff> tariffs)
    {
        if (!customers.TryGetValue(request.CustomerId, out var customer))
            return CreateRejection(request.RequestId, ReasonUnknownCustomer, customerName: null);

        if (!tariffs.TryGetValue(request.TargetTariffId, out var tariff))
            return CreateRejection(request.RequestId, ReasonUnknownTariff, customer.Name);

        if (customer.HasUnpaidInvoice)
            return CreateRejection(request.RequestId, ReasonUnpaidInvoice, customer.Name);

        var requiresMeterUpgrade = tariff.RequiresSmartMeter && customer.MeterType == MeterType.Classic;

        var deadline = _slaCalculationService.CalculateDeadline(
            request.RequestedAt,
            customer.SLA,
            requiresMeterUpgrade);

        return CreateApproval(request.RequestId, customer.Name, deadline);
    }

    private static RequestDecision CreateRejection(string requestId, string reason, string? customerName)
    {
        return new RequestDecision
        {
            RequestId = requestId,
            Status = DecisionStatus.Rejected,
            CustomerName = customerName,
            Reason = reason
        };
    }

    private static RequestDecision CreateApproval(string requestId, string? customerName, SlaDeadline deadline)
    {
        return new RequestDecision
        {
            RequestId = requestId,
            Status = DecisionStatus.Approved,
            CustomerName = customerName,
            DueAt = FormatDateTimeOffset(deadline.DueAt),
            FollowUpAction = deadline.FollowUpAction,
            FollowUpDueAt = deadline.FollowUpDueAt.HasValue
                ? FormatDateTimeOffset(deadline.FollowUpDueAt.Value)
                : null
        };
    }

    private static string FormatDateTimeOffset(DateTimeOffset dto)
    {
        return dto.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }
}
