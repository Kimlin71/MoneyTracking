# Acceptance checklist


---

## Conflicts and ambiguities

| # | Issue | Location | Resolution needed |
|---|-------|----------|-------------------|
| A1 | Month representation is unspecified — numeric (1–12), abbreviated name ("Jan"), or full name ("January") are all possible | project-context.md §Design decisions | Architect must choose and document one validated type |
| A2 | "Save to file" is required but the trigger is not specified — save-on-quit, save-on-change, or explicit save command are all consistent with the spec | project-context.md §Persistence | Architect must decide and document |
| A3 | The file format and location are unspecified — JSON, CSV, and a plain text file in the working directory are all valid | project-context.md §Design decisions | Architect must decide and document in README |
| A4 | "Edit existing item" does not specify which fields are editable — title only, all fields, or a field-by-field prompt | project-context.md §Interaction | Implementer must decide; all fields is the safe default |
| A5 | Item selection for edit/remove is unspecified — by display index, by stable ID, or by title search | project-context.md §Design decisions | Architect must choose a stable identifier approach |
| A6 | Income/expense model (enum, subclass, bool flag) is an open design decision | project-context.md §Design decisions | Architect must choose and document |
| A7 | Whether sorting and filtering can be combined (e.g., show only expenses sorted by amount) is not stated | project-context.md §Collection | Clarify before implementing filter+sort UI |

---

## Mandatory requirements

Each question maps to the evidence expected to answer Yes.

### Domain model

| # | Question | Evidence |
|---|----------|----------|
| M1 | Does a `MoneyItem` (or equivalent) class exist with `Title`, `Amount`, and `Month` properties? | Source file containing the domain class |
| M2 | Is `decimal` used for `Amount` (not `double` or `float`)? | Domain class source; confirmed by grep or compiler warning absence |
| M3 | Is income/expense explicitly distinguished in the domain model (enum, subclass, or equivalent — not a raw bool)? | Domain class source and documented design decision |
| M4 | Is month represented by a validated type or value that rejects out-of-range input (e.g., 0 or 13)? | Domain class or validation layer source; manual test of bad month input |

### Collection and display

| # | Question | Evidence |
|---|----------|----------|
| M5 | Can the application display all items showing title, amount, month, and income/expense type? | Console output from running the app with items loaded |
| M6 | Can items be sorted by month ascending and descending? | Console output; or deterministic unit test |
| M7 | Can items be sorted by amount ascending and descending? | Console output; or deterministic unit test |
| M8 | Can items be sorted by title ascending and descending? | Console output; or deterministic unit test |
| M9 | Can the display be filtered to show only expense items? | Console output; or deterministic unit test |
| M10 | Can the display be filtered to show only income items? | Console output; or deterministic unit test |

### Interaction

| # | Question | Evidence |
|---|----------|----------|
| M11 | Does the application provide a text-based menu that makes add, list, sort, filter, edit, remove, and quit discoverable? | Console output showing the menu |
| M12 | Can a user add an income item through the CLI? | Console session recording or manual test |
| M13 | Can a user add an expense item through the CLI? | Console session recording or manual test |
| M14 | Can a user edit an existing item (at least one field)? | Console session recording or manual test |
| M15 | Can a user remove an existing item? | Console session recording or manual test |
| M16 | Can a user quit the application via a menu option? | Console session recording or manual test |

### Persistence

| # | Question | Evidence |
|---|----------|----------|
| M17 | Is the item list saved to a file (format and location documented in README)? | Data file present after running the app; README documents path and format |
| M18 | Is the saved state restored when the application is restarted? | Manual test: add items, quit, restart, verify items reappear |
| M19 | Does the application handle a missing data file without crashing (e.g., starts with an empty list)? | Manual test: delete data file, restart app |
| M20 | Does the application handle a malformed data file explicitly (error message, not silent data loss or unhandled exception)? | Manual test: corrupt data file, restart app; observe message |

---

## Quality and verification

| # | Question | Evidence |
|---|----------|----------|
| Q1 | Does the solution build with zero errors and zero suppressed warnings? | `dotnet build` output |
| Q2 | Are invalid amounts (non-numeric, negative if disallowed) handled without crashing? | Manual test or unit test |
| Q3 | Are invalid month values handled without crashing? | Manual test or unit test |
| Q4 | Are invalid menu choices handled without crashing? | Manual test or unit test |
| Q5 | Are sorting and filtering behaviors covered by at least one deterministic unit test each? | Test project with passing tests (`dotnet test` output) |
| Q6 | Is the save/load round trip covered by a test using an isolated temporary file? | Test project with passing test |
| Q7 | Is domain logic (sorting, filtering, model) kept in classes separate from console I/O and file I/O? | Source file structure |
| Q8 | Are setup, build, run, and data-file location instructions present in README? | README content |

---

## Optional features

These are not required for acceptance but add value if present. Each must not break any mandatory behavior.

| # | Question | Evidence |
|---|----------|----------|
| O1 | Does the application display a running total or balance (income minus expenses)? | Console output |
| O2 | Does the application support totals broken down by month or by type? | Console output |
| O3 | Does the application support a text search or keyword filter? | Console behavior |
| O4 | Does the application use colored console output to distinguish income from expense? | Console behavior |
| O5 | Does the application support CSV export? | Exported file |
| O6 | Does the application support pagination for large lists? | Console behavior |
| O7 | Does the application include unit tests beyond the mandatory behaviors listed in Q5–Q6? | Test project |