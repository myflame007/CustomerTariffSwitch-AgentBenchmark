# Metriken — CustomerTariffSwitch

## Chat-Interaktion (GitHub Copilot — Claude Opus 4.6)

| Metrik | Wert |
|--------|------|
| Modell | Claude Opus 4.6 |
| User-Nachrichten | 6 |
| Arbeitsschritte (Agent) | Analyse → Roadmap → Struktur → Models → Services → Program → Tests → Logging → Report → Metriken |
| Tool-Aufrufe (ca.) | ~50 (read_file, create_file, replace, terminal, etc.) |
| Terminal-Befehle | ~15 (dotnet build, dotnet test, dotnet run, git, etc.) |
| Erstellte Dateien | 10 (8 Source + .gitignore + roadmap.md) |
| Bearbeitete Dateien | 4 (Program.cs, .gitignore, roadmap.md, metrik.md) |
| Build-Fehler behoben | 1 (fehlende GlobalUsings.cs für xUnit) |

## Codebase

| Kategorie | Dateien | Lines of Code |
|-----------|---------|---------------|
| Models | 4 | 67 |
| Services | 3 | 229 |
| Program.cs | 1 | 120 |
| **Source gesamt** | **8** | **416** |
| Unit-Tests | 4 | 600 |
| Integration-Tests | 1 | 66 |
| **Tests gesamt** | **5** | **666** |
| **Projekt gesamt** | **13** | **1.082** |

## Tests

| Metrik | Wert |
|--------|------|
| Unit-Tests | 36 |
| Integration-Tests | 7 |
| **Tests gesamt** | **43** |
| Bestanden | 43 |
| Fehlgeschlagen | 0 |
| Testabdeckung (Szenarien) | 8/8 (alle Business-Szenarien) |

## Git

| Metrik | Wert |
|--------|------|
| Commits | 5 (inkl. init) |
| Letzter Commit | `d855e5b` |

### Commit-Historie

| Hash | Beschreibung |
|------|-------------|
| `384b999` | init |
| `6373467` | Implement CustomerTariffSwitch solution |
| `b8c88b7` | Update roadmap with completion status |
| `74e32ae` | Add console logging, .gitignore, remove build artifacts |
| `d855e5b` | Add output-report.txt generation |

## Laufzeit (Applikation)

| Metrik | Wert |
|--------|------|
| Verarbeitete Requests | 9 |
| Davon genehmigt | 4 |
| Davon abgelehnt | 5 |
| Ausführungszeit | ~150–190 ms |

## Technologie-Stack

- **.NET 8.0** — Console App
- **xUnit 2.6** — Testing
- **System.Text.Json** — Persistenz (decisions.json)
- **Europe/Vienna TimeZone** — DST-sichere SLA-Berechnung
