# Vergleich der 4 CustomerTariffSwitch-Implementierungen

> **Analysiert am:** 16.03.2026  
> **Analysemethode:** Automatisierte Metriken + manuelle Code-Review  
> **Gemeinsame Input-Daten:** 3 CSV-Dateien (customers.csv, requests.csv, tariffs.csv)

## Übersicht der Projekte

| ID | Projekt | Beschreibung | .NET Version |
|----|---------|-------------|-------------|
| **P1** | CustomerTariffSwitch | Manuelle Implementierung (Mensch) | net10.0 |
| **P2** | CustomerTariffSwitch-Claude | Claude, nur Instructions.md | net10.0 |
| **P3** | CustomerTariffSwitch-Claude-Tests | Claude, Instructions.md + Tests aus P1 | net8.0 |
| **P4** | CustomerTariffSwitch-Claude-TestsAndAgent | Claude, Instructions.md + Multi-Agent-Workflow | net8.0 |

---

## 1. Objektive Metriken

### 1.1 Projektstruktur

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Assemblies/csproj** | 4 | 2 | 2 | 2 |
| **Source-Dateien** | 10 | 9 | 9 | 11 |
| **Test-Dateien** | 5 | 1 | 6 | 6 |
| **Solution-Typ** | .sln | .slnx | .slnx | .slnx |

**Assembly-Aufschlüsselung P1:** CustomerTariffSwitch, CustomerTariffSwitch.Data, CustomerTariffSwitch.Models, CustomerTariffSwitch.Test  
**Assembly-Aufschlüsselung P2-P4:** CustomerTariffSwitch, CustomerTariffSwitch.Test(s)

### 1.2 Lines of Code (LOC)

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Source: Gesamtzeilen** | 586 | 485 | 528 | 515 |
| **Source: Code-Zeilen** | 433 | 393 | 430 | 409 |
| **Source: Kommentare** | 39 | 28 | 10 | 19 |
| **Source: Leerzeilen** | 114 | 64 | 88 | 87 |
| **Test: Gesamtzeilen** | 802 | 199 | 803 | 803 |
| **Test: Code-Zeilen** | 633 | 165 | 634 | 634 |

### 1.3 Strukturelle Komplexität

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Klassen** | 8 | 7 | 7 | 7 |
| **Enums** | 3 | 1 | 3 | 3 |
| **Records** | 0 | 4 | 0 | 0 |
| **Methoden (src)** | 17 | 13 | 17 | 16 |

### 1.4 Tests

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Testanzahl** | 43 | 15 | 43 | 43 |
| **Bestanden** | 43 | 15 | 43 | 43 |
| **Fehlgeschlagen** | 0 | 0 | 0 | 0 |
| **Pass Rate** | 100% | 100% | 100% | 100% |
| **Test-Framework** | xUnit | xUnit | xUnit | xUnit |
| **Test-Dateien** | 5 | 1 | 6 | 6 |
| **Test-LOC** | 633 | 165 | 634 | 634 |

### 1.5 Laufzeit (Cold Run, `--no-build`)

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Run 1 (Cold)** | 1712 ms | 1981 ms | 2020 ms | 1985 ms |
| **Run 2 (Warm/Idempotent)** | 1781 ms | 1736 ms | 1948 ms | 1931 ms |

> **Hinweis:** Bei dieser Datenmenge (5 Customers, 4 Tariffs, ~10 Requests) sind die Laufzeitunterschiede primär durch .NET-Startup und File-I/O bedingt, nicht durch algorithmische Effizienz. P1/P2 laufen auf net10.0, P3/P4 auf net8.0.

### 1.6 Build-Qualität

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Compiler Warnings** | 0 | 0 | 0 | 0 |
| **Compiler Errors** | 0 | 0 | 0 | 0 |
| **Nullable Reference Types** | Ja | Ja | Ja | Ja |

### 1.7 Output-Formate

| Projekt | decisions.json | CSV Report | Text Report |
|---------|:-:|:-:|:-:|
| **P1** | ✅ | — | — |
| **P2** | ✅ | ✅ (report.csv) | — |
| **P3** | ✅ | — | ✅ (report.txt) |
| **P4** | ✅ | ✅ (decisions.csv) | — |

---

## 2. Subjektive Analyse (Code Review)

### 2.1 Architektur & Projektstruktur

| Aspekt | P1 | P2 | P3 | P4 | Bewertung |
|--------|:--:|:--:|:--:|:--:|-----------|
| **Schichtentrennung** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ | P1 trennt Models, Data und Services in eigene Assemblies (3-Schicht). P2-P4 packen alles in ein einziges Projekt mit Ordnern. |
| **Single Responsibility** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P1 hat klare Trennung (CsvService, DecisionRepository, ProcessRequestService, SolutionPathHelper). P2 hat eine eigene SlaCalculator-Klasse — sauberste SRP. P3 vermischt FindProjectRoot()-Logik im DecisionRepository und dupliziert sie im Program.cs. P4 ähnlich wie P1 mit Data/Services-Trennung. |
| **Abstraktionsebene** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ | P1 nutzt SolutionPathHelper als wiederverwendbare Utility. P2-P4 haben Path-Discovery inline oder als private Methoden. |

### 2.2 Coding Style & Lesbarkeit

| Aspekt | P1 | P2 | P3 | P4 | Bewertung |
|--------|:--:|:--:|:--:|:--:|-----------|
| **Konsistenz** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P1 durchgängig konsistenter Stil. P2 nutzt konsequent `sealed`, `record`, moderne C#-Features. P3 mischt Stile (z.B. `List<T>` vs `IReadOnlyCollection<T>` als Parameter). P4 durchgehend sauber. |
| **Kommentierung** | ⭐⭐⭐ | ⭐⭐ | ⭐ | ⭐⭐ | P1 hat die meisten und hilfreichsten Kommentare (39 Zeilen), inkl. Scenario-Referenzen und DST-Erklärungen. P2 nutzt XML-Docs. P3 hat nur 10 Kommentarzeilen — der wenigste Kontext. P4 nutzt Scenario-Kommentare ähnlich P1. |
| **Naming** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | Alle Projekte verwenden klare, sprechende Bezeichner. P2 sticht positiv heraus mit `IsPremium` / `HasSmartMeter` als berechnete Properties am Model. |
| **Moderne C#-Features** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | P1 nutzt Collection Expressions `[.. x]`, Tuple-Returns, `required`. P2 nutzt `record`, `sealed`, Top-Level Statements mit `static local functions`. P3/P4 auf net8.0 — etwas ältere Syntax, aber solide. |
| **Console Output** | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | P1 gibt eine kompakte, strukturierte Zusammenfassung. P2 liefert das detaillierteste Logging mit Timestamps. P3 gibt nummerierte Schritte [1/4] bis [4/4] mit Timing. P4 ist am knappsten. |

### 2.3 Robustheit & Fehlerbehandlung

| Aspekt | P1 | P2 | P3 | P4 | Bewertung |
|--------|:--:|:--:|:--:|:--:|-----------|
| **Input-Validierung** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | Alle validieren fehlende/leere Felder, ungültige Timestamps. P1 nutzt `try/catch(FormatException)` beim CSV-Parsing. P2 validiert einzelne Felder explizit im Processor. P3/P4 verwenden `TryParse` — defensiver. |
| **Idempotenz** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | Alle implementieren Idempotenz (bereits verarbeitete Request-IDs werden übersprungen). |
| **Atomic Writes** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P1, P3, P4 nutzen tmp-File + Move-Swap. P2 schreibt direkt mit `File.WriteAllText` — kein Crash-Schutz. |
| **DST-Handling** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P1 behandelt Invalid/Ambiguous Times korrekt mit minutengenauer Auflösung. P2 nutzt `GetUtcOffset()` ohne explizite Invalid/Ambiguous-Prüfung — potenziell fehlerhaft bei Zeitumstellung. P3/P4 behandeln beides explizit. |
| **Paralleles CSV-Lesen** | ⭐⭐⭐ | ⭐ | ⭐ | ⭐ | Nur P1 nutzt `Parallel.ForEach` zum parallelen Einlesen der CSV-Dateien + `FileShare.ReadWrite` für Thread-Safety. |

### 2.4 Wartbarkeit & Erweiterbarkeit

| Aspekt | P1 | P2 | P3 | P4 | Bewertung |
|--------|:--:|:--:|:--:|:--:|-----------|
| **Testbarkeit** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P1 hat `overridePath`-Parameter für testbare Dependency Injection im DecisionRepository. P2 injiziert Daten per Konstruktor. P3 hat `overridePath` aber auch statische Helper-Methoden — schwieriger zu mocken. P4 ähnlich gut wie P1. |
| **Modularität** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | P1 mit 4 Assemblies am modularsten. P2 hat klare Trennung in Parsing/Services/Models trotz Single-Assembly. P3/P4 sind funktional, aber stärker gekoppelt. |
| **Erweiterbarkeit** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ | P1's Schichtentrennung erlaubt einfacheres Hinzufügen neuer Data-Sources oder Output-Formate. P2's Single-Responsibility-Design macht das Hinzufügen neuer Business-Rules einfach. |

### 2.5 Design-Entscheidungen im Vergleich

| Design-Aspekt | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|---------------|-------------|-------------|--------------------|--------------------|
| **Model-Typ** | `class` mit `required init` + Factory Methods | `record` mit berechneten Properties (`IsPremium`, `HasSmartMeter`) | `class` mit Enums | `class` mit Enums |
| **Enum-Nutzung** | `SLALevel`, `MeterType`, `DecisionStatus` (3 Enums) | Strings als Enum-Ersatz (nur `RequestStatus` als Enum) | Wie P1 (3 Enums) | Wie P1 (3 Enums, teils einzelne Dateien) |
| **SLA-Berechnung** | Inline in ProcessRequestService | Eigene statische `SlaCalculator`-Klasse | Inline in ProcessRequestService | Inline in ProcessRequestService |
| **CSV-Parsing** | Eigene CsvService-Klasse mit `ConcurrentDictionary` | Eigene statische `CsvParser`-Klasse | CsvService wie P1 | CsvService mit `Data`-Namespace |
| **Path Discovery** | `SolutionPathHelper` (shared utility) | `FindRepoRoot()` als lokale Funktion | `FindInputDirectory()` + `FindProjectRoot()` (dupliziert) | `FindInputFilesDirectory()` + `FindProjectRoot()` (dupliziert) |
| **Program.cs Stil** | Minimalistisch, ~55 Zeilen | Verbose mit Timestamps, ~120 Zeilen | Verbose mit Schritt-Nummern, ~130 Zeilen | Kompakt, ~75 Zeilen |

---

## 3. Gesamtbewertung

### Scoring (gewichtet)

| Kategorie (Gewicht) | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) |
|---------------------|:-----------:|:-----------:|:------------------:|:------------------:|
| **Korrektheit** (30%) | 10/10 | 8/10 | 10/10 | 10/10 |
| **Code-Qualität** (20%) | 9/10 | 8/10 | 7/10 | 8/10 |
| **Testabdeckung** (20%) | 9/10 | 5/10 | 9/10 | 9/10 |
| **Architektur** (15%) | 9/10 | 8/10 | 7/10 | 7/10 |
| **Robustheit** (15%) | 9/10 | 7/10 | 8/10 | 8/10 |
| **Gewichteter Score** | **9.3** | **7.2** | **8.4** | **8.5** |

### Stärken & Schwächen

#### P1 — Mensch (Manuelle Implementierung)
- **Stärken:** Beste Architektur (4 Assemblies), vollständigstes DST-Handling, paralleles CSV-Lesen, hilfreichste Kommentare, atomic writes
- **Schwächen:** Höchster LOC bei Source (433) — etwas mehr Boilerplate durch Multi-Assembly-Setup

#### P2 — Claude (nur Instructions)
- **Stärken:** Kompaktester Source-Code (393 LOC), moderne `record`-Nutzung, eigene SlaCalculator-Klasse (SRP), detailliertestes Console-Logging
- **Schwächen:** Nur 15 Tests (vs. 43 bei den anderen), DST nicht vollständig behandelt (fehlende Invalid/Ambiguous-Time-Prüfung), kein atomic write, Strings statt Enums für SLA/MeterType

#### P3 — Claude + Tests (Instructions + vorgegebene Tests)
- **Stärken:** Besteht alle 43 Tests, solides DST-Handling, `TryParse`-basierte Validierung
- **Schwächen:** Wenigste Kommentare (10 Zeilen), Path-Discovery-Logik dupliziert, inkonsistente Parameter-Typen (`List` vs `HashSet`)

#### P4 — Claude + Multi-Agent
- **Stärken:** Besteht alle 43 Tests, saubere `IReadOnlyCollection`-Parameter, Scenario-kommentiert, klare Data/Services-Trennung
- **Schwächen:** Einzelne Enums in separaten Dateien (unnötige Dateimenge), Path-Discovery dupliziert

---

## 4. Fazit

Die **manuelle Implementierung (P1)** liefert die beste Gesamtqualität — vor allem bei Architektur, DST-Korrektheit und Robustheit. Die Investition in Multi-Assembly-Trennung zahlt sich bei Wartbarkeit aus.

**P2 (Claude ohne Tests)** zeigt, dass KI kompakten, lesbaren Code generieren kann, aber ohne vorgegebene Tests entsteht ein signifikantes Testdefizit (15 vs. 43 Tests) und subtile Korrektheitslücken (DST). Die `record`-Nutzung ist hingegen ein guter moderner Ansatz.

**P3 und P4** sind nahezu identisch in der Qualität. Die vorgegebenen Tests waren der entscheidende Faktor — sie zwingen die KI, die gleiche Korrektheit wie die manuelle Implementierung zu erreichen. Der Multi-Agent-Workflow (P4) liefert marginal besseren Code-Stil als P3.

**Kernaussage:** Tests sind der wichtigste Qualitätstreiber bei KI-generiertem Code. Ohne Tests (P2) sinkt die Qualität merklich. Mit Tests (P3/P4) erreicht KI-generierter Code nahezu das Niveau manueller Implementierung — bei gleichzeitig weniger Architectural Investment.
