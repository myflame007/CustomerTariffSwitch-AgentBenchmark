# Git-History — CustomerTariffSwitch-Claude (P2)

> Claude aus Instructions.md — ohne Tests, ohne Agent-Workflow

## Commit-Übersicht (12 Commits)

| # | Datum | Commit | Nachricht |
|---|-------|--------|-----------|
| 1 | 2026-03-16 19:42 | `d34c312` | init |
| 2 | 2026-03-16 19:46 | `a160387` | Scaffold .NET solution with console app and xUnit test project |
| 3 | 2026-03-16 20:27 | `743be12` | Implement CSV models and parsing (customers, tariffs, requests) |
| 4 | 2026-03-16 20:29 | `ed6c994` | Implement business logic, SLA computation, idempotent processing, and orchestration |
| 5 | 2026-03-16 20:30 | `7c461a7` | Add unit tests for SLA calculator, tariff switch processor, and processing store |
| 6 | 2026-03-16 20:31 | `5592b70` | Final cleanup: exclude runtime output from git, update roadmap |
| 7 | 2026-03-16 20:44 | `dd4b735` | Add timestamped console logs and update .gitignore |
| 8 | 2026-03-16 20:48 | `d1b6394` | Add CSV output report and exclude it from git |
| 9 | 2026-03-16 21:13 | `c141432` | Add metrik.md with development metrics |
| 10 | 2026-03-16 21:20 | `59a726d` | Remove misleading duration from metrik.md |
| 11 | 2026-03-16 21:23 | `638ca3d` | Add chat workflow metrics to metrik.md |
| 12 | 2026-03-16 23:33 | `22d2d2d` | Standardize input/output paths: shared Input Files at root, Output/decisions.json per project |

## Detaillierte Commit-Historie

### 1. `d34c312` — init
- **Datum:** 2026-03-16 19:42:59
- **Autor:** Robert Stickler
- Initialer leerer Commit

### 2. `a160387` — Scaffold .NET solution with console app and xUnit test project
- **Datum:** 2026-03-16 19:46:06
- Projektstruktur erstellt: Console-App + xUnit-Testprojekt
- Solution-Datei (.slnx)

### 3. `743be12` — Implement CSV models and parsing
- **Datum:** 2026-03-16 20:27:08
- CSV-Modelle: Customer, Tariff, SwitchRequest
- CSV-Parsing mit Semikolon-Delimiter

### 4. `ed6c994` — Implement business logic, SLA computation, idempotent processing
- **Datum:** 2026-03-16 20:29:32
- Geschäftslogik für Tarifwechsel
- SLA-Berechnung
- Idempotente Verarbeitung
- Orchestrierung

### 5. `7c461a7` — Add unit tests
- **Datum:** 2026-03-16 20:30:49
- Unit Tests für SLA-Calculator
- Tests für Tariff-Switch-Processor
- Tests für Processing Store

### 6. `5592b70` — Final cleanup
- **Datum:** 2026-03-16 20:31:36
- Runtime-Output aus Git ausgeschlossen
- Roadmap aktualisiert

### 7. `dd4b735` — Add timestamped console logs
- **Datum:** 2026-03-16 20:44:53
- Zeitgestempelte Konsolenausgaben
- .gitignore aktualisiert

### 8. `d1b6394` — Add CSV output report
- **Datum:** 2026-03-16 20:48:28
- CSV-Ausgabereport hinzugefügt
- Report aus Git ausgeschlossen

### 9. `c141432` — Add metrik.md
- **Datum:** 2026-03-16 21:13:37
- Entwicklungsmetriken dokumentiert

### 10. `59a726d` — Remove misleading duration from metrik.md
- **Datum:** 2026-03-16 21:20:46
- Irreführende Zeitangaben entfernt

### 11. `638ca3d` — Add chat workflow metrics
- **Datum:** 2026-03-16 21:23:00
- Chat-Workflow-Metriken zu metrik.md hinzugefügt

### 12. `22d2d2d` — Standardize input/output paths
- **Datum:** 2026-03-16 23:33:37
- Shared Input Files im Root-Verzeichnis
- Output standardisiert auf Output/decisions.json pro Projekt
