# Vergleich der 5 CustomerTariffSwitch-Implementierungen

> **Analysiert am:** 07.04.2026  
> **Analysemethode:** Automatisierte Metriken + manuelle Code-Review  
> **Gemeinsame Input-Daten:** 3 CSV-Dateien (customers.csv, requests.csv, tariffs.csv)

## Übersicht der Projekte

| ID | Projekt | Beschreibung | .NET Version |
|----|---------|-------------|-------------|
| **P1** | CustomerTariffSwitch | Manuelle Implementierung (Mensch) | net10.0 |
| **P2** | CustomerTariffSwitch-Claude | Claude, nur Instructions.md | net10.0 |
| **P3** | CustomerTariffSwitch-Claude-Tests | Claude, Instructions.md + Tests aus P1 | net8.0 |
| **P4** | CustomerTariffSwitch-Claude-TestsAndAgent | Claude, Instructions.md + Multi-Agent-Workflow | net8.0 |
| **P5** | CustomerTariffSwitch-DetailedInstructions | Claude, detaillierte Instructions mit vorgegebener Architektur + DI + Agent-Workflow | net10.0 |

---

## 1. Objektive Metriken

### 1.1 Projektstruktur

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Assemblies/csproj** | 4 | 2 | 2 | 2 | 4 |
| **Source-Dateien** | 10 | 9 | 9 | 11 | 17 |
| **Test-Dateien** | 5 | 1 | 6 | 6 | 5 |
| **Solution-Typ** | .sln | .slnx | .slnx | .slnx | .slnx |

**Assembly-Aufschlüsselung P1:** CustomerTariffSwitch, CustomerTariffSwitch.Data, CustomerTariffSwitch.Models, CustomerTariffSwitch.Test  
**Assembly-Aufschlüsselung P2-P4:** CustomerTariffSwitch, CustomerTariffSwitch.Test(s)  
**Assembly-Aufschlüsselung P5:** CustomerTariffSwitch, CustomerTariffSwitch.Data, CustomerTariffSwitch.Models, CustomerTariffSwitch.Tests (gleiche Struktur wie P1)

### 1.2 Lines of Code (LOC)

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Source: Gesamtzeilen** | 586 | 485 | 528 | 515 | 897 |
| **Source: Code-Zeilen** | 433 | 393 | 430 | 409 | 733 |
| **Source: Kommentare** | 39 | 28 | 10 | 19 | 0 |
| **Source: Leerzeilen** | 114 | 64 | 88 | 87 | 164 |
| **Test: Gesamtzeilen** | 802 | 199 | 803 | 803 | 493 |
| **Test: Code-Zeilen** | 633 | 165 | 634 | 634 | 393 |

### 1.3 Strukturelle Komplexität

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Klassen** | 8 | 7 | 7 | 7 | 7 |
| **Enums** | 3 | 1 | 3 | 3 | 4 |
| **Records** | 0 | 4 | 0 | 0 | 5 |
| **Methoden (src)** | 17 | 13 | 17 | 16 | 43 |

### 1.4 Tests

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Testanzahl** | 43 | 15 | 43 | 43 | 37 |
| **Bestanden** | 43 | 15 | 43 | 43 | 37 |
| **Fehlgeschlagen** | 0 | 0 | 0 | 0 | 0 |
| **Pass Rate** | 100% | 100% | 100% | 100% | 100% |
| **Test-Framework** | xUnit | xUnit | xUnit | xUnit | xUnit |
| **Test-Dateien** | 5 | 1 | 6 | 6 | 5 |
| **Test-LOC** | 633 | 165 | 634 | 634 | 393 |

### 1.5 Laufzeit (Cold Run, `--no-build`)

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Run 1 (Cold)** | 1712 ms | 1981 ms | 2020 ms | 1985 ms | 1807 ms |
| **Run 2 (Warm/Idempotent)** | 1781 ms | 1736 ms | 1948 ms | 1931 ms | 1696 ms |

> **Hinweis:** Bei dieser Datenmenge (5 Customers, 4 Tariffs, ~10 Requests) sind die Laufzeitunterschiede primär durch .NET-Startup und File-I/O bedingt, nicht durch algorithmische Effizienz. P1/P2/P5 laufen auf net10.0, P3/P4 auf net8.0.

### 1.6 Build-Qualität

| Metrik | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|--------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Compiler Warnings** | 0 | 0 | 0 | 0 | 0 |
| **Compiler Errors** | 0 | 0 | 0 | 0 | 0 |
| **Nullable Reference Types** | Ja | Ja | Ja | Ja | Ja |

### 1.7 Output-Formate

| Projekt | decisions.json | CSV Report | Text Report |
|---------|:-:|:-:|:-:|
| **P1** | ✅ | — | — |
| **P2** | ✅ | ✅ (report.csv) | — |
| **P3** | ✅ | — | ✅ (report.txt) |
| **P4** | ✅ | ✅ (decisions.csv) | — |
| **P5** | ✅ | — | — |

---

## 2. Subjektive Analyse (Code Review)

### 2.1 Architektur & Projektstruktur

| Aspekt | P1 | P2 | P3 | P4 | P5 | Bewertung |
|--------|:--:|:--:|:--:|:--:|:--:|-----------|
| **Schichtentrennung** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P1 und P5 trennen Models, Data und Services in eigene Assemblies (3-Schicht). P2-P4 packen alles in ein einziges Projekt mit Ordnern. P5 folgt exakt der in den Instructions vorgegebenen Assembly-Struktur. |
| **Single Responsibility** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 trennt sauber: CsvReaderService, DecisionRepository, SlaCalculationService, RequestValidationService, TariffSwitchProcessor. Hat zusätzlich CsvParsingHelper und SolutionPathHelper als shared Utilities. |
| **Abstraktionsebene** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P1 und P5 nutzen SolutionPathHelper als wiederverwendbare Utility. P5 hat zusätzlich DI via Microsoft.Extensions.DependencyInjection mit Constructor Injection. |

### 2.2 Coding Style & Lesbarkeit

| Aspekt | P1 | P2 | P3 | P4 | P5 | Bewertung |
|--------|:--:|:--:|:--:|:--:|:--:|-----------|
| **Konsistenz** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 nutzt konsequent `record`, `init`, `IReadOnlyList<T>`, switch-expressions und collection expressions. Durchgehend konsistenter moderner Stil. |
| **Kommentierung** | ⭐⭐⭐ | ⭐⭐ | ⭐ | ⭐⭐ | ⭐ | P5 hat 0 Kommentarzeilen — noch weniger als P3 (10). Der Code ist zwar selbsterklärend durch gutes Naming, aber Scenario-Referenzen und DST-Erklärungen fehlen komplett. |
| **Naming** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 verwendet durchgehend klare Bezeichner. Service- und Methodennamen entsprechen exakt der Aufgabenstellung. |
| **Moderne C#-Features** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P5 auf net10.0 nutzt `record`, `required init`, switch-expressions, collection expressions `[]`, `sealed`. Gleiches Niveau wie P1/P2. |
| **Console Output** | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P5 gibt strukturierte Ausgabe mit Lade-Zusammenfassung, Zeilen-Status ([APPROVED]/[REJECTED]/[SKIPPED]), und Gesamt-Summary. Nah am spezifizierten Format. |

### 2.3 Robustheit & Fehlerbehandlung

| Aspekt | P1 | P2 | P3 | P4 | P5 | Bewertung |
|--------|:--:|:--:|:--:|:--:|:--:|-----------|
| **Input-Validierung** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 validiert Dateien, Header, Enums und Timestamps. Nutzt `EnsureFileExists` + `GetRequiredHeaderIndex` für Fail-Fast. Leere CustomerId in Requests wird nicht explizit geprüft, aber korrekt als "Unknown customer" abgelehnt. |
| **Idempotenz** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | Alle implementieren Idempotenz korrekt. P5 aktualisiert das In-Memory-Set sofort nach Verarbeitung — verhindert auch Duplikate innerhalb eines Runs. |
| **Atomic Writes** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 nutzt temp-file + `File.Move` mit Overwrite — crash-sicher wie P1/P3/P4. |
| **DST-Handling** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 behandelt `IsInvalidTime` (spring-forward) und `IsAmbiguousTime` (fall-back) explizit. Zeitzonen-Fallback Windows/Linux vorhanden. |
| **Paralleles CSV-Lesen** | ⭐⭐⭐ | ⭐ | ⭐ | ⭐ | ⭐ | Nur P1 nutzt `Parallel.ForEach` zum parallelen Einlesen. |

### 2.4 Wartbarkeit & Erweiterbarkeit

| Aspekt | P1 | P2 | P3 | P4 | P5 | Bewertung |
|--------|:--:|:--:|:--:|:--:|:--:|-----------|
| **Testbarkeit** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | P5 nutzt echte DI (Microsoft.Extensions.DependencyInjection) mit Constructor Injection. Services können über Path-Override-Parameter in Tests isoliert werden. |
| **Modularität** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P5 mit 4 Assemblies gleich modular wie P1. Zusätzlich mit formalem DI-Container — modularste Lösung insgesamt. |
| **Erweiterbarkeit** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐⭐ | P5's Kombination aus Assembly-Trennung und DI ermöglicht einfaches Austauschen/Erweitern von Services. RejectionReason-Enum (4 Werte) ist definiert, wird aber intern per String-Konstanten genutzt — minimaler Drift-Risiko. |

### 2.5 Design-Entscheidungen im Vergleich

| Design-Aspekt | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|---------------|-------------|-------------|--------------------|--------------------|---------------------|
| **Model-Typ** | `class` mit `required init` + Factory Methods | `record` mit berechneten Properties (`IsPremium`, `HasSmartMeter`) | `class` mit Enums | `class` mit Enums | `record` mit `required init` + immutable Design |
| **Enum-Nutzung** | `SLALevel`, `MeterType`, `DecisionStatus` (3 Enums) | Strings als Enum-Ersatz (nur `RequestStatus` als Enum) | Wie P1 (3 Enums) | Wie P1 (3 Enums, teils einzelne Dateien) | 4 Enums in Einzeldateien (`SlaLevel`, `MeterType`, `DecisionStatus`, `RejectionReason`) |
| **SLA-Berechnung** | Inline in ProcessRequestService | Eigene statische `SlaCalculator`-Klasse | Inline in ProcessRequestService | Inline in ProcessRequestService | Eigene `SlaCalculationService`-Klasse (DI-injiziert) |
| **CSV-Parsing** | Eigene CsvService-Klasse mit `ConcurrentDictionary` | Eigene statische `CsvParser`-Klasse | CsvService wie P1 | CsvService mit `Data`-Namespace | `CsvReaderService` + `CsvParsingHelper` (Data-Assembly) |
| **Path Discovery** | `SolutionPathHelper` (shared utility) | `FindRepoRoot()` als lokale Funktion | `FindInputDirectory()` + `FindProjectRoot()` (dupliziert) | `FindInputFilesDirectory()` + `FindProjectRoot()` (dupliziert) | `SolutionPathHelper` (shared utility, wie P1) |
| **Program.cs Stil** | Minimalistisch, ~55 Zeilen | Verbose mit Timestamps, ~120 Zeilen | Verbose mit Schritt-Nummern, ~130 Zeilen | Kompakt, ~75 Zeilen | Minimalistisch mit DI-Setup, ~32 Zeilen |
| **Dependency Injection** | Manuell | Manuell | Manuell | Manuell | `Microsoft.Extensions.DependencyInjection` mit Constructor Injection |


---

## 3. Gesamtbewertung

### Scoring (gewichtet)

| Kategorie (Gewicht) | P1 (Mensch) | P2 (Claude) | P3 (Claude+Tests) | P4 (Claude+Agent) | P5 (DetailedInstr.) |
|---------------------|:-----------:|:-----------:|:------------------:|:------------------:|:-------------------:|
| **Korrektheit** (30%) | 10/10 | 8/10 | 10/10 | 10/10 | 9/10 |
| **Code-Qualität** (20%) | 9/10 | 8/10 | 7/10 | 8/10 | 9/10 |
| **Testabdeckung** (20%) | 9/10 | 5/10 | 9/10 | 9/10 | 8/10 |
| **Architektur** (15%) | 9/10 | 8/10 | 7/10 | 7/10 | 10/10 |
| **Robustheit** (15%) | 9/10 | 7/10 | 8/10 | 8/10 | 9/10 |
| **Gewichteter Score** | **9.3** | **7.2** | **8.4** | **8.5** | **9.0** |

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

#### P5 — Claude + Detaillierte Instructions
- **Stärken:** Beste Architektur aller KI-Varianten (4 Assemblies wie P1), echte DI mit Microsoft.Extensions.DependencyInjection, konsequent immutable Records, moderne C#-Features (switch-expressions, collection expressions), eigene SlaCalculationService-Klasse, vollständiges DST-Handling, atomic writes, kompaktestes Program.cs (~32 Zeilen)
- **Schwächen:** 0 Kommentarzeilen, RejectionReason-Enum definiert aber intern per String-Konstanten genutzt (Drift-Risiko), 37 statt 43 Tests (eigenständig generiert — weniger als P1 aber deutlich mehr als P2), höchster Source-LOC (733 Code-Zeilen — fast doppelt so viel wie P1)

---

## 4. Fazit

Die **manuelle Implementierung (P1)** liefert die beste Gesamtqualität — vor allem bei Architektur, DST-Korrektheit und Robustheit. Die Investition in Multi-Assembly-Trennung zahlt sich bei Wartbarkeit aus.

**P5 (Claude mit detaillierten Instructions)** erreicht mit 9.0 den zweithöchsten Score und ist die beste KI-generierte Variante. Die vorgegebene Architektur (4 Assemblies, DI, Records) wurde präzise umgesetzt. Der Einsatz von DI und immutable Records geht über P1 hinaus. Schwächen: fehlende Kommentare, ein ungenutztes RejectionReason-Enum und der höchste Source-LOC aller Varianten (733 vs. 433 bei P1).

**P2 (Claude ohne Tests)** zeigt, dass KI kompakten, lesbaren Code generieren kann, aber ohne vorgegebene Tests entsteht ein signifikantes Testdefizit (15 vs. 43 Tests) und subtile Korrektheitslücken (DST). Die `record`-Nutzung ist hingegen ein guter moderner Ansatz.

**P3 und P4** sind nahezu identisch in der Qualität. Die vorgegebenen Tests waren der entscheidende Faktor — sie zwingen die KI, die gleiche Korrektheit wie die manuelle Implementierung zu erreichen. Der Multi-Agent-Workflow (P4) liefert marginal besseren Code-Stil als P3.

**Kernaussage:** Detaillierte architektonische Vorgaben in den Instructions (P5) sind der zweitwichtigste Qualitätstreiber nach Tests. P5 erreicht die beste Architektur aller KI-Varianten und fast das Niveau der manuellen Implementierung — obwohl keine Tests vorgegeben wurden. Die Kombination aus detaillierten Instructions + vorgegebenen Tests wäre vermutlich die optimale Strategie.
