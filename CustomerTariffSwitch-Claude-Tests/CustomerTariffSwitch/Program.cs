using System.Diagnostics;
using CustomerTariffSwitch.Models;
using CustomerTariffSwitch.Services;

var sw = Stopwatch.StartNew();
Console.WriteLine("=== CustomerTariffSwitch ===");
Console.WriteLine($"Run started at {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
Console.WriteLine();

// --- Load CSV data ---
Console.WriteLine("[1/4] Loading CSV data...");
var csvService = new CsvService();
var (customers, validRequests, tariffs, invalidRequests) = csvService.ReadKnownFiles();
Console.WriteLine($"      Customers loaded:       {customers.Count}");
Console.WriteLine($"      Tariffs loaded:         {tariffs.Count}");
Console.WriteLine($"      Valid requests loaded:  {validRequests.Count}");
Console.WriteLine($"      Invalid requests found: {invalidRequests.Count}");
foreach (var inv in invalidRequests)
    Console.WriteLine($"        - {inv.RawId}: {inv.Reason}");
Console.WriteLine();

// --- Load previous decisions ---
Console.WriteLine("[2/4] Checking previously processed requests...");
var decisionRepo = new DecisionRepository();
var processedIds = decisionRepo.LoadProcessedRequestIds();
Console.WriteLine($"      Already processed: {processedIds.Count}");
if (processedIds.Count > 0)
    Console.WriteLine($"      IDs: {string.Join(", ", processedIds)}");
Console.WriteLine();

// --- Determine new work ---
Console.WriteLine("[3/4] Processing new requests...");
var invalidDecisions = invalidRequests
    .Where(r => !processedIds.Contains(r.RawId))
    .Select(r => RequestDecision.Rejected(r.RawId, r.Reason))
    .ToList();

var newRequests = validRequests
    .Where(r => !processedIds.Contains(r.RequestId))
    .ToList();

Console.WriteLine($"      New valid requests:   {newRequests.Count}");
Console.WriteLine($"      New invalid requests: {invalidDecisions.Count}");
Console.WriteLine($"      Skipped (already processed): {validRequests.Count - newRequests.Count + invalidRequests.Count - invalidDecisions.Count}");

var processRequestService = new ProcessRequestService();
var validDecisions = processRequestService.ProcessRequests(customers, newRequests, tariffs, []);

var allNewDecisions = invalidDecisions.Concat(validDecisions).ToList();

if (allNewDecisions.Count == 0)
{
    Console.WriteLine("      Nothing new to process — all requests already handled.");
    Console.WriteLine();
    Console.WriteLine($"=== Finished in {sw.ElapsedMilliseconds} ms ===");
    return;
}
Console.WriteLine();

// --- Persist and report ---
Console.WriteLine("[4/4] Persisting decisions...");
decisionRepo.AppendDecisions(allNewDecisions);
Console.WriteLine($"      Saved {allNewDecisions.Count} decision(s) to decisions.json");
Console.WriteLine();

var approved = allNewDecisions.Where(d => d.Status == DecisionStatus.Approved).ToList();
var rejected = allNewDecisions.Where(d => d.Status == DecisionStatus.Rejected).ToList();

Console.WriteLine($"--- Results: {approved.Count} approved, {rejected.Count} rejected ---");
Console.WriteLine();

if (approved.Count > 0)
{
    Console.WriteLine("  Approved:");
    foreach (var d in approved)
    {
        Console.WriteLine($"    {d.RequestId} | {d.CustomerName} | Due: {d.DueAt:o}");
        if (d.FollowUpAction != null)
            Console.WriteLine($"      -> Follow-up: {d.FollowUpAction} (deadline: {d.DueAt:o})");
    }
    Console.WriteLine();
}

if (rejected.Count > 0)
{
    Console.WriteLine("  Rejected:");
    foreach (var d in rejected)
        Console.WriteLine($"    {d.RequestId} | Reason: {d.Reason}");
    Console.WriteLine();
}

// --- Write output report file ---
var projectRoot = FindProjectRoot();
var outputDir = Path.Combine(projectRoot, "Output");
Directory.CreateDirectory(outputDir);
var reportPath = Path.Combine(outputDir, "report.txt");

static string FindProjectRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (dir.GetFiles("*.slnx").Length > 0 || dir.GetFiles("*.sln").Length > 0)
            return dir.FullName;
        dir = dir.Parent;
    }
    return Directory.GetCurrentDirectory();
}
using (var writer = new StreamWriter(reportPath, false, System.Text.Encoding.UTF8))
{
    writer.WriteLine($"CustomerTariffSwitch — Run Report");
    writer.WriteLine($"Generated: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
    writer.WriteLine(new string('=', 60));
    writer.WriteLine();
    writer.WriteLine($"Input Summary");
    writer.WriteLine($"  Customers:        {customers.Count}");
    writer.WriteLine($"  Tariffs:          {tariffs.Count}");
    writer.WriteLine($"  Valid requests:   {validRequests.Count}");
    writer.WriteLine($"  Invalid requests: {invalidRequests.Count}");
    writer.WriteLine($"  Previously processed: {processedIds.Count}");
    writer.WriteLine();
    writer.WriteLine($"This Run: {allNewDecisions.Count} decision(s)");
    writer.WriteLine($"  Approved: {approved.Count}");
    writer.WriteLine($"  Rejected: {rejected.Count}");
    writer.WriteLine(new string('-', 60));
    writer.WriteLine();

    if (approved.Count > 0)
    {
        writer.WriteLine("APPROVED");
        writer.WriteLine($"{"RequestId",-12} {"Customer",-30} {"DueAt",-35} {"Follow-Up"}");
        writer.WriteLine(new string('-', 100));
        foreach (var d in approved)
            writer.WriteLine($"{d.RequestId,-12} {d.CustomerName,-30} {d.DueAt:o}  {d.FollowUpAction ?? "-"}");
        writer.WriteLine();
    }

    if (rejected.Count > 0)
    {
        writer.WriteLine("REJECTED");
        writer.WriteLine($"{"RequestId",-12} {"Reason"}");
        writer.WriteLine(new string('-', 60));
        foreach (var d in rejected)
            writer.WriteLine($"{d.RequestId,-12} {d.Reason}");
        writer.WriteLine();
    }
}
Console.WriteLine($"Report written to: {reportPath}");
Console.WriteLine();

sw.Stop();
Console.WriteLine($"=== Finished in {sw.ElapsedMilliseconds} ms ===");
