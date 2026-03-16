using System.Globalization;
using System.Text;
using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Services;

public class CsvService
{
    private const char Separator = ';';

    public (List<Customer> Customers, List<SwitchRequest> Requests, List<Tariff> Tariffs, List<(string RawId, string Reason)> InvalidRequests) ReadKnownFiles()
    {
        var inputDir = FindInputDirectory();

        var customerRows = ReadCsvRows(Path.Combine(inputDir, "customers.csv"));
        var requestRows = ReadCsvRows(Path.Combine(inputDir, "requests.csv"));
        var tariffRows = ReadCsvRows(Path.Combine(inputDir, "tariffs.csv"));

        var customers = ParseCustomers(customerRows);
        var (validRequests, invalidRequests) = ParseRequests(requestRows);
        var tariffs = ParseTariffs(tariffRows);

        return (customers, validRequests, tariffs, invalidRequests);
    }

    public static List<Customer> ParseCustomers(List<string[]> rows)
    {
        if (rows.Count <= 1) return [];

        var result = new List<Customer>();

        foreach (var cols in rows.Skip(1))
        {
            if (cols.Length < 5) continue;

            result.Add(new Customer
            {
                CustomerId = cols[0].Trim(),
                Name = cols[1].Trim(),
                HasUnpaidInvoice = cols[2].Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase),
                Sla = Enum.Parse<SLALevel>(cols[3].Trim(), ignoreCase: true),
                MeterType = Enum.Parse<MeterType>(cols[4].Trim(), ignoreCase: true)
            });
        }

        return result;
    }

    public static (List<SwitchRequest> Valid, List<(string RawId, string Reason)> Invalid) ParseRequests(List<string[]> rows)
    {
        if (rows.Count <= 1) return ([], []);

        var valid = new List<SwitchRequest>();
        var invalid = new List<(string RawId, string Reason)>();

        foreach (var cols in rows.Skip(1))
        {
            var rawId = cols.Length > 0 ? cols[0].Trim() : "";

            if (cols.Length < 4
                || string.IsNullOrWhiteSpace(cols[0])
                || string.IsNullOrWhiteSpace(cols[1])
                || string.IsNullOrWhiteSpace(cols[2])
                || string.IsNullOrWhiteSpace(cols[3]))
            {
                invalid.Add((rawId, "Invalid request data"));
                continue;
            }

            if (!DateTimeOffset.TryParse(cols[3].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var requestedAt))
            {
                invalid.Add((rawId, "Invalid request data"));
                continue;
            }

            valid.Add(new SwitchRequest
            {
                RequestId = cols[0].Trim(),
                CustomerId = cols[1].Trim(),
                TargetTariffId = cols[2].Trim(),
                RequestedAt = requestedAt
            });
        }

        return (valid, invalid);
    }

    public static List<Tariff> ParseTariffs(List<string[]> rows)
    {
        if (rows.Count <= 1) return [];

        var result = new List<Tariff>();

        foreach (var cols in rows.Skip(1))
        {
            if (cols.Length < 4) continue;

            result.Add(new Tariff
            {
                TariffId = cols[0].Trim(),
                Name = cols[1].Trim(),
                RequiresSmartMeter = cols[2].Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase),
                BaseMonthlyGross = decimal.Parse(cols[3].Trim(), CultureInfo.InvariantCulture)
            });
        }

        return result;
    }

    private static List<string[]> ReadCsvRows(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Required CSV file not found: {path}");

        var lines = File.ReadAllLines(path, Encoding.UTF8);

        if (lines.Length == 0)
            throw new InvalidOperationException($"CSV file is empty: {path}");

        return lines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(Separator))
            .ToList();
    }

    private static string FindInputDirectory()
    {
        var dir = Directory.GetCurrentDirectory();

        while (dir != null)
        {
            var candidate = Path.Combine(dir, "Input Files");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new DirectoryNotFoundException(
            "Could not find 'Input Files' directory. Ensure it exists relative to the working directory.");
    }
}
