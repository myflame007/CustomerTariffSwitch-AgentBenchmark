using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Services;

public class ProcessRequestService
{
    private static readonly TimeZoneInfo Vienna = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

    public List<RequestDecision> ProcessRequests(
        IReadOnlyCollection<Customer> customers,
        IReadOnlyCollection<SwitchRequest> requests,
        IReadOnlyCollection<Tariff> tariffs,
        IReadOnlyCollection<string> processedIds)
    {
        var customerMap = customers.ToDictionary(c => c.CustomerId, StringComparer.OrdinalIgnoreCase);
        var tariffMap = tariffs.ToDictionary(t => t.TariffId, StringComparer.OrdinalIgnoreCase);
        var processed = new HashSet<string>(processedIds, StringComparer.OrdinalIgnoreCase);

        var decisions = new List<RequestDecision>();

        foreach (var request in requests)
        {
            if (processed.Contains(request.RequestId))
                continue;

            // Scenario 5: Unknown customer
            if (!customerMap.TryGetValue(request.CustomerId, out var customer))
            {
                decisions.Add(RequestDecision.Rejected(request.RequestId, "Unknown customer"));
                continue;
            }

            // Scenario 6: Unknown tariff
            if (!tariffMap.TryGetValue(request.TargetTariffId, out var tariff))
            {
                decisions.Add(RequestDecision.Rejected(request.RequestId, "Unknown tariff"));
                continue;
            }

            // Scenario 4: Unpaid invoice
            if (customer.HasUnpaidInvoice)
            {
                decisions.Add(RequestDecision.Rejected(request.RequestId, "Unpaid invoice"));
                continue;
            }

            // Scenarios 1, 2, 3: Approve
            var slaHours = CalculateSlaHours(customer, tariff);
            var needsUpgrade = tariff.RequiresSmartMeter && customer.MeterType == MeterType.Classic;
            var followUp = needsUpgrade ? "Schedule meter upgrade" : null;
            var dueAt = ComputeDueDate(request.RequestedAt, slaHours);

            decisions.Add(RequestDecision.Approved(request.RequestId, customer.Name, dueAt, followUp));
        }

        return decisions;
    }

    public int CalculateSlaHours(Customer customer, Tariff tariff)
    {
        var baseHours = customer.Sla == SLALevel.Premium ? 24 : 48;
        var needsUpgrade = tariff.RequiresSmartMeter && customer.MeterType == MeterType.Classic;
        return needsUpgrade ? baseHours + 12 : baseHours;
    }

    private static DateTimeOffset ComputeDueDate(DateTimeOffset requestedAt, int slaHours)
    {
        // Convert to Vienna local time
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(requestedAt.UtcDateTime, Vienna);

        // Add SLA hours in local time
        var localDue = localDateTime.AddHours(slaHours);

        // Handle DST transitions
        if (Vienna.IsInvalidTime(localDue))
        {
            // Spring forward gap: push the time forward past the gap
            var offsetAfterGap = Vienna.GetUtcOffset(localDue.AddHours(1));
            var offsetBeforeGap = Vienna.GetUtcOffset(localDue.AddHours(-1));
            var gapDuration = offsetAfterGap - offsetBeforeGap;
            var adjustedLocal = localDue.Add(gapDuration);
            return new DateTimeOffset(adjustedLocal, offsetAfterGap);
        }

        if (Vienna.IsAmbiguousTime(localDue))
        {
            // Fall back overlap: pick the maximum offset (earlier occurrence)
            var offsets = Vienna.GetAmbiguousTimeOffsets(localDue);
            var maxOffset = offsets.Max()!;
            return new DateTimeOffset(localDue, maxOffset);
        }

        // Normal case
        var offset = Vienna.GetUtcOffset(localDue);
        return new DateTimeOffset(localDue, offset);
    }
}
