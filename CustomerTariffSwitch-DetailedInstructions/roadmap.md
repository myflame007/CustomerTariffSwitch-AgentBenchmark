# Roadmap — CustomerTariffSwitch

## Implementation Steps

| # | Step | Status | Commit |
|---|------|--------|--------|
| 1 | Create solution structure (sln, projects, references, NuGet packages) | ✅ Done | — |
| 2 | Implement `CustomerTariffSwitch.Models` (enums: SlaLevel, MeterType, DecisionStatus, RejectionReason; records: Customer, Tariff, SwitchRequest, SlaDeadline, RequestDecision) | ✅ Done | — |
| 3 | Implement `CustomerTariffSwitch.Data` (SolutionPathHelper, CsvParsingHelper, CsvReaderService, DecisionRepository) | ✅ Done | — |
| 4 | Implement `CustomerTariffSwitch` host (SlaCalculationService, RequestValidationService, TariffSwitchProcessor, Program.cs with DI) | ✅ Done | — |
| 5 | Copy input CSV files, create `.gitignore` | ✅ Done | — |
| 6 | Implement `CustomerTariffSwitch.Tests` — Unit tests (SlaCalculationServiceTests, RequestValidationServiceTests, CsvParsingHelperTests, DecisionRepositoryTests) | ✅ Done | — |
| 7 | Implement `CustomerTariffSwitch.Tests` — Integration tests (CsvReaderServiceIntegrationTests) | ✅ Done | — |
| 8 | Build verification (`dotnet build --no-incremental`) | ✅ Done | — |
| 9 | Run all tests (`dotnet test`) — 33 unit + 4 integration = 37 total | ✅ Done | — |
| 10 | End-to-end verification: first run produces correct `decisions.json`, second run skips all | ✅ Done | — |

## Test Results

- **Unit tests**: 33 passed
- **Integration tests**: 4 passed
- **Total**: 37 passed, 0 failed

## Notes

- .NET 10 creates `.slnx` instead of `.sln` — `SolutionPathHelper` checks for both.
- `CsvParsingHelper.ParseDecimal` uses `NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign` to reject comma-separated decimals.
- DST spring-forward and fall-back handling verified for Europe/Vienna timezone.
- JSON output uses `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` for readable UTF-8 output (Umlaute, `+` sign).
