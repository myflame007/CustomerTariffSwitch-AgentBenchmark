using CustomerTariffSwitch.Data.Helpers;
using CustomerTariffSwitch.Data.Services;
using CustomerTariffSwitch.Models.Enums;
using CustomerTariffSwitch.Models.Models;
using Microsoft.Extensions.Logging;

namespace CustomerTariffSwitch.Services;

public class TariffSwitchProcessor
{
    private readonly CsvReaderService _csvReaderService;
    private readonly DecisionRepository _decisionRepository;
    private readonly RequestValidationService _validationService;
    private readonly ILogger<TariffSwitchProcessor> _logger;

    public TariffSwitchProcessor(
        CsvReaderService csvReaderService,
        DecisionRepository decisionRepository,
        RequestValidationService validationService,
        ILogger<TariffSwitchProcessor> logger)
    {
        _csvReaderService = csvReaderService;
        _decisionRepository = decisionRepository;
        _validationService = validationService;
        _logger = logger;
    }

    public int Run()
    {
        Console.WriteLine("=== CustomerTariffSwitch ===");
        Console.WriteLine();

        var inputPath = SolutionPathHelper.GetInputFilesPath();

        Console.WriteLine("Loading CSV files...");
        var (customers, customersSkipped) = _csvReaderService.ReadCustomers(inputPath);
        var (tariffs, tariffsSkipped) = _csvReaderService.ReadTariffs(inputPath);
        var (validRequests, malformedRequests) = _csvReaderService.ReadRequests(inputPath);

        Console.WriteLine($"  customers.csv — {customers.Count} rows loaded ({customersSkipped} skipped)");
        Console.WriteLine($"  tariffs.csv   — {tariffs.Count} rows loaded");
        Console.WriteLine($"  requests.csv  — {validRequests.Count + malformedRequests.Count} rows loaded");

        var customerLookup = BuildCustomerLookup(customers);
        var tariffLookup = BuildTariffLookup(tariffs);

        var processedIds = _decisionRepository.LoadProcessedRequestIds();
        var existingDecisions = _decisionRepository.LoadExistingDecisions();

        Console.WriteLine();
        Console.WriteLine("Processing requests...");
        Console.WriteLine(new string('-', 60));

        var newDecisions = new List<RequestDecision>();
        int approved = 0, rejected = 0, skipped = 0;

        ProcessMalformedRequests(malformedRequests, processedIds, newDecisions, ref rejected, ref skipped);
        ProcessValidRequests(validRequests, customerLookup, tariffLookup, processedIds, newDecisions, ref approved, ref rejected, ref skipped);

        Console.WriteLine(new string('-', 60));

        if (newDecisions.Count == 0)
        {
            var totalSkipped = validRequests.Count + malformedRequests.Count;
            Console.WriteLine($"  => {totalSkipped} request(s) skipped — all already processed.");
            Console.WriteLine("  => Nothing new to process. To reprocess from scratch, delete the Output/ folder.");
        }
        else
        {
            Console.WriteLine($"  => {approved} approved, {rejected} rejected, {skipped} skipped");

            var allDecisions = existingDecisions.Concat(newDecisions).ToList();
            _decisionRepository.SaveDecisions(allDecisions);
        }

        Console.WriteLine();
        Console.WriteLine("  Exit code: 0");
        return 0;
    }

    private void ProcessMalformedRequests(
        IReadOnlyList<(int LineNumber, string RawLine)> malformedRequests,
        HashSet<string> processedIds,
        List<RequestDecision> newDecisions,
        ref int rejected,
        ref int skipped)
    {
        foreach (var (lineNumber, rawLine) in malformedRequests)
        {
            var fields = rawLine.Split(';');
            var requestId = fields.Length > 0 ? fields[0].Trim() : "";

            if (string.IsNullOrEmpty(requestId))
            {
                var decision = _validationService.ProcessMalformedRequest($"LINE_{lineNumber}");
                newDecisions.Add(decision);
                rejected++;
                PrintRejected(decision.RequestId, null, decision.Reason!);
                continue;
            }

            if (processedIds.Contains(requestId))
            {
                PrintSkipped(requestId);
                skipped++;
                continue;
            }

            var malformedDecision = _validationService.ProcessMalformedRequest(requestId);

            var customerName = ResolveCustomerNameForMalformed(fields, requestId);
            if (customerName is not null)
            {
                malformedDecision = malformedDecision with { CustomerName = customerName };
            }

            newDecisions.Add(malformedDecision);
            processedIds.Add(requestId);
            rejected++;
            PrintRejected(requestId, malformedDecision.CustomerName, malformedDecision.Reason!);
        }
    }

    private string? ResolveCustomerNameForMalformed(string[] fields, string requestId)
    {
        return null;
    }

    private void ProcessValidRequests(
        IReadOnlyList<SwitchRequest> requests,
        IReadOnlyDictionary<string, Customer> customerLookup,
        IReadOnlyDictionary<string, Tariff> tariffLookup,
        HashSet<string> processedIds,
        List<RequestDecision> newDecisions,
        ref int approved,
        ref int rejected,
        ref int skipped)
    {
        foreach (var request in requests)
        {
            if (processedIds.Contains(request.RequestId))
            {
                PrintSkipped(request.RequestId);
                skipped++;
                continue;
            }

            var decision = _validationService.ProcessRequest(request, customerLookup, tariffLookup);
            newDecisions.Add(decision);
            processedIds.Add(request.RequestId);

            if (decision.Status == DecisionStatus.Approved)
            {
                approved++;
                PrintApproved(decision);
            }
            else
            {
                rejected++;
                PrintRejected(decision.RequestId, decision.CustomerName, decision.Reason!);
            }
        }
    }

    private static void PrintSkipped(string requestId)
    {
        Console.WriteLine($"  [SKIPPED]  {requestId} — already processed");
    }

    private static void PrintApproved(RequestDecision decision)
    {
        var name = decision.CustomerName ?? "-";
        var followUp = decision.FollowUpAction is not null
            ? $" | Follow-up: {decision.FollowUpAction}"
            : "";
        Console.WriteLine($"  [APPROVED] {decision.RequestId} | {name,-18} | Due: {decision.DueAt}{followUp}");
    }

    private static void PrintRejected(string requestId, string? customerName, string reason)
    {
        var name = customerName ?? "-";
        Console.WriteLine($"  [REJECTED] {requestId} | {name,-18} | Reason: {reason}");
    }

    private static IReadOnlyDictionary<string, Customer> BuildCustomerLookup(IReadOnlyList<Customer> customers)
    {
        return customers.ToDictionary(c => c.CustomerId, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, Tariff> BuildTariffLookup(IReadOnlyList<Tariff> tariffs)
    {
        return tariffs.ToDictionary(t => t.TariffId, StringComparer.Ordinal);
    }
}
