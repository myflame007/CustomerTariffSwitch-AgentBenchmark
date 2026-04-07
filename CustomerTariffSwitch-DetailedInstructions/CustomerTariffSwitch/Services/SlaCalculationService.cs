using CustomerTariffSwitch.Models.Enums;
using CustomerTariffSwitch.Models.Models;

namespace CustomerTariffSwitch.Services;

public class SlaCalculationService
{
    private static readonly TimeZoneInfo ViennaTimeZone = GetViennaTimeZone();

    private static readonly TimeSpan StandardSlaDuration = TimeSpan.FromHours(48);
    private static readonly TimeSpan PremiumSlaDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan MeterUpgradeExtension = TimeSpan.FromHours(12);

    public SlaDeadline CalculateDeadline(DateTimeOffset requestedAt, SlaLevel slaLevel, bool requiresMeterUpgrade)
    {
        var baseDuration = slaLevel switch
        {
            SlaLevel.Standard => StandardSlaDuration,
            SlaLevel.Premium => PremiumSlaDuration,
            _ => throw new ArgumentOutOfRangeException(nameof(slaLevel))
        };

        var dueAt = AddDurationInVienna(requestedAt, baseDuration);

        if (!requiresMeterUpgrade)
        {
            return new SlaDeadline { DueAt = dueAt };
        }

        var followUpDueAt = AddDurationInVienna(dueAt, MeterUpgradeExtension);

        return new SlaDeadline
        {
            DueAt = dueAt,
            FollowUpAction = "Schedule meter upgrade",
            FollowUpDueAt = followUpDueAt
        };
    }

    private static DateTimeOffset AddDurationInVienna(DateTimeOffset source, TimeSpan duration)
    {
        var viennaLocal = TimeZoneInfo.ConvertTime(source, ViennaTimeZone);
        var resultLocal = viennaLocal.DateTime + duration;

        if (ViennaTimeZone.IsInvalidTime(resultLocal))
        {
            var adjustment = ViennaTimeZone.GetAdjustmentRules()
                .Where(r => r.DateStart <= resultLocal && r.DateEnd >= resultLocal)
                .Select(r => r.DaylightDelta)
                .FirstOrDefault(TimeSpan.FromHours(1));

            resultLocal = resultLocal.Add(adjustment);
        }

        TimeSpan offset;
        if (ViennaTimeZone.IsAmbiguousTime(resultLocal))
        {
            var offsets = ViennaTimeZone.GetAmbiguousTimeOffsets(resultLocal);
            offset = offsets.Min();
        }
        else
        {
            offset = ViennaTimeZone.GetUtcOffset(resultLocal);
        }

        return new DateTimeOffset(resultLocal, offset);
    }

    private static TimeZoneInfo GetViennaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }
}
