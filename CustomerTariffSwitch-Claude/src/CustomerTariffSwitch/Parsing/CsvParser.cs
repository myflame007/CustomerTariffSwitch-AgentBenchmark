using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Parsing;

public sealed class CustomerMap : ClassMap<Customer>
{
    public CustomerMap()
    {
        Map(m => m.CustomerId).Name("CustomerId");
        Map(m => m.Name).Name("Name");
        Map(m => m.HasUnpaidInvoice).Name("HasUnpaidInvoice");
        Map(m => m.Sla).Name("SLA");
        Map(m => m.MeterType).Name("MeterType");
    }
}

public sealed class TariffMap : ClassMap<Tariff>
{
    public TariffMap()
    {
        Map(m => m.TariffId).Name("TariffId");
        Map(m => m.Name).Name("Name");
        Map(m => m.RequiresSmartMeter).Name("RequiresSmartMeter");
        Map(m => m.BaseMonthlyGross).Name("BaseMonthlyGross");
    }
}

public static class CsvParser
{
    private static CsvConfiguration CreateConfig() => new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        HasHeaderRecord = true,
        MissingFieldFound = null
    };

    public static List<Customer> LoadCustomers(string path)
    {
        ValidateFileExists(path);
        using var reader = new StreamReader(path, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, CreateConfig());
        csv.Context.RegisterClassMap<CustomerMap>();
        ValidateHeader<Customer>(csv, path);
        return csv.GetRecords<Customer>().ToList();
    }

    public static List<Tariff> LoadTariffs(string path)
    {
        ValidateFileExists(path);
        using var reader = new StreamReader(path, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, CreateConfig());
        csv.Context.RegisterClassMap<TariffMap>();
        ValidateHeader<Tariff>(csv, path);
        return csv.GetRecords<Tariff>().ToList();
    }

    /// <summary>
    /// Loads raw request rows as string arrays. Parsing of individual fields
    /// (especially timestamps) is deferred to the processing layer so that
    /// malformed rows can be rejected per-request instead of aborting the whole file.
    /// </summary>
    public static List<RawRequest> LoadRawRequests(string path)
    {
        ValidateFileExists(path);
        using var reader = new StreamReader(path, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, CreateConfig());

        csv.Read();
        csv.ReadHeader();
        var header = csv.HeaderRecord
            ?? throw new InvalidOperationException($"CSV schema error: '{path}' has no header row.");

        var required = new[] { "RequestId", "CustomerId", "TargetTariffId", "RequestedAtISO8601" };
        var missing = required.Where(r => !header.Contains(r)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"CSV schema error in '{path}': missing columns: {string.Join(", ", missing)}");

        var results = new List<RawRequest>();
        while (csv.Read())
        {
            results.Add(new RawRequest(
                csv.GetField("RequestId") ?? "",
                csv.GetField("CustomerId") ?? "",
                csv.GetField("TargetTariffId") ?? "",
                csv.GetField("RequestedAtISO8601") ?? ""));
        }
        return results;
    }

    private static void ValidateFileExists(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Required input file not found: '{path}'", path);
    }

    private static void ValidateHeader<T>(CsvReader csv, string path)
    {
        csv.Read();
        csv.ReadHeader();
        try
        {
            csv.ValidateHeader<T>();
        }
        catch (HeaderValidationException ex)
        {
            throw new InvalidOperationException(
                $"CSV schema error in '{path}': {ex.Message}", ex);
        }
    }
}

public record RawRequest(
    string RequestId,
    string CustomerId,
    string TargetTariffId,
    string RequestedAtIso8601);
