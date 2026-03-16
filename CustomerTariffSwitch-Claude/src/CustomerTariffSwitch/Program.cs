using CustomerTariffSwitch.Models;
using CustomerTariffSwitch.Parsing;
using CustomerTariffSwitch.Services;

// Resolve paths relative to the solution root and shared input directory.
var baseDir = FindProjectRoot() ?? Environment.CurrentDirectory;
var inputDir = FindInputDir() ?? Path.Combine(baseDir, "Input Files");
var outputDir = Path.Combine(baseDir, "Output");
Directory.CreateDirectory(outputDir);
var resultsFile = Path.Combine(outputDir, "decisions.json");
var reportFile = Path.Combine(outputDir, "report.csv");

static string? FindProjectRoot()
{
    var dir = AppContext.BaseDirectory;
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir, "CustomerTariffSwitch.slnx")))
            return dir;
        dir = Path.GetDirectoryName(dir);
    }
    return null;
}

static string? FindInputDir()
{
    var dir = AppContext.BaseDirectory;
    while (dir is not null)
    {
        var candidate = Path.Combine(dir, "Input Files");
        if (Directory.Exists(candidate))
            return candidate;
        dir = Path.GetDirectoryName(dir);
    }
    return null;
}

try
{
    Console.WriteLine($"[{DateTimeOffset.Now:o}] === Customer Tariff Switch — Starting ===");
    Console.WriteLine($"[{DateTimeOffset.Now:o}] Base directory: {baseDir}");
    Console.WriteLine($"[{DateTimeOffset.Now:o}] Input directory: {inputDir}");
    Console.WriteLine($"[{DateTimeOffset.Now:o}] Results file:   {resultsFile}");
    Console.WriteLine();

    // --- Load CSVs (schema/file errors fail fast) ---
    Console.WriteLine($"[{DateTimeOffset.Now:o}] Loading CSV files...");
    var customers = CsvParser.LoadCustomers(Path.Combine(inputDir, "customers.csv"));
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   customers.csv  -> {customers.Count} records");
    var tariffs = CsvParser.LoadTariffs(Path.Combine(inputDir, "tariffs.csv"));
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   tariffs.csv    -> {tariffs.Count} records");
    var rawRequests = CsvParser.LoadRawRequests(Path.Combine(inputDir, "requests.csv"));
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   requests.csv   -> {rawRequests.Count} records");
    Console.WriteLine();

    // --- Load processing store (idempotency) ---
    var store = new ProcessingStore(resultsFile);
    Console.WriteLine($"[{DateTimeOffset.Now:o}] Processing store loaded — {store.Results.Count} previously processed request(s).");
    Console.WriteLine();

    var processor = new TariffSwitchProcessor(customers, tariffs);

    int approved = 0, rejected = 0, skippedCount = 0;

    Console.WriteLine($"[{DateTimeOffset.Now:o}] Processing requests...");
    foreach (var raw in rawRequests)
    {
        if (store.IsProcessed(raw.RequestId))
        {
            Console.WriteLine($"[{DateTimeOffset.Now:o}]   [SKIP] {raw.RequestId} — already processed");
            skippedCount++;
            continue;
        }

        var result = processor.Process(raw);
        store.Add(result);

        if (result.Status == RequestStatus.Approved)
        {
            approved++;
            Console.WriteLine($"[{DateTimeOffset.Now:o}]   [APPROVED] {result.RequestId}" +
                $" | Customer: {result.CustomerId} -> Tariff: {result.TargetTariffId}" +
                $" | SLA due: {result.SlaDueDate:o}" +
                (result.FollowUpAction is not null ? $" | Action: {result.FollowUpAction}" : ""));
        }
        else
        {
            rejected++;
            Console.WriteLine($"[{DateTimeOffset.Now:o}]   [REJECTED] {result.RequestId} — {result.RejectionReason}");
        }
    }

    store.Save();

    // --- Write CSV report ---
    WriteCsvReport(reportFile, store.Results);
    Console.WriteLine($"[{DateTimeOffset.Now:o}] CSV report written to '{Path.GetFullPath(reportFile)}'.");

    Console.WriteLine();
    Console.WriteLine($"[{DateTimeOffset.Now:o}] === Summary ===");
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   Approved: {approved}");
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   Rejected: {rejected}");
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   Skipped (already processed): {skippedCount}");
    Console.WriteLine($"[{DateTimeOffset.Now:o}]   Total:    {approved + rejected + skippedCount}");
    Console.WriteLine($"[{DateTimeOffset.Now:o}] Results persisted to '{Path.GetFullPath(resultsFile)}'.");
    Console.WriteLine($"[{DateTimeOffset.Now:o}] === Done ===");
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"[{DateTimeOffset.Now:o}] FATAL: {ex.Message}");
    Environment.Exit(1);
}
catch (InvalidOperationException ex) when (ex.Message.StartsWith("CSV schema error"))
{
    Console.Error.WriteLine($"[{DateTimeOffset.Now:o}] FATAL: {ex.Message}");
    Environment.Exit(1);
}

static void WriteCsvReport(string path, IReadOnlyList<ProcessingResult> results)
{
    using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
    writer.WriteLine("RequestId;Status;CustomerId;TargetTariffId;RequestedAt;SlaDueDate;RejectionReason;FollowUpAction;ProcessedAt");
    foreach (var r in results)
    {
        writer.WriteLine(string.Join(";",
            r.RequestId,
            r.Status,
            r.CustomerId ?? "",
            r.TargetTariffId ?? "",
            r.RequestedAt?.ToString("o") ?? "",
            r.SlaDueDate?.ToString("o") ?? "",
            r.RejectionReason ?? "",
            r.FollowUpAction ?? "",
            r.ProcessedAt.ToString("o")));
    }
}
