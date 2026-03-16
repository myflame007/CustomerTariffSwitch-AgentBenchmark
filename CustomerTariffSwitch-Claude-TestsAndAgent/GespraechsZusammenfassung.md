# Gesprächs-Zusammenfassung: AI-Workflow Integration

**Datum:** 16. März 2026

---

## Ausgangslage

Das Ziel war, hilfreiche Tools aus dem Projekt **AI-Workflow** (`C:\Users\robert.stickler\source\repos\AI-Workflow`) für das **CustomerTariffSwitch**-Projekt zu evaluieren und zu nutzen.

Das AI-Workflow-Repository enthält mehrere geklonte Repos:
- **Impeccable** — Frontend-Design-Skills mit 17 Befehlen (audit, polish, animate, etc.)
- **Agency Agents** — 150+ domain-spezifische KI-Spezialistenpersonas in 14+ Abteilungen
- **OpenViking** — Context-Management für KI-Agents
- **AntiGravity** — Web/3D-fokussiert

---

## Analyse: Was ist relevant?

### Relevant für das Projekt

**Agency Agents — Engineering:**
| Agent | Relevanz |
|---|---|
| `engineering-software-architect` | Architektur, DDD, Trade-off-Analyse |
| `engineering-backend-architect` | Schema-Design, Datenfluss, Error-Handling |
| `engineering-code-reviewer` | Code-Review mit 🔴/🟡/💭 Priorisierung |
| `engineering-git-workflow-master` | Commit-Strategie, passend zur Roadmap-Anforderung |
| `engineering-security-engineer` | Sicherheits-Audit |

**Agency Agents — Testing:**
| Agent | Relevanz |
|---|---|
| `testing-test-results-analyzer` | Testergebnisse analysieren, Quality-Metriken |
| `testing-reality-checker` | Skeptische Endkontrolle, Evidence-based |
| `testing-workflow-optimizer` | Prozessoptimierung |

**Agency Agents — Project Management:**
| Agent | Relevanz |
|---|---|
| `project-management-project-shepherd` | Projektplanung, Roadmap-Steuerung |

### Nicht relevant
- **Impeccable** — Frontend-Design-Skills, nicht anwendbar auf .NET Console-App
- **OpenViking, AntiGravity** — Web/3D-fokussiert

---

## Drei diskutierte Ansätze

### Ansatz 1: Agent-Dateien ins Projekt kopieren
- **Pro:** Einfach, sofort verfügbar
- **Contra:** Viel irrelevanter Inhalt (Laravel, Microservices), kein klarer Workflow, Redundanz möglich

### Ansatz 2: Einzelne Agents gezielt einsetzen
- **Pro:** Schlank, kein Ballast
- **Contra:** Manueller Aufwand bei jedem Schritt, kein automatisierter Ablauf

### Ansatz 3: Kombinierter, projektspezifischer Workflow ⭐ (gewählt)
- **Pro:** Maßgeschneidert auf .NET Console/CSV-Projekt, destillierte Best Practices, klarer sequenzieller Ablauf, kein irrelevantes Material
- **Contra:** Einmaliger Erstellungsaufwand

---

## Entscheidung

**Ansatz 3** wurde gewählt. Die besten Konzepte aus 6 Agency Agents wurden in ein einziges, projektspezifisches Workflow-Dokument destilliert:

→ **`AgentWorkflow.md`** — enthält 5 Phasen:

| Phase | Inhalt | Quelle |
|---|---|---|
| 1. Architektur & Domain-Modellierung | DDD-Prinzipien, ADR-Template, Datenfluss | Software Architect |
| 2. Implementierungs-Standards | Schema-Validierung, Fehlertoleranz, Idempotenz, DST-Sicherheit | Backend Architect |
| 3. Code-Review-Checkliste | 🔴/🟡/💭 System, projektspezifische Checkpoints | Code Reviewer |
| 4. Test-Analyse & Quality-Gates | "Default: NEEDS WORK", Szenarien-Abdeckung, datengetrieben | Test Results Analyzer + Reality Checker |
| 5. Git-Workflow & Roadmap | Atomic Commits, Conventional Commits, Commit-Ablauf | Git Workflow Master |

---

## Erzeugte Dateien

| Datei | Zweck |
|---|---|
| `AgentWorkflow.md` | Projektspezifische Workflow-Instruction (5 Phasen) |
| `GespraechsZusammenfassung.md` | Diese Zusammenfassung |
