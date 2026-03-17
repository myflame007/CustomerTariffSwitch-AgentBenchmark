Role:
You are a Senior .NET Software Developer

Task:
Your task is to implement a solution according to the provided specification.

Project Structure:
The input files are located in a folder at the project root.
<!-- OPTIONAL: Add the following line if pre-built tests are provided: -->
<!-- Unit tests and integration tests are also provided in the project root. -->

After analyzing what to do, create a roadmap.md including all the neccessary steps
Update them accordingly and set timestamps when you implemented a step successful
<!-- OPTIONAL: Add the following line if the agent should explicitly output results: -->
<!-- Output the final results into a file to compare later on -->

After completing each roadmap step:
- Ensure the solution builds successfully before moving to the next step
- Run all tests and only proceed if all pass
- Create a meaningful git commit
- Record the commit hash next to the completed step in roadmap.md

**Specification:**
Overview
This project involves developing a lightweight .NET console application in C# to
automate processing customer tariff-switch requests. The application reads structured
data from CSV files and applies validation rules to decide whether to approve or reject
each request. If a request is valid, the specified tariff should be applied to the customer
by the SLA due date. If not, the request should be rejected.
The core goals of the application are:
• Validation of input data: Ensure that each request references valid customers
and tariffs, and that the customer has no unpaid invoices.
• Business logic enforcement: Handle special conditions such as smart meter
requirements and SLA-based deadlines.
• Idempotent processing: Guarantee that each request is processed only once,
even across multiple runs.
• Timezone-aware SLA computation: Calculate deadlines based on SLA level and
meter upgrade requirements, using Europe/Vienna local time with correct
handling of daylight-saving time.
All processing is performed in a local machine without reliance on external services. The
application is designed to be robust, handling malformed or incomplete data gracefully
while failing fast on configuration or schema errors. It supports incremental processing
by identifying and handling only newly added requests in subsequent runs.
Input files
• customers.csv – Customer master data (ID, Name, Unpaid-invoice flag, SLA
level, Meter type).
• tariffs.csv – Tariff catalog (ID, Name, Smart-meter requirement, Monthly price).
• requests.csv – Tariff-switch requests (Request ID, Customer ID, Target Tariff ID,
Requested timestamp).
Scenarios for approval handling (business logic)
1. Valid — Standard SLA, no unpaid invoices, no smart-meter required or
already installed
Approve; SLA due = RequestedAt + 48h (Europe/Vienna); no follow-up actions.
2. Valid — Premium SLA, no unpaid invoices, no smart-meter required or
already installed
Approve; SLA due = RequestedAt + 24h (Europe/Vienna); no follow-up actions.
3. Tariff requires a smart meter; customer has classic, no unpaid invoices
Approve; add follow-up action: "Schedule meter upgrade"; SLA due =
(Standard/Premium rule) + 12h.
4. Customer has unpaid invoices
Reject with reason: Unpaid invoice.
5. Unknown customer ID
Reject with reason: Unknown customer.
6. Unknown tariff ID
Reject with reason: Unknown tariff.
7. Invalid request data (e.g., bad timestamp or malformed fields)
Reject with reason: Invalid request data.
8. Already processed (from a previous run)
Skip (do not process again).
DateTime rules
• Compute all SLA due times in Europe/Vienna.
• Be DST-safe: add durations in local time semantics and output ISO-8601.
Expected behavior of the app
On each run, the app loads the three CSVs, identifies all requests that have not yet been
handled, and processes only those, without any user interaction during runtime. If new
rows are later added to requests.csv, the following run processes just the newly added
requests (previously handled requests are not re-processed). Row-level data issues
(e.g., unknown customer/tariff, bad timestamp) should reject that request with an
apparent reason without stopping the run. At the same time, configuration or CSV
schema errors (e.g., missing columns, missing or unreadable file) should fail fast with an
actionable error message so the user can correct the input. The app must seamlessly
handle newly added customers, tariffs, and requests in their respective CSVs, using the
latest available data at runtime.
Notes
• You may modify the given CSV files (ex. add columns or rows) or create additional
output files (CSV/JSON) as you deem appropriate.
• Ensure idempotent processing: each RequestId is handled at most once
across runs. If additional rows are later appended to requests.csv, a subsequent
run must process only the new requests (the implementation approach is up to
you).
• Please add unit tests in the areas where you think they are necessary.
• All follow-up actions (Smart meter upgrade) must be persisted, including their
deadlines. It's up to you to decide how you present them.
• Keep documentation short and precise. We should be able to rebuild and test the
app without additional clarification.
