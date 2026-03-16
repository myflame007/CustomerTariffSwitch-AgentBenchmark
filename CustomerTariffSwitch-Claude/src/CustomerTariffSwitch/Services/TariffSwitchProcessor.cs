using CustomerTariffSwitch.Models;
using CustomerTariffSwitch.Parsing;

namespace CustomerTariffSwitch.Services;

public sealed class TariffSwitchProcessor
{
    private readonly Dictionary<string, Customer> _customers;
    private readonly Dictionary<string, Tariff> _tariffs;

    public TariffSwitchProcessor(
        IEnumerable<Customer> customers, IEnumerable<Tariff> tariffs)
    {
        _customers = customers.ToDictionary(c => c.CustomerId);
        _tariffs = tariffs.ToDictionary(t => t.TariffId);
    }

    /// <summary>
    /// Processes a single raw request row and returns the result.
    /// Already-processed IDs must be filtered before calling this method.
    /// </summary>
    public ProcessingResult Process(RawRequest raw)
    {
        // --- Validate request data ---
        if (string.IsNullOrWhiteSpace(raw.RequestId))
            return Reject(raw.RequestId, "Invalid request data", "Missing RequestId.");

        if (string.IsNullOrWhiteSpace(raw.CustomerId) ||
            string.IsNullOrWhiteSpace(raw.TargetTariffId) ||
            string.IsNullOrWhiteSpace(raw.RequestedAtIso8601))
            return Reject(raw.RequestId, "Invalid request data");

        if (!DateTimeOffset.TryParse(raw.RequestedAtIso8601, out var requestedAt))
            return Reject(raw.RequestId, "Invalid request data",
                $"Cannot parse timestamp: '{raw.RequestedAtIso8601}'.");

        // --- Validate references ---
        if (!_customers.TryGetValue(raw.CustomerId, out var customer))
            return Reject(raw.RequestId, "Unknown customer",
                customerId: raw.CustomerId, tariffId: raw.TargetTariffId, requestedAt: requestedAt);

        if (!_tariffs.TryGetValue(raw.TargetTariffId, out var tariff))
            return Reject(raw.RequestId, "Unknown tariff",
                customerId: raw.CustomerId, tariffId: raw.TargetTariffId, requestedAt: requestedAt);

        // --- Business rules ---
        if (customer.HasUnpaidInvoice)
            return Reject(raw.RequestId, "Unpaid invoice",
                customerId: raw.CustomerId, tariffId: raw.TargetTariffId, requestedAt: requestedAt);

        bool needsMeterUpgrade = tariff.RequiresSmartMeter && !customer.HasSmartMeter;
        string? followUp = needsMeterUpgrade ? "Schedule meter upgrade" : null;

        var slaDue = SlaCalculator.ComputeSlaDueDate(
            requestedAt, customer.IsPremium, needsMeterUpgrade);

        return new ProcessingResult
        {
            RequestId = raw.RequestId,
            Status = RequestStatus.Approved,
            CustomerId = raw.CustomerId,
            TargetTariffId = raw.TargetTariffId,
            RequestedAt = requestedAt,
            SlaDueDate = slaDue,
            FollowUpAction = followUp
        };
    }

    private static ProcessingResult Reject(
        string requestId,
        string reason,
        string? detail = null,
        string? customerId = null,
        string? tariffId = null,
        DateTimeOffset? requestedAt = null)
    {
        return new ProcessingResult
        {
            RequestId = string.IsNullOrWhiteSpace(requestId) ? "(empty)" : requestId,
            Status = RequestStatus.Rejected,
            RejectionReason = detail is null ? reason : $"{reason} — {detail}",
            CustomerId = customerId,
            TargetTariffId = tariffId,
            RequestedAt = requestedAt
        };
    }
}
