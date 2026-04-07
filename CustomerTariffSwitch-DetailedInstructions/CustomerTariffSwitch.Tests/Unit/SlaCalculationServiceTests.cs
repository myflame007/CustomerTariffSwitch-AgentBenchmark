using CustomerTariffSwitch.Models.Enums;
using CustomerTariffSwitch.Models.Models;
using CustomerTariffSwitch.Services;

namespace CustomerTariffSwitch.Tests.Unit;

public class SlaCalculationServiceTests
{
    private readonly SlaCalculationService _sut = new();

    [Fact]
    public void StandardSla_Adds48Hours_WithCorrectViennaOffset()
    {
        var requestedAt = DateTimeOffset.Parse("2025-06-15T10:00:00+02:00");

        var result = _sut.CalculateDeadline(requestedAt, SlaLevel.Standard, requiresMeterUpgrade: false);

        Assert.Equal("2025-06-17T10:00:00+02:00", Format(result.DueAt));
        Assert.Null(result.FollowUpAction);
        Assert.Null(result.FollowUpDueAt);
    }

    [Fact]
    public void PremiumSla_Adds24Hours_WithCorrectViennaOffset()
    {
        var requestedAt = DateTimeOffset.Parse("2025-06-15T10:00:00+02:00");

        var result = _sut.CalculateDeadline(requestedAt, SlaLevel.Premium, requiresMeterUpgrade: false);

        Assert.Equal("2025-06-16T10:00:00+02:00", Format(result.DueAt));
        Assert.Null(result.FollowUpAction);
        Assert.Null(result.FollowUpDueAt);
    }

    [Fact]
    public void MeterUpgrade_AddsExtraHours_ToBaseSla()
    {
        var requestedAt = DateTimeOffset.Parse("2025-06-15T10:00:00+02:00");

        var result = _sut.CalculateDeadline(requestedAt, SlaLevel.Standard, requiresMeterUpgrade: true);

        Assert.Equal("2025-06-17T10:00:00+02:00", Format(result.DueAt));
        Assert.Equal("Schedule meter upgrade", result.FollowUpAction);
        Assert.NotNull(result.FollowUpDueAt);
        Assert.Equal("2025-06-17T22:00:00+02:00", Format(result.FollowUpDueAt!.Value));
    }

    [Fact]
    public void DstSpringForward_RequestAt0115CET_Plus48h_ResultsInCEST()
    {
        // 2025-03-30 is DST spring-forward in Europe/Vienna (02:00 → 03:00)
        var requestedAt = DateTimeOffset.Parse("2025-03-30T01:15:00+01:00");

        var result = _sut.CalculateDeadline(requestedAt, SlaLevel.Standard, requiresMeterUpgrade: false);

        Assert.Equal("2025-04-01T01:15:00+02:00", Format(result.DueAt));
    }

    [Fact]
    public void DstFallBack_RequestAt0230CEST_Plus48h_ResolvesToLaterOffset()
    {
        // 2025-10-26 is DST fall-back in Europe/Vienna (03:00 → 02:00)
        var requestedAt = DateTimeOffset.Parse("2025-10-26T02:30:00+02:00");

        var result = _sut.CalculateDeadline(requestedAt, SlaLevel.Standard, requiresMeterUpgrade: false);

        // Falls into ambiguous time — resolves to later offset (+01:00)
        Assert.Equal("2025-10-28T02:30:00+01:00", Format(result.DueAt));
    }

    private static string Format(DateTimeOffset dto)
    {
        return dto.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }
}
