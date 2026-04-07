using CustomerTariffSwitch.Data.Helpers;
using CustomerTariffSwitch.Data.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CustomerTariffSwitch.Tests.Integration;

[Trait("Category", "Integration")]
public class CsvReaderServiceIntegrationTests
{
    private readonly CsvReaderService _sut;
    private readonly string _inputPath;

    public CsvReaderServiceIntegrationTests()
    {
        _sut = new CsvReaderService(NullLogger<CsvReaderService>.Instance);
        _inputPath = SolutionPathHelper.GetInputFilesPath();
    }

    [Fact]
    public void ReadCustomers_LoadsAllValidRows_SkipsMalformed()
    {
        var (customers, skipped) = _sut.ReadCustomers(_inputPath);

        // 7 data rows: C001-C006 valid (C006 has empty name but valid ID), last row has empty ID
        Assert.Equal(6, customers.Count);
        Assert.Equal(1, skipped);
        Assert.Contains(customers, c => c.CustomerId == "C001");
        Assert.Contains(customers, c => c.CustomerId == "C006");
        Assert.DoesNotContain(customers, c => c.Name == "Max Muster");
    }

    [Fact]
    public void ReadTariffs_LoadsAllValidRows()
    {
        var (tariffs, skipped) = _sut.ReadTariffs(_inputPath);

        Assert.Equal(3, tariffs.Count);
        Assert.Equal(0, skipped);
        Assert.Contains(tariffs, t => t.TariffId == "T-ECO");
        Assert.Contains(tariffs, t => t.TariffId == "T-BASIC");
        Assert.Contains(tariffs, t => t.TariffId == "T-PRO");
    }

    [Fact]
    public void ReadRequests_LoadsAllRows_IncludingMalformed()
    {
        var (valid, malformed) = _sut.ReadRequests(_inputPath);

        // R1001-R1006 have valid dates, R1007 empty tariff, R1008 bad date, R1009 empty date
        Assert.Equal(6, valid.Count);
        Assert.Equal(3, malformed.Count);
    }

    [Fact]
    public void ReadCustomers_MissingRequiredColumn_ThrowsDescriptiveException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"cts-integ-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "customers.csv"), "Id;Name;SLA\nC001;Test;Standard");

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.ReadCustomers(tempDir));
            Assert.Contains("CustomerId", ex.Message);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
