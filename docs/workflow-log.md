# Reusable workflow log

---

## Slice 1 — Domain model, in-memory collection, and menu loop

- **Goal:** Create `MoneyItem`, `ItemType`, `SortField`, `ItemCollection`, and a working console menu with add, list, sort, filter, edit, and remove.
- **Agent used:** CSharp Architect → CSharp Implementer
- **Prompt files:** `02-design-slice.prompt.md`, `03-implement-slice.prompt.md`
- **Acceptance questions answered:** M1–M16, Q1–Q4, Q7

### Context provided
- `docs/project-context.md`, `docs/acceptance-checklist.md`
- Ambiguities A1–A7 resolved in `docs/architecture.md` before implementation

### Outcome
- **Files created:** `Domain/ItemType.cs`, `Domain/SortField.cs`, `Domain/MoneyItem.cs`, `Services/ItemCollection.cs`, `Program.cs`
- **Build:** zero errors, zero warnings (`dotnet build`)
- **Tests:** none yet (test project created in Slice 2)
- **Manual check:** menu loop, add/edit/remove/sort/filter all exercised interactively

### Review
- Seed data added in `Program.cs` for early demo; marked for removal once persistence works.
- No issues found by Code Reviewer.

### Learning
- Resolving all ambiguities in an architecture doc before writing any code eliminated back-and-forth during implementation.
- `record` + `with` expression is a clean, student-explainable way to handle immutable edits.
- Top-level statements keep `Program.cs` short and readable for a student audience.

---

## Slice 2 — Save and Load (JSON persistence)

- **Goal:** Implement menu options 8 (Save) and 9 (Load), auto-load on startup, handle missing and malformed files, add test project with persistence tests.
- **Agent used:** Project Documenter (design) → CSharp Implementer → Test Designer → Project Documenter (this entry)
- **Prompt files:** `02-design-slice.prompt.md`, `03-implement-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions answered:** M17–M20, Q6, Q8

### Context provided
- All Slice 1 source files, `docs/architecture.md` Slice 2 design section
- Decision A2 (explicit save menu), A3 (JSON + working directory) from architecture doc

### Outcome
- **Files created:** `Services/JsonPersistence.cs`, `MoneyTracking.Tests/MoneyTracking.Tests.csproj`, `MoneyTracking.Tests/PersistenceTests.cs`
- **Files edited:** `Program.cs` (DataFile constant, auto-load, cases 8 and 9, seed data removed), `README.md` (Data file section, setup/build/run instructions)
- **Build:** zero errors after fixing two issues (see Corrections below)
- **Tests:** `dotnet test MoneyTracking.Tests/MoneyTracking.Tests.csproj` — 3 tests pass

### Review
- **Finding 1:** Test project `.csproj` had duplicate `<ItemGroup>` with wildcard xunit references appended alongside the pinned ones — broke NuGet restore and caused `[Fact]` not found.  
  **Fix:** Removed duplicate `<ItemGroup>`.
- **Finding 2:** `MoneyTracking.csproj` implicit `**/*.cs` glob included `MoneyTracking.Tests/*.cs`, causing duplicate assembly attribute errors when running `dotnet run`.  
  **Fix:** Added `<Compile Remove="MoneyTracking.Tests/**" />` to `MoneyTracking.csproj`.
- **Finding 3:** `PersistenceTests.cs` was missing `using Xunit;` — `ImplicitUsings` does not include xunit.  
  **Fix:** Added the using directive.
- **Remaining risk:** Q5 (sort/filter unit tests) is still ⚠️ — `ItemCollectionTests.cs` has not been created as a file yet (only proposed in a previous assistant message).

### Learning
- When a test project is a subfolder of an Exe project, always add `<Compile Remove="TestFolder/**" />` to the main `.csproj` to prevent glob overlap.
- xunit is not part of `ImplicitUsings`; `using Xunit;` must be explicit even with `<ImplicitUsings>enable</ImplicitUsings>`.
- Do not append duplicate `<ItemGroup>` blocks — one ItemGroup per logical group, pinned versions only.
- The atomic write pattern (temp file + `File.Move`) is worth teaching: it prevents data corruption and is easy to explain.
- **Reusable instruction added:** In the architecture doc, note that test projects must be excluded from the main project's compile glob.

---

## Code review — Slice 2 findings and corrections

- **Goal:** Review completed implementation, fix two robustness issues found, close all mandatory acceptance questions.
- **Agent used:** Code Reviewer → CSharp Implementer → Project Documenter
- **Prompt files:** `04-review-slice.prompt.md`, `03-implement-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions affected:** Q2 (improved), Q8 (confirmed ✅)

### Context provided
- All source files, both test files, `docs/acceptance-checklist.md`

### Outcome
- **Files edited:** `Program.cs` (two changes listed below)
- **Build:** zero errors, zero warnings (static analysis)
- **Tests:** 13 tests — static analysis confirms no errors; `dotnet test` to be run manually
- **Manual check:** not applicable (robustness fixes, not new features)

### Corrections applied

| Finding | File | Change |
|---------|------|--------|
| 2-A Culture sensitivity | `Program.cs` `PromptDecimal` | Added `NumberStyles.Any, CultureInfo.InvariantCulture` to `decimal.TryParse`; updated user message to mention `.` separator |
| 3-A Null guard in sub-prompts | `Program.cs` `PromptSortField`, `PromptAscending`, `PromptItemType` | Added `?? ""` to each `Console.ReadLine()` inside `switch` |

### Findings not actioned (by decision)

| Finding | Reason |
|---------|--------|
| 3-B `Remove` return value ignored | Id comes from `GetAll()` immediately before the call — always valid; fixing would add noise |
| 4-A Redundant `Save` comment | Minor; not worth touching a working file for a cosmetic comment |
| 4-B `var` inconsistency between test files | Presentation only; no behavioral impact |
| 4-C No feedback when amount `0` is silently kept | Consistent with A4 decision; acceptable for student project |
| 5-A/5-B Demo presentation notes | Verbal notes, not code defects |

### Learning
- `decimal.TryParse` without a culture argument uses the OS locale — always pass `CultureInfo.InvariantCulture` for user-typed numbers in a cross-locale app.
- `Console.ReadLine()` inside a `switch` expression is safe for the happy path but should have a `?? ""` guard for consistency with the top-level menu.
- A code review pass after each slice catches issues that automated analysis misses (locale, null in stdin).
- **Reusable instruction added:** Always pass `CultureInfo.InvariantCulture` to numeric `TryParse` calls at console boundaries.

---

## Slice 3 — Display enrichment: color, balance, totals (O1, O2, O4)

- **Goal:** Enrich the item list with colored rows (income green, expense red) and a summary line (income total, expenses total, net balance). No structural changes to any other feature.
- **Agent used:** Project Documenter (optional feature analysis) → CSharp Architect (Slice 3 design) → CSharp Implementer → Code Reviewer → CSharp Implementer (correction) → Project Documenter (this entry)
- **Prompt files:** `01-analyze-requirements.prompt.md`, `02-design-slice.prompt.md`, `03-implement-slice.prompt.md`, `04-review-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions answered:** O1 ✅, O2 ✅, O4 ✅

### Context provided
- All existing source files, `docs/architecture.md` Slice 3 design section
- Optional feature analysis with risk/constraint table from requirements pass

### Outcome
- **Files edited:** `Program.cs` (`PrintList` only)
- **Build:** zero errors, zero warnings (static analysis)
- **Tests:** 13 existing tests unaffected (they test `ItemCollection` and `JsonPersistence`, not `PrintList`)
- **Manual check:** not run (terminal tool disabled); visual check of code logic confirmed correct

### Changes applied

| Change | Detail |
|--------|--------|
| Colored rows | `Console.ForegroundColor` set to `Green`/`Red` per row; `Console.ResetColor()` in `finally` block |
| Summary line | Income total, expenses total, net balance printed below separator after every list |
| Summary scope | Computed from the passed-in `items` list — reflects filtered/sorted views correctly |

### Corrections applied (from code review)

| Finding | Fix |
|---------|-----|
| 2-B `ResetColor` not in `finally` | Moved `Console.ResetColor()` into a `try/finally` block so it runs even if `Console.WriteLine` throws |

### Findings not actioned (by decision)

| Finding | Reason |
|---------|--------|
| 2-A `Balance: +0.00` on zero balance | Cosmetic only — `+0.00` is technically correct |
| 4-A Summary line width misalignment | Cosmetic only — not worth adding padding logic for a student project |

### Learning
- `Console.ForegroundColor` is global process state — always pair it with `Console.ResetColor()` in a `finally` block, not just sequentially after output.
- Summary computed from the passed-in list (not `GetAll()`) is the right pattern: the same function works correctly for filtered and sorted views without any change.
- Adding color and totals to an existing display function requires zero changes to domain, service, or persistence layers — a clean demonstration of layer separation.
- **Reusable instruction added:** Always wrap `Console.ForegroundColor` changes in `try/finally { Console.ResetColor(); }` to prevent terminal color bleed.
