using System.Text;
using CustomerTariffSwitch.Data.Helpers;
using CustomerTariffSwitch.Models.Models;
using Microsoft.Extensions.Logging;

namespace CustomerTariffSwitch.Data.Services;

public class CsvReaderService
{
    private const string CustomersFileName = "customers.csv";
    private const string TariffsFileName = "tariffs.csv";
    private const string RequestsFileName = "requests.csv";

    private readonly ILogger<CsvReaderService> _logger;

    public CsvReaderService(ILogger<CsvReaderService> logger)
    {
        _logger = logger;
    }

    public (IReadOnlyList<Customer> Customers, int Skipped) ReadCustomers(string inputPath)
    {
        var filePath = Path.Combine(inputPath, CustomersFileName);
        EnsureFileExists(filePath);

        var lines = ReadAllLines(filePath);
        if (lines.Count == 0)
            throw new InvalidOperationException($"File '{CustomersFileName}' is empty — header row required.");

        var headers = CsvParsingHelper.SplitLine(lines[0]);
        var idIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "CustomerId", CustomersFileName);
        var nameIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "Name", CustomersFileName);
        var invoiceIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "HasUnpaidInvoice", CustomersFileName);
        var slaIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "SLA", CustomersFileName);
        var meterIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "MeterType", CustomersFileName);

        var customers = new List<Customer>();
        var skipped = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var fields = CsvParsingHelper.SplitLine(line);

                if (fields.Length <= Math.Max(Math.Max(idIdx, nameIdx), Math.Max(Math.Max(invoiceIdx, slaIdx), meterIdx)))
                {
                    _logger.LogWarning("customers.csv line {Line}: wrong column count — skipping.", i + 1);
                    skipped++;
                    continue;
                }

                var customerId = fields[idIdx].Trim();
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("customers.csv line {Line}: empty CustomerId — skipping.", i + 1);
                    skipped++;
                    continue;
                }

                var name = fields[nameIdx].Trim();
                var customer = new Customer
                {
                    CustomerId = customerId,
                    Name = string.IsNullOrEmpty(name) ? null : name,
                    HasUnpaidInvoice = CsvParsingHelper.ParseBool(fields[invoiceIdx]),
                    SLA = CsvParsingHelper.ParseEnum<Models.Enums.SlaLevel>(fields[slaIdx]),
                    MeterType = CsvParsingHelper.ParseEnum<Models.Enums.MeterType>(fields[meterIdx])
                };

                customers.Add(customer);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                _logger.LogWarning("customers.csv line {Line}: {Error} — skipping.", i + 1, ex.Message);
                skipped++;
            }
        }

        return (customers, skipped);
    }

    public (IReadOnlyList<Tariff> Tariffs, int Skipped) ReadTariffs(string inputPath)
    {
        var filePath = Path.Combine(inputPath, TariffsFileName);
        EnsureFileExists(filePath);

        var lines = ReadAllLines(filePath);
        if (lines.Count == 0)
            throw new InvalidOperationException($"File '{TariffsFileName}' is empty — header row required.");

        var headers = CsvParsingHelper.SplitLine(lines[0]);
        var idIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "TariffId", TariffsFileName);
        var nameIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "Name", TariffsFileName);
        var smartIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "RequiresSmartMeter", TariffsFileName);
        var priceIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "BaseMonthlyGross", TariffsFileName);

        var tariffs = new List<Tariff>();
        var skipped = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var fields = CsvParsingHelper.SplitLine(line);

                if (fields.Length <= Math.Max(Math.Max(idIdx, nameIdx), Math.Max(smartIdx, priceIdx)))
                {
                    _logger.LogWarning("tariffs.csv line {Line}: wrong column count — skipping.", i + 1);
                    skipped++;
                    continue;
                }

                var tariffId = fields[idIdx].Trim();
                if (string.IsNullOrEmpty(tariffId))
                {
                    _logger.LogWarning("tariffs.csv line {Line}: empty TariffId — skipping.", i + 1);
                    skipped++;
                    continue;
                }

                var tariff = new Tariff
                {
                    TariffId = tariffId,
                    Name = fields[nameIdx].Trim(),
                    RequiresSmartMeter = CsvParsingHelper.ParseBool(fields[smartIdx]),
                    BaseMonthlyGross = CsvParsingHelper.ParseDecimal(fields[priceIdx])
                };

                tariffs.Add(tariff);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                _logger.LogWarning("tariffs.csv line {Line}: {Error} — skipping.", i + 1, ex.Message);
                skipped++;
            }
        }

        return (tariffs, skipped);
    }

    public (IReadOnlyList<SwitchRequest> Valid, IReadOnlyList<(int LineNumber, string RawLine)> Malformed) ReadRequests(string inputPath)
    {
        var filePath = Path.Combine(inputPath, RequestsFileName);
        EnsureFileExists(filePath);

        var lines = ReadAllLines(filePath);
        if (lines.Count == 0)
            throw new InvalidOperationException($"File '{RequestsFileName}' is empty — header row required.");

        var headers = CsvParsingHelper.SplitLine(lines[0]);
        var idIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "RequestId", RequestsFileName);
        var custIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "CustomerId", RequestsFileName);
        var tariffIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "TargetTariffId", RequestsFileName);
        var dateIdx = CsvParsingHelper.GetRequiredHeaderIndex(headers, "RequestedAtISO8601", RequestsFileName);

        var requests = new List<SwitchRequest>();
        var malformed = new List<(int, string)>();

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = CsvParsingHelper.SplitLine(line);

            if (fields.Length <= Math.Max(Math.Max(idIdx, custIdx), Math.Max(tariffIdx, dateIdx)))
            {
                malformed.Add((i + 1, line));
                continue;
            }

            var requestId = fields[idIdx].Trim();
            if (string.IsNullOrEmpty(requestId))
            {
                malformed.Add((i + 1, line));
                continue;
            }

            var customerId = fields[custIdx].Trim();
            var targetTariffId = fields[tariffIdx].Trim();
            var dateStr = fields[dateIdx].Trim();

            if (string.IsNullOrEmpty(targetTariffId) || string.IsNullOrEmpty(dateStr))
            {
                malformed.Add((i + 1, line));
                continue;
            }

            try
            {
                var requestedAt = CsvParsingHelper.ParseDateTimeOffset(dateStr);

                var request = new SwitchRequest
                {
                    RequestId = requestId,
                    CustomerId = customerId,
                    TargetTariffId = targetTariffId,
                    RequestedAt = requestedAt
                };

                requests.Add(request);
            }
            catch (FormatException)
            {
                malformed.Add((i + 1, line));
            }
        }

        return (requests, malformed);
    }

    private static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Required file not found: '{filePath}'", filePath);
    }

    private static IReadOnlyList<string> ReadAllLines(string filePath)
    {
        return File.ReadAllLines(filePath, Encoding.UTF8);
    }
}
