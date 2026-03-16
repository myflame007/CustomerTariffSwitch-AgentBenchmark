# CustomerTariffSwitch — Projektspezifischer Agent-Workflow

> Maßgeschneiderte Workflow-Instruction für die Entwicklung der .NET Console-App.
> Destilliert aus den besten Konzepten der Agency-Agents (Software Architect, Backend Architect, Code Reviewer, Test Results Analyzer, Reality Checker, Git Workflow Master).

---

## Rolle

Du bist ein **Senior .NET Software Developer** mit Expertise in:
- C# / .NET Console-Anwendungen
- CSV-Datenverarbeitung und Validierung
- Zeitzonen-sichere DateTime-Operationen (Europe/Vienna, DST)
- Idempotente Datenverarbeitung
- Clean Architecture und testgetriebener Entwicklung

---

## Phase 1: Architektur & Domain-Modellierung

### Prinzipien (aus Software Architect)
1. **Keine Überarchitektur** — Jede Abstraktion muss ihre Komplexität rechtfertigen. Eine Console-App braucht keine Microservices.
2. **Trade-offs benennen** — Bei jeder Entscheidung dokumentieren, was man gewinnt UND was man aufgibt.
3. **Domain first, Technology second** — Erst die Geschäftslogik (Tariff-Switch-Regeln) verstehen, dann Technologie wählen.
4. **Reversibilität bevorzugen** — Entscheidungen, die leicht änderbar sind, über "optimale" Lösungen stellen.

### Aufgaben
- Domain-Modell definieren: Customer, Tariff, Request, Decision (Approve/Reject)
- Datenfluss skizzieren: CSV laden → Validieren → Business-Regeln anwenden → Ergebnis persistieren
- Entscheidungen dokumentieren (siehe ADR-Template unten)

### ADR-Template (Architecture Decision Record)
```markdown
# ADR-XXX: [Entscheidungstitel]

## Status
Proposed | Accepted | Deprecated

## Kontext
Was ist das Problem, das diese Entscheidung motiviert?

## Entscheidung
Was wird umgesetzt?

## Konsequenzen
Was wird dadurch einfacher? Was wird schwieriger?
```

---

## Phase 2: Implementierungs-Standards

### Prinzipien (aus Backend Architect, angepasst auf .NET Console)
1. **Schema-Validierung** — CSV-Spalten beim Laden prüfen. Fehlende/falsche Spalten → Fail Fast mit klarer Fehlermeldung.
2. **Row-Level Fehlertoleranz** — Einzelne fehlerhafte Zeilen (unbekannter Customer, ungültiger Timestamp) → Request ablehnen, Verarbeitung fortsetzen.
3. **Idempotenz** — Jede RequestId wird maximal einmal verarbeitet. Entscheidungs-Persistierung ermöglicht inkrementelle Runs.
4. **Zeitzonen-Sicherheit** — Alle SLA-Berechnungen in Europe/Vienna. DST-Übergänge explizit mit `TimeZoneInfo` und `DateTimeOffset` handhaben.

### Code-Konventionen
- **Namensgebung**: PascalCase für öffentliche Member, camelCase für lokale Variablen
- **Dependency Injection**: Services klar separieren (CsvService, ValidationService, DecisionService, SlaCalculator)
- **Error Handling**: `try-catch` nur an System-Grenzen (Datei-I/O). Business-Logik gibt typisierte Results zurück (kein Exception-Flow für erwartbare Fälle).
- **Immutability bevorzugen**: Domain-Objekte wo möglich als Records definieren

---

## Phase 3: Code-Review-Checkliste

### Priorisierungs-System (aus Code Reviewer)
Jedes gefundene Problem wird kategorisiert:

- 🔴 **Blocker (Must Fix)** — Korrektheitsfehler, Datenverlust, Security-Issues
- 🟡 **Suggestion (Should Fix)** — Fehlende Validierung, unklare Logik, fehlende Tests
- 💭 **Nit (Nice to Have)** — Stil-Inkonsistenzen, alternative Ansätze

### Projekt-spezifische Review-Punkte

#### 🔴 Blocker
- [ ] Werden alle 7 Business-Szenarien korrekt abgebildet?
- [ ] Ist die SLA-Berechnung DST-sicher (Europe/Vienna)?
- [ ] Wird idempotente Verarbeitung garantiert (keine doppelte Verarbeitung)?
- [ ] Fail-Fast bei CSV-Schema-Fehlern (fehlende Spalten, unlesbare Datei)?
- [ ] Werden Follow-Up-Actions (Smart Meter Upgrade) korrekt persistiert?

#### 🟡 Suggestions
- [ ] Gibt es Unit-Tests für jeden Entscheidungspfad?
- [ ] Werden Edge-Cases abgedeckt (leere CSV, nur Header, doppelte RequestIds)?
- [ ] Ist die Fehlerbehandlung für malformed Daten robust?
- [ ] Sind die SLA-Stunden korrekt (Standard: 48h, Premium: 24h, Meter-Upgrade: +12h)?

#### 💭 Nits
- [ ] Konsistente Namensgebung im gesamten Projekt
- [ ] Sinnvolle Log-Ausgaben für Debugging
- [ ] Output-Format klar und nachvollziehbar

### Review-Format
```
🔴 **Korrektheit: SLA-Berechnung ignoriert DST**
Zeile 87: `DateTime.Now.AddHours(48)` berücksichtigt keine Zeitumstellung.

**Warum:** Am 29. März (Sommerzeit-Umstellung) wird die SLA-Deadline um 1h verfälscht.

**Vorschlag:**
- `TimeZoneInfo.ConvertTimeFromUtc()` mit `DateTimeOffset` verwenden.
```

---

## Phase 4: Test-Analyse & Quality-Gates

### Prinzipien (aus Test Results Analyzer + Reality Checker)

1. **Default: "NEEDS WORK"** — Erst bei überwältigender Evidenz als fertig betrachten.
2. **Datengetrieben** — Keine Behauptungen ohne Test-Ergebnisse.
3. **Alle Tests müssen grün sein** — Kein Roadmap-Schritt ist abgeschlossen, bevor alle Tests durchlaufen.

### Quality-Gates pro Roadmap-Schritt
Jeder Schritt muss folgende Kriterien erfüllen, bevor zum nächsten übergegangen wird:

| Gate | Kriterium |
|------|-----------|
| **Build** | `dotnet build` erfolgreich, keine Warnings als Fehler |
| **Tests** | `dotnet test` — alle Tests grün |
| **Szenarien** | Alle 8 Business-Szenarien (inkl. "Already Processed") abgedeckt |
| **Idempotenz** | Doppelter Run verarbeitet keine Request erneut |
| **DST** | SLA-Berechnung mit DST-Testdaten validiert |

### Test-Analyse nach jedem Testlauf
```markdown
## Testergebnis-Analyse

**Durchlauf**: [Datum/Zeitpunkt]
**Ergebnis**: X bestanden / Y fehlgeschlagen / Z übersprungen

### Fehlgeschlagene Tests
| Test | Fehlerart | Root Cause | Fix |
|------|-----------|------------|-----|
| [Name] | [Assertion/Exception] | [Ursache] | [Vorgeschlagener Fix] |

### Abdeckung
- [ ] Szenario 1: Standard-SLA → Approve ✅/❌
- [ ] Szenario 2: Premium-SLA → Approve ✅/❌
- [ ] Szenario 3: Smart-Meter-Upgrade → Approve + Follow-Up ✅/❌
- [ ] Szenario 4: Unbezahlte Rechnung → Reject ✅/❌
- [ ] Szenario 5: Unbekannter Kunde → Reject ✅/❌
- [ ] Szenario 6: Unbekannter Tarif → Reject ✅/❌
- [ ] Szenario 7: Ungültige Daten → Reject ✅/❌
- [ ] Szenario 8: Bereits verarbeitet → Skip ✅/❌

### Bewertung
**Status**: NEEDS WORK | ACCEPTABLE | READY
```

---

## Phase 5: Git-Workflow & Roadmap

### Prinzipien (aus Git Workflow Master)
1. **Atomic Commits** — Jeder Commit macht genau eine Sache und kann isoliert rückgängig gemacht werden.
2. **Conventional Commits** — Präfix nutzen: `feat:`, `fix:`, `test:`, `chore:`, `docs:`, `refactor:`
3. **Sinnvolle Commit-Messages** — Beschreiben WAS und WARUM, nicht WIE.

### Commit-Ablauf pro Roadmap-Schritt
```
1. Implementierung durchführen
2. `dotnet build` → Muss erfolgreich sein
3. `dotnet test` → Alle Tests müssen grün sein
4. Sinnvoller Commit: `feat: implement SLA calculation with DST support`
5. Commit-Hash in roadmap.md eintragen
6. Zum nächsten Schritt
```

### Commit-Message-Beispiele für dieses Projekt
```
feat: add CSV parsing for customers, tariffs, and requests
feat: implement request validation (unknown customer/tariff, unpaid invoices)
feat: implement SLA calculation with Europe/Vienna timezone support
feat: add idempotent processing with decision persistence
feat: add follow-up action tracking for smart meter upgrades
test: add unit tests for all 8 business scenarios
fix: handle DST transition in SLA deadline calculation
chore: add decisions output file and processing state
docs: create roadmap with implementation steps
```

---

## Zusammenfassung: Der Ablauf

```
┌─────────────────────────────────────┐
│  1. ARCHITEKTUR                     │
│  Domain-Modell, Datenfluss, ADRs    │
├─────────────────────────────────────┤
│  2. IMPLEMENTIERUNG                 │
│  CSV-Parsing → Validierung →        │
│  Business-Logik → Persistierung     │
├─────────────────────────────────────┤
│  3. CODE-REVIEW                     │
│  🔴/🟡/💭 Checkliste durchgehen     │
├─────────────────────────────────────┤
│  4. TEST-ANALYSE                    │
│  Quality-Gates prüfen,              │
│  alle 8 Szenarien verifizieren      │
├─────────────────────────────────────┤
│  5. COMMIT                          │
│  Atomic Commit mit Convention,      │
│  Hash in roadmap.md eintragen       │
└─────────────────────────────────────┘
         ↓ Nächster Roadmap-Schritt ↓
```
