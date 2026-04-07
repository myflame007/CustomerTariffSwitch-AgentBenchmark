# CustomerTariffSwitch — One-Shot Implementation Prompt

## Agent Usage

Before starting implementation, spawn the following sub-agents in order:

1. **Software Architect** (`C:\Users\robert.stickler\source\repos\__Development__\AI-Workflow\agency-agents\engineering\engineering-software-architect.md`)
   — Validate the solution structure and layer boundaries defined in this prompt. Raise any concerns before implementation begins.

2. **Backend Architect** (`C:\Users\robert.stickler\source\repos\__Development__\AI-Workflow\agency-agents\engineering\engineering-backend-architect.md`)
   — Own the implementation of all services, repositories, and the processing pipeline.

3. **Code Reviewer** (`C:\Users\robert.stickler\source\repos\__Development__\AI-Workflow\agency-agents\engineering\engineering-code-reviewer.md`)
   — Review each project after it is implemented. Block progression to the next step on any 🔴 issue.

---

## Role

You are a Senior .NET Software Developer.

---

## Task

Implement a .NET 10 console application in C# that automates processing of customer tariff-switch requests. Read structured data from CSV files, apply business validation rules, persist decisions idempotently, and produce a JSON result file.

After analyzing the specification, create a `roadmap.md` at the solution root listing all implementation steps. After each step:
- Verify the solution builds (`dotnet build --no-incremental`)
- Run all tests (`dotnet test`)
- Create a meaningful git commit using **Conventional Commits** format: `feat:`, `fix:`, `test:`, `refactor:`, `chore:` — e.g. `feat: implement SLA calculation with DST handling`
- Record the commit hash and a UTC timestamp next to the completed step in `roadmap.md`

Only proceed to the next step after all checks pass.

---

## Solution Structure

```
CustomerTariffSwitch.sln
|
+-- CustomerTariffSwitch.Models/        # Shared domain types — no dependencies on other projects
|     Models/
|       Customer.cs
|       Tariff.cs
|       SwitchRequest.cs
|       RequestDecision.cs
|     Enums/
|       SlaLevel.cs
|       MeterType.cs
|       DecisionStatus.cs
|
+-- CustomerTariffSwitch.Data/          # I/O layer — depends on Models only
|     Services/
|       CsvReaderService.cs             # Reads and parses all three CSVs
|       DecisionRepository.cs           # Persists decisions.json (atomic, idempotent)
|     Helpers/
|       SolutionPathHelper.cs           # Resolves "Input Files/" and "Output/" relative to solution root
|       CsvParsingHelper.cs             # Shared low-level CSV parsing utilities
|
+-- CustomerTariffSwitch/               # Console host — depends on Data and Models
|     Services/
|       TariffSwitchProcessor.cs        # Orchestrates load → process → persist
|       RequestValidationService.cs     # Applies business rules (Scenarios 1–7)
|       SlaCalculationService.cs        # Computes SLA deadlines in Europe/Vienna
|     Program.cs
|
+-- CustomerTariffSwitch.Tests/         # xUnit — depends on all above projects
|     Unit/
|       RequestValidationServiceTests.cs
|       SlaCalculationServiceTests.cs
|       CsvParsingHelperTests.cs
|       DecisionRepositoryTests.cs
|     Integration/
|       CsvReaderServiceIntegrationTests.cs
```

**Rules:**
- `CustomerTariffSwitch.Models` has **zero** project references.
- `CustomerTariffSwitch.Data` references only `Models`.
- `CustomerTariffSwitch` (host) references `Data` and `Models`.
- No project may introduce a circular dependency.
- Use constructor injection throughout; register all services in `Program.cs` via `Microsoft.Extensions.DependencyInjection`.

---

## Immutability Rules

Apply immutability wherever the object is not mutated after construction:

- All model types (`Customer`, `Tariff`, `SwitchRequest`, `RequestDecision`) must use `init`-only properties and be constructed via constructor or object initializer — never mutated after creation.
- Use `IReadOnlyList<T>` / `IReadOnlyDictionary<K,V>` for collections returned from services.
- Service classes are stateless; all state is passed as parameters.
- `record` types are preferred over `class` for domain models where structural equality makes sense.

---

## Input Files

Location: `Input Files/` folder at the solution root (sibling of the `.sln` file).

### customers.csv

Delimiter: `;` | Encoding: UTF-8 | Header row required

```
CustomerId;Name;HasUnpaidInvoice;SLA;MeterType
C001;Anna Maier;FALSE;Premium;Smart
C002;Stadtcafé GmbH;TRUE;Standard;Classic
C003;Jamal Idris;FALSE;Standard;Classic
C004;Miriam Hölzl;FALSE;Premium;Classic
C005;Bäckerei Schönbrunn KG;FALSE;Standard;Smart
C006;;FALSE;Standard;Smart
;Max Muster;FALSE;Premium;Smart
```

| Column | Type | Notes |
|---|---|---|
| CustomerId | string | Unique key; empty → skip row, log warning |
| Name | string | May be empty — serialize as `null` in output (field is omitted via `WhenWritingNull`) |
| HasUnpaidInvoice | bool | `TRUE`/`FALSE` (case-insensitive) |
| SLA | enum | `Standard` / `Premium` |
| MeterType | enum | `Smart` / `Classic` |

### tariffs.csv

Delimiter: `;` | Encoding: UTF-8 | Header row required

```
TariffId;Name;RequiresSmartMeter;BaseMonthlyGross
T-ECO;ÖkoStrom;TRUE;29.90
T-BASIC;Basis;FALSE;24.50
T-PRO;ProFiX;TRUE;39.00
```

| Column | Type | Notes |
|---|---|---|
| TariffId | string | Unique key; empty → skip row, log warning |
| Name | string | Display name |
| RequiresSmartMeter | bool | `TRUE`/`FALSE` (case-insensitive) |
| BaseMonthlyGross | decimal | Invariant culture (`.` as decimal separator) |

### requests.csv

Delimiter: `;` | Encoding: UTF-8 | Header row required

```
RequestId;CustomerId;TargetTariffId;RequestedAtISO8601
R1001;C001;T-ECO;2025-03-30T01:15:00+01:00
R1002;C002;T-BASIC;2025-10-26T02:05:00+02:00
R1003;C003;T-ECO;2025-10-26T02:30:00+02:00
R1004;C004;T-PRO;2025-06-15T11:20:00+02:00
R1005;C005;T-BASIC;2025-12-29T23:40:00+01:00
R1006;C001;T-UNKNOWN;2025-04-08T09:10:00+02:00
R1007;C001;;2025-06-01T10:00:00+02:00
R1008;C002;T-BASIC;not-a-date
R1009;C001;T-BASIC;
```

| Column | Type | Notes |
|---|---|---|
| RequestId | string | Unique key |
| CustomerId | string | Reference to customers.csv |
| TargetTariffId | string | Reference to tariffs.csv |
| RequestedAtISO8601 | DateTimeOffset | ISO-8601 with UTC offset |

---

## Error Handling — Fail Fast vs. Row-Level

| Situation | Behaviour |
|---|---|
| File missing or unreadable | **Fail fast** — throw, print actionable error, exit non-zero. Do not continue. |
| Required header column missing | **Fail fast** — same as above. |
| customers/tariffs row malformed (empty key, bad enum/bool, wrong column count) | Skip row; write warning to console; requests referencing it receive "Unknown customer" / "Unknown tariff". |
| requests row malformed (empty RequestId, unparseable timestamp, wrong column count) | Reject that request immediately with reason `Invalid request data`; continue with next row. |
| Request already processed (idempotency) | Skip silently; count and report at end. |

**Fail fast means**: no further CSV is read, no processing occurs, no output file is written. The application exits with **exit code 1** and prints a human-readable error message to stderr naming the file and the problem. A successful run exits with **exit code 0**.

---

## Business Rules (Scenarios)

Evaluate in this exact order. Use the first matching scenario.

| # | Condition | Outcome |
|---|---|---|
| 7 | Request row is malformed | Reject — `Invalid request data` |
| 8 | RequestId already present in `decisions.json` | Skip |
| 5 | CustomerId not found in loaded customers | Reject — `Unknown customer` |
| 6 | TargetTariffId not found in loaded tariffs | Reject — `Unknown tariff` |
| 4 | Customer has `HasUnpaidInvoice = true` | Reject — `Unpaid invoice` |
| 3 | Tariff `RequiresSmartMeter = true` AND customer `MeterType = Classic` | Approve + follow-up `Schedule meter upgrade`; SLA due = base SLA + 12 h |
| 1 | Customer `SLA = Standard` | Approve; SLA due = RequestedAt + 48 h |
| 2 | Customer `SLA = Premium` | Approve; SLA due = RequestedAt + 24 h |

---

## SLA Calculation Rules

- **Timezone**: `Europe/Vienna` (IANA). Use `TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna")` on Windows / `"Europe/Vienna"` on Linux; add a fallback to `"W. Europe Standard Time"` for Windows compatibility.
- **Method**: Convert `RequestedAt` to Vienna local time, add the duration (48 h, 24 h, or +12 h extension), then convert back to `DateTimeOffset` with the correct Vienna offset at that point in time.
- **DST spring-forward gap**: If the resulting local time falls in a non-existent hour (e.g., 02:00–03:00 on the last Sunday of March), advance to 03:00 CEST.
- **DST fall-back ambiguity**: If the resulting local time is ambiguous (e.g., 02:00–03:00 on the last Sunday of October), resolve to the later UTC offset (`+01:00`).
- Output all `DueAt` / `FollowUpDueAt` as ISO-8601 with the Vienna offset at that instant.

---

## Idempotency

- On startup, load all `RequestId` values from `Output/decisions.json` into a `HashSet<string>`.
- Before processing any request, check membership. If found → skip (counts as "already processed").
- **Duplicate RequestId within the same run**: the HashSet is updated immediately after each request is processed. If `requests.csv` contains the same `RequestId` twice, the second occurrence is skipped exactly like a cross-run duplicate.
- After processing, atomically append new decisions:
  1. Read existing `decisions.json` (or start with empty array).
  2. Merge new decisions.
  3. Write to a temp file in the same directory.
  4. Atomic rename (replace original).
- `Output/decisions.json` is encoded **UTF-8 without BOM**.
- `Output/` is created automatically if it does not exist. It must be excluded from version control via `.gitignore`.

---

## Output Format

File: `Output/decisions.json` (relative to solution root)

```json
[
  {
    "RequestId": "R1001",
    "Status": 0,
    "CustomerName": "Anna Maier",
    "DueAt": "2025-03-31T01:15:00+02:00"
  },
  {
    "RequestId": "R1002",
    "Status": 1,
    "CustomerName": "Stadtcafé GmbH",
    "Reason": "Unpaid invoice"
  },
  {
    "RequestId": "R1003",
    "Status": 0,
    "CustomerName": "Jamal Idris",
    "DueAt": "2025-10-28T14:30:00+01:00",
    "FollowUpAction": "Schedule meter upgrade",
    "FollowUpDueAt": "2025-10-28T14:30:00+01:00"
  },
  {
    "RequestId": "R1007",
    "Status": 1,
    "Reason": "Invalid request data"
  }
]
```

**Field rules:**
- `Status`: `0` = Approved, `1` = Rejected (integer, not string).
- `CustomerName`: include when the customer was resolved; omit when unknown (e.g. `Unknown customer` / `Unknown tariff` rejections where no customer object exists). For `Invalid request data`, parsing failed before a customer lookup was possible — omit `CustomerName`.
- `Reason`: include only for rejected requests; omit for approved.
- `DueAt`: include only for approved requests; omit for rejected.
- `FollowUpAction` / `FollowUpDueAt`: include only when a meter upgrade follow-up exists; omit otherwise.
- Use `JsonSerializerOptions` with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`.
- Serialize with indentation (`WriteIndented = true`).
- Encoding: **UTF-8 without BOM**.

---

## Console Output

```
=== CustomerTariffSwitch ===

Loading CSV files...
  customers.csv — 5 rows loaded (2 skipped)
  tariffs.csv   — 3 rows loaded
  requests.csv  — 9 rows loaded

Processing requests...
------------------------------------------------------------
  [SKIPPED]  R1001 — already processed
  [APPROVED] R1003 | Jamal Idris        | Due: 2025-10-28T14:30:00+01:00 | Follow-up: Schedule meter upgrade
  [REJECTED] R1006 | -                  | Reason: Unknown tariff
  [REJECTED] R1007 | Anna Maier         | Reason: Invalid request data
------------------------------------------------------------
  => 1 approved, 2 rejected, 1 skipped

  Decisions written to: C:\...\Output\decisions.json

  Exit code: 0
```

On a second run with no new rows:

```
  => 9 request(s) skipped — all already processed.
  => Nothing new to process. To reprocess from scratch, delete the Output/ folder.
```

---

## Unit Tests

Use **xUnit**. Cover at minimum:

### `SlaCalculationServiceTests`
- Standard SLA → +48 h, correct Vienna offset
- Premium SLA → +24 h, correct Vienna offset
- Meter upgrade → base SLA + 12 h
- DST spring-forward: input `2025-03-30T01:15:00+01:00` + 48 h → expected `2025-04-01T01:15:00+02:00`
- DST fall-back: input `2025-10-26T02:30:00+02:00` + 48 h → expected `2025-10-28T02:30:00+01:00` (resolves to later offset)

### `RequestValidationServiceTests`
- Scenario 3: tariff requires smart meter, customer has Classic → Approved + follow-up
- Scenario 4: unpaid invoice → Rejected with correct reason
- Scenario 5: unknown customer → Rejected
- Scenario 6: unknown tariff → Rejected
- Scenario ordering: unpaid invoice takes priority over meter upgrade check
- Approved with Premium SLA, Smart meter already installed → no follow-up

### `CsvParsingHelperTests`
- Bool parsing: `TRUE`, `true`, `False`, `FALSE`, `1`, `0` → correct bool; invalid value → exception/null
- Enum parsing: valid value (case-insensitive) → correct enum; invalid value → exception/null
- Decimal parsing: `29.90` with invariant culture → `29.90m`; comma-decimal → exception

### `DecisionRepositoryTests`
- Load from non-existent file → empty `HashSet`
- Append to existing file → all previous entries retained, new entries added
- Idempotency: appending a RequestId that already exists does not duplicate it
- Atomic write: simulated crash mid-write does not corrupt the existing file (verify temp-file strategy)

### Integration Tests (Category=Integration)

`CsvReaderServiceIntegrationTests` — reads the real files from `Input Files/`:
- All valid rows from `customers.csv` are loaded; malformed rows are skipped without exception
- All valid rows from `tariffs.csv` are loaded
- All rows from `requests.csv` are loaded (including malformed ones, which surface as parse failures)
- Fail-fast: if a required column is missing, a descriptive exception is thrown

Run unit tests with:
```bash
dotnet test --filter "Category!=Integration"
```

Run integration tests with:
```bash
dotnet test --filter "Category=Integration"
```

Tag integration test classes with `[Trait("Category", "Integration")]`.

---

## Code Quality Standards

### Readability (highest priority)
- Methods ≤ 30 lines; one responsibility per method.
- Name everything for what it *means*, not what it *does* — `ApprovedDecision`, not `BuildResult`.
- No magic strings anywhere — every literal that carries domain meaning must be a constant or enum value.
- No clever one-liners that sacrifice clarity. Prefer explicit over terse.
- Avoid deep nesting; extract guard clauses and helper methods early.

### Enums
Use an enum wherever a value belongs to a fixed, named set:
- `SlaLevel` (`Standard`, `Premium`)
- `MeterType` (`Smart`, `Classic`)
- `DecisionStatus` (`Approved`, `Rejected`)
- `RejectionReason` (`UnpaidInvoice`, `UnknownCustomer`, `UnknownTariff`, `InvalidRequestData`)

Never use raw strings for these values inside business logic.

### Dedicated classes
Create a dedicated class (or record) wherever a concept has a name and cohesion — do not pass primitives when a typed wrapper is clearer:
- `SlaDeadline` (or similar) to carry `DueAt` + optional `FollowUpDueAt` together
- `ProcessingResult` / `RequestDecision` to represent the outcome of a single request
- Do not collapse multiple return values into tuples or `out` parameters if a named type is more readable

### Other rules
- **Immutability**: all model records with `init`-only properties; no `set` on domain types.
- **Service orientation**: each service does one job; no service reaches into another service's internals.
- **Dependency injection**: no `new SomeService()` calls inside services; resolve via constructor.
- **No defensive over-engineering**: do not add retry logic, caching, or configuration abstractions beyond what this spec requires.
- **Helpers**: move reusable, low-level utilities (path resolution, CSV field parsing) into `Helpers/` — not into services.
- **No commented-out code, no TODOs** in the committed implementation.

---

## Build & Run

```bash
# from solution root
dotnet build
dotnet run --project CustomerTariffSwitch

# reset and reprocess
rm -rf Output/
dotnet run --project CustomerTariffSwitch
```

Target framework: **net10.0**.

Allowed NuGet packages:
- `xUnit` and `xunit.runner.visualstudio` for testing
- `Microsoft.Extensions.Logging` + `Microsoft.Extensions.Logging.Console` for structured console logging
- `System.Text.Json` (inbox — no package reference needed)
- Built-in CSV parsing via `StreamReader` — no CsvHelper or similar

---

## Reprocessing

The `Output/` folder is git-ignored. To reprocess all requests from scratch:

```bash
# Unix / Git Bash
rm -rf Output/

# Windows
rmdir /s /q Output
```

Then re-run the application.
