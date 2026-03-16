namespace CustomerTariffSwitch.Services;

public static class SlaCalculator
{
    private static readonly TimeZoneInfo ViennaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

    /// <summary>
    /// Computes the SLA due date in Europe/Vienna local time, then returns it as a DateTimeOffset.
    /// DST-safe: the duration is added in local time semantics.
    /// </summary>
    public static DateTimeOffset ComputeSlaDueDate(
        DateTimeOffset requestedAt, bool isPremium, bool needsMeterUpgrade)
    {
        // Convert to Vienna local time
        var localRequested = TimeZoneInfo.ConvertTime(requestedAt, ViennaTimeZone);

        // Base SLA: Premium = 24h, Standard = 48h
        var slaHours = isPremium ? 24 : 48;

        // Meter upgrade adds 12h
        if (needsMeterUpgrade)
            slaHours += 12;

        // Add hours in local time, then resolve the offset for the resulting local time
        var localDue = localRequested.DateTime + TimeSpan.FromHours(slaHours);
        var dueOffset = ViennaTimeZone.GetUtcOffset(localDue);

        return new DateTimeOffset(localDue, dueOffset);
    }
}
