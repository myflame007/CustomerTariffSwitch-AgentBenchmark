# CustomerTariffSwitch — Implementation Roadmap

| # | Step | Status | Timestamp | Commit Hash |
|---|------|--------|-----------|-------------|
| 1 | Create solution & project structure (.sln, .csproj) | ✅ Done | 2026-03-16 | 6373467 |
| 2 | Create model classes (enums, Customer, Tariff, SwitchRequest, RequestDecision) | ✅ Done | 2026-03-16 | 6373467 |
| 3 | Implement CsvService (ParseCustomers, ParseRequests, ParseTariffs, ReadKnownFiles) | ✅ Done | 2026-03-16 | 6373467 |
| 4 | Implement ProcessRequestService (CalculateSlaHours, ProcessRequests, DST handling) | ✅ Done | 2026-03-16 | 6373467 |
| 5 | Implement DecisionRepository (JSON persistence, idempotent writes, atomic file ops) | ✅ Done | 2026-03-16 | 6373467 |
| 6 | Implement Program.cs (console app orchestration) | ✅ Done | 2026-03-16 | 6373467 |
| 7 | Final integration test pass & cleanup — 43/43 tests passing | ✅ Done | 2026-03-16 | 6373467 |
