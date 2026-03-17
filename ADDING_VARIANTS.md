# Adding New Variants

This document describes the conventions for adding a new implementation variant to the benchmark.

---

## Naming Convention

Folder name pattern: `CustomerTariffSwitch-<Tag>`

| Tag | Bedeutung |
|-----|-----------|
| `Claude` | Nur Instructions.md (kein Testvorwissen) |
| `Claude-Tests` | Instructions.md + Testsuite aus P1 vorab gegeben |
| `Claude-TestsAndAgent` | Instructions.md + Tests + AgentWorkflow.md |
| `Claude-Reviewed` | Instructions.md + vorheriger Output wurde reviewed/korrigiert |
| `GPT-...` | Andere Modelle analog benennen |

Wähle einen Tag, der klar beschreibt was dem Agent vorab gegeben wurde.

> **Wichtig:** Der Ordnername darf NICHT `.Test` oder `.Tests` enthalten
> (z.B. ~~`CustomerTariffSwitch-Claude.Tests`~~), da das Metriken-Skript
> Testprojekte anhand von `.Test` / `.Tests` im csproj-Pfad erkennt.

---

## Pflichtstruktur

```
CustomerTariffSwitch-<Tag>/
├── Instructions.md          # Der genaue Prompt / die Instructions die dem Agent gegeben wurden
├── <SolutionName>.sln(x)   # .NET Solution
├── <MainProject>/           # Hauptprojekt (kein "Test" im csproj-Namen, hat Program.cs)
│   └── ...
└── <TestProject>/           # Testprojekt (csproj-Name enthält "Test" oder "Tests")
    └── ...                  # Optional – wenn kein Test gegeben/generiert
```

**Pflichtfelder:**
- `Instructions.md` — der exakte Prompt (damit reproduzierbar)
- Eine lauffähige .NET Solution (baut ohne Fehler: `dotnet build`)
- Erzeugt `Output/decisions.json` bei `dotnet run` mit den Standard-Input-Files

**Optional:**
- `AgentWorkflow.md` — System-Prompt / Multi-Agent-Workflow falls verwendet
- `metrik.md`, `roadmap.md` — vom Agent generierte Artefakte (nicht löschen, Teil des Benchmarks)
- `Git-History.md` — falls Commit-History dokumentiert wurde

---

## Input Files

Alle Varianten lesen vom gemeinsamen Ordner `../Input Files/`:
- `customers.csv`
- `tariffs.csv`
- `requests.csv`

**Niemals** eigene Input-Kopien anlegen — Vergleichbarkeit geht verloren.

---

## Instructions.md erstellen

Die `Instructions.md` der bisherigen Varianten sind nahezu identisch.
Es gibt zwei optionale Zeilen, die je nach Variante eingefügt werden:

| Zeile | Wann einfügen | Inhalt |
|-------|---------------|--------|
| 9 | Wenn Testsuite vorab mitgegeben wird | `Unit tests and integration tests are also provided in the project root.` |
| 13 | Wenn der Agent die Ergebnisse explizit ausgeben soll | `Output the final results into a file to compare later on` |

**Varianten-Übersicht:**

| Variante | Zeile 9 (Tests) | Zeile 13 (Output) |
|----------|:---:|:---:|
| P2 — Claude | nein | nein |
| P3 — Claude-Tests | ja | nein |
| P4 — Claude-TestsAndAgent | ja | ja |

**Vorgehen:**
1. Kopiere `Input Files/Instructions-Template.md` als Ausgangsbasis
2. Entkommentiere die relevanten optionalen Zeilen (siehe HTML-Kommentare im Template)
3. Entferne die `<!-- OPTIONAL: ... -->` Kommentar-Zeilen
4. Speichere als `Instructions.md` im Varianten-Ordner

---

## Vergleich aktualisieren

Nach dem Hinzufügen einer neuen Variante:

1. **Objektive Metriken** automatisch generieren:
   ```powershell
   .\scripts\collect-metrics.ps1
   ```
   Erzeugt `Vergleich-Metrics.md` mit allen Tabellen aus Abschnitt 1.

2. **Subjektive Analyse** manuell in `Vergleich.md` ergänzen:
   - Code-Review (Abschnitt 2)
   - Stärken/Schwächen (Abschnitt 3)
   - Gewichteten Score berechnen und einfügen

3. `Vergleich.html` aus `Vergleich.md` regenerieren (z.B. mit Pandoc oder VS Code Extension).

---

## Checkliste vor dem Commit

- [ ] `dotnet build` ohne Fehler und 0 Warnings
- [ ] `dotnet test` — Ergebnis notiert (wie viele Tests, wie viele bestehen)
- [ ] `dotnet run` erzeugt `Output/decisions.json`
- [ ] `Instructions.md` enthält den exakten Agent-Prompt
- [ ] `scripts/collect-metrics.ps1` ausgeführt, Ausgabe geprüft
- [ ] `Vergleich.md` manuell ergänzt (subjektive Analyse + Score)
