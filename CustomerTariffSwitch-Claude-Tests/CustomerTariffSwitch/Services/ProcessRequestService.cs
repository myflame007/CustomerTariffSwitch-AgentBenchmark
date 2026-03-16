using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Services;

public class ProcessRequestService
{
    private static readonly TimeZoneInfo ViennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

    public List<RequestDecision> ProcessRequests(
        List<Customer> customers,
        List<SwitchRequest> requests,
        List<Tariff> tariffs,
        HashSet<string> processedIds)
    {
        var customerMap = customers.ToDictionary(c => c.CustomerId, StringComparer.OrdinalIgnoreCase);
        var tariffMap = tariffs.ToDictionary(t => t.TariffId, StringComparer.OrdinalIgnoreCase);

        var decisions = new List<RequestDecision>();

        foreach (var request in requests)
        {
            if (processedIds.Contains(request.RequestId))
                continue;

            if (!customerMap.TryGetValue(request.CustomerId, out var customer))
            {
                decisions.Add(RequestDecision.Rejected(request.RequestId, "Unknown customer"));
                continue;
            }

            if (!tariffMap.TryGetValue(request.TargetTariffId, out var tariff))
            {
                decisions.Add(RequestDecision.Rejected(request.RequestId, "Unknown tariff"));
                continue;
            }

            if (customer.HasUnpaidInvoice)
            {
                decisions.Add(RequestDecision.Rejected(request.RequestId, "Unpaid invoice"));
                continue;
            }

            var slaHours = CalculateSlaHours(customer, tariff);
            var dueAt = ComputeDueAt(request.RequestedAt, slaHours);
            var needsUpgrade = tariff.RequiresSmartMeter && customer.MeterType == MeterType.Classic;
            var followUp = needsUpgrade ? "Schedule meter upgrade" : null;

            decisions.Add(RequestDecision.Approved(request.RequestId, customer.Name, dueAt, followUp));
        }

        return decisions;
    }

    public int CalculateSlaHours(Customer customer, Tariff tariff)
    {
        var baseHours = customer.Sla == SLALevel.Premium ? 24 : 48;
        var upgradeHours = (tariff.RequiresSmartMeter && customer.MeterType == MeterType.Classic) ? 12 : 0;
        return baseHours + upgradeHours;
    }

    private static DateTimeOffset ComputeDueAt(DateTimeOffset requestedAt, int slaHours)
    {
        // Convert to Vienna local time
        var localDt = TimeZoneInfo.ConvertTimeFromUtc(requestedAt.UtcDateTime, ViennaTimeZone);

        // Add SLA hours in local time
        var targetLocalDt = localDt.AddHours(slaHours);

        // Handle DST transitions
        return ResolveViennaLocal(targetLocalDt);
    }

    private static DateTimeOffset ResolveViennaLocal(DateTime localDt)
    {
        if (ViennaTimeZone.IsInvalidTime(localDt))
        {
            // Gap (spring forward): push forward by the DST delta
            var rules = ViennaTimeZone.GetAdjustmentRules();
            var rule = rules.FirstOrDefault(r => r.DateStart <= localDt && r.DateEnd >= localDt);
            var delta = rule?.DaylightDelta ?? TimeSpan.FromHours(1);
            var adjusted = localDt.Add(delta);
            return new DateTimeOffset(adjusted, ViennaTimeZone.GetUtcOffset(adjusted));
        }

        if (ViennaTimeZone.IsAmbiguousTime(localDt))
        {
            // Ambiguous (fall back): pick max offset (earlier occurrence, e.g. CEST +02:00)
            var offsets = ViennaTimeZone.GetAmbiguousTimeOffsets(localDt);
            var maxOffset = offsets.Max();
            return new DateTimeOffset(localDt, maxOffset);
        }

        return new DateTimeOffset(localDt, ViennaTimeZone.GetUtcOffset(localDt));
    }
}
