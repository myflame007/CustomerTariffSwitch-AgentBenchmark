using CustomerTariffSwitch.Data;
using CustomerTariffSwitch.Models;
using CustomerTariffSwitch.Services;

var csvService = new CsvService();
var decisionRepo = new DecisionRepository();
var processService = new ProcessRequestService();

Console.WriteLine("=== CustomerTariffSwitch ===");
Console.WriteLine();

// 1. Load CSV data
Console.WriteLine("Loading input files...");
var (customers, requests, tariffs, invalidRequests) = csvService.ReadKnownFiles();
Console.WriteLine($"  Customers: {customers.Count}");
Console.WriteLine($"  Tariffs:   {tariffs.Count}");
Console.WriteLine($"  Requests:  {requests.Count} valid, {invalidRequests.Count} invalid");

// 2. Load already-processed request IDs
var processedIds = decisionRepo.LoadProcessedRequestIds();
Console.WriteLine($"  Already processed: {processedIds.Count}");
Console.WriteLine();

// 3. Filter new requests
var newRequests = requests.Where(r => !processedIds.Contains(r.RequestId)).ToList();
if (newRequests.Count == 0 && invalidRequests.Count == 0)
{
    Console.WriteLine("No new requests to process.");
    return;
}

// 4. Process new requests (business logic)
var decisions = processService.ProcessRequests(customers, newRequests, tariffs, []);

// 5. Add rejected decisions for invalid parse results (only if not already processed)
foreach (var inv in invalidRequests)
{
    if (!processedIds.Contains(inv.RawId))
    {
        decisions.Add(RequestDecision.Rejected(inv.RawId, inv.Reason));
    }
}

// 6. Persist decisions
if (decisions.Count > 0)
{
    decisionRepo.AppendDecisions(decisions);
}

// 7. Print results
Console.WriteLine($"Processed {decisions.Count} request(s):");
Console.WriteLine();

foreach (var d in decisions)
{
    if (d.Status == DecisionStatus.Approved)
    {
        Console.WriteLine($"  [APPROVED] {d.RequestId} — Customer: {d.CustomerName}, Due: {d.DueAt:O}");
        if (d.FollowUpAction != null)
            Console.WriteLine($"             Follow-up: {d.FollowUpAction}");
    }
    else
    {
        Console.WriteLine($"  [REJECTED] {d.RequestId} — Reason: {d.Reason}");
    }
}

// 8. Write output file for comparison
var outputDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Output");
CsvService.WriteDecisionsCsv(outputDir, decisions);
Console.WriteLine($"Output written to: {Path.GetFullPath(Path.Combine(outputDir, "decisions.csv"))}");

Console.WriteLine();
Console.WriteLine("Done.");
