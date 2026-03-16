using System.Globalization;
using CustomerTariffSwitch.Models;

namespace CustomerTariffSwitch.Data;

public class CsvService
{
    private const char Separator = ';';

    public (List<Customer> Customers, List<SwitchRequest> Requests, List<Tariff> Tariffs, List<(string RawId, string Reason)> InvalidRequests) ReadKnownFiles()
    {
        var inputDir = FindInputFilesDirectory();

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
        if (rows.Count == 0)
            return [];

        var results = new List<Customer>();

        // Skip header row
        for (int i = 1; i < rows.Count; i++)
        {
            var cols = rows[i];
            if (cols.Length < 5)
                continue;

            var hasUnpaid = cols[2].Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
            if (!Enum.TryParse<SLALevel>(cols[3].Trim(), ignoreCase: true, out var sla))
                continue;
            if (!Enum.TryParse<MeterType>(cols[4].Trim(), ignoreCase: true, out var meter))
                continue;

            results.Add(new Customer
            {
                CustomerId = cols[0].Trim(),
                Name = cols[1].Trim(),
                HasUnpaidInvoice = hasUnpaid,
                Sla = sla,
                MeterType = meter
            });
        }

        return results;
    }

    public static (List<SwitchRequest> Valid, List<(string RawId, string Reason)> Invalid) ParseRequests(List<string[]> rows)
    {
        if (rows.Count == 0)
            return ([], []);

        var valid = new List<SwitchRequest>();
        var invalid = new List<(string RawId, string Reason)>();

        for (int i = 1; i < rows.Count; i++)
        {
            var cols = rows[i];
            if (cols.Length < 4)
            {
                var rawId = cols.Length > 0 ? cols[0].Trim() : $"Row{i}";
                invalid.Add((rawId, "Invalid request data"));
                continue;
            }

            var requestId = cols[0].Trim();
            var customerId = cols[1].Trim();
            var tariffId = cols[2].Trim();
            var timestampRaw = cols[3].Trim();

            if (string.IsNullOrWhiteSpace(requestId) ||
                string.IsNullOrWhiteSpace(customerId) ||
                string.IsNullOrWhiteSpace(tariffId) ||
                string.IsNullOrWhiteSpace(timestampRaw))
            {
                invalid.Add((requestId, "Invalid request data"));
                continue;
            }

            if (!DateTimeOffset.TryParse(timestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var requestedAt))
            {
                invalid.Add((requestId, "Invalid request data"));
                continue;
            }

            valid.Add(new SwitchRequest
            {
                RequestId = requestId,
                CustomerId = customerId,
                TargetTariffId = tariffId,
                RequestedAt = requestedAt
            });
        }

        return (valid, invalid);
    }

    public static List<Tariff> ParseTariffs(List<string[]> rows)
    {
        if (rows.Count == 0)
            return [];

        var results = new List<Tariff>();

        for (int i = 1; i < rows.Count; i++)
        {
            var cols = rows[i];
            if (cols.Length < 4)
                continue;

            var requiresSmart = cols[2].Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

            if (!decimal.TryParse(cols[3].Trim(), CultureInfo.InvariantCulture, out var price))
                continue;

            results.Add(new Tariff
            {
                TariffId = cols[0].Trim(),
                Name = cols[1].Trim(),
                RequiresSmartMeter = requiresSmart,
                BaseMonthlyGross = price
            });
        }

        return results;
    }

    private static List<string[]> ReadCsvRows(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"CSV file not found: {filePath}");

        var lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
        if (lines.Length == 0)
            throw new InvalidDataException($"CSV file is empty: {filePath}");

        return lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(Separator))
            .ToList();
    }

    public static void WriteDecisionsCsv(string outputDir, IReadOnlyCollection<RequestDecision> decisions)
    {
        Directory.CreateDirectory(outputDir);
        var filePath = Path.Combine(outputDir, "decisions.csv");

        var lines = new List<string>
        {
            string.Join(Separator, "RequestId", "Status", "CustomerName", "DueAt", "FollowUpAction", "Reason")
        };

        foreach (var d in decisions)
        {
            var dueAt = d.DueAt?.ToString("o", CultureInfo.InvariantCulture) ?? "";
            lines.Add(string.Join(Separator,
                d.RequestId,
                d.Status,
                d.CustomerName ?? "",
                dueAt,
                d.FollowUpAction ?? "",
                d.Reason ?? ""));
        }

        File.WriteAllLines(filePath, lines, System.Text.Encoding.UTF8);
    }

    private static string FindInputFilesDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Input Files");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find 'Input Files' directory. Ensure the CSV files are in an 'Input Files' folder at the project root.");
    }
}
