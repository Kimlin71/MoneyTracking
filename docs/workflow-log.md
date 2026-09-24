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

---

## Slices 4+ — UX redesign, keyword search, tests, and robustness fixes

- **Goal:** Add keyword search (O3), redesign menu UX to match student presentation style, fix two input robustness issues, add 17 new tests covering search and decimal parsing.
- **Agent used:** CSharp Architect (design) → CSharp Implementer (multiple passes) → Code Reviewer → Project Documenter
- **Prompt files:** `02-design-slice.prompt.md`, `03-implement-slice.prompt.md`, `04-review-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions answered:** O3 ✅; O1 extended (balance also shown in menu header)

### Context provided
- All existing source files, acceptance checklist, architecture doc Slice 4 design

### Outcome
- **Files edited:** `Program.cs` (all changes below), `Services/ItemCollection.cs` (new method)
- **Files created:** `MoneyTracking.Tests/KeywordSearchTests.cs` (8 tests), `MoneyTracking.Tests/DecimalParsingTests.cs` (9 tests)
- **Build:** zero errors, zero warnings
- **Tests:** `dotnet test` — **30 tests, 0 failed, 0 skipped** (verified by user)

### Changes applied

| Change | Detail |
|--------|--------|
| Keyword search | `ItemCollection.GetByKeyword(string)` — case-insensitive substring match on Title; `SearchItems()` in `Program.cs`; menu option 6 |
| Balance header | Current balance shown in green/red above the menu before every prompt |
| Menu redesign | Numbered list with descriptions; sub-menus for Show / Add / Edit+Remove |
| Save and Quit | Option 0 saves then exits; removed separate Save option |
| Discard unsaved | Option 7 with `y/n` confirmation prompt — reloads from last saved file |
| Type removed from Edit | Type is set at add time and not editable (A4 update) |
| `EditOrRemove` re-prompt | Loops on invalid input, consistent with other sub-menus |
| Decimal input fix | Both `PromptDecimal` and `EditItem` amount accept comma or dot as separator |
| Summary alignment | Separator auto-sizes to match summary line width |
| `AddOrChooseType` fix | Loops on invalid input instead of silently defaulting to Income |
| Mock data | 10 realistic items in `moneyitems.json` for demo |
| Save reminders | Added after Add, Edit, Remove: `"Choose '0. Save and Quit' to save"` |

### Learning
- `GetByKeyword` using `StringComparison.OrdinalIgnoreCase` is the correct locale-safe approach for a student project — no regex needed.
- Accepting both `.` and `,` via `.Replace(',', '.')` before `TryParse` with `InvariantCulture` solves the Swedish locale issue cleanly without changing the domain model.
- Dynamic separator width (`summaryLine.Length`) is simpler and more correct than a hardcoded constant.
- Sub-menus keep the main menu short and discoverable without adding more numbered options.
- The balance header makes the app's purpose immediately visible on every menu redraw.
- **Reusable instruction added:** When removing a feature (Type from Edit), update the acceptance checklist ambiguity table (A4) to record the decision and rationale.

---

## Slice 5 — Month sub-filter in Filter (option 5)

- **Goal:** Extend option 5 so users can optionally narrow results by a specific month in addition to type. Pressing Enter skips the month filter (original behavior preserved).
- **Agent used:** CSharp Architect → CSharp Implementer → Code Reviewer → CSharp Implementer (corrections) → Project Documenter
- **Prompt files:** `02-design-slice.prompt.md`, `03-implement-slice.prompt.md`, `04-review-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions affected:** M9, M10 (evidence updated)

### Context provided
- All existing source files, architecture doc Slice 5 design

### Outcome
- **Files edited:** `Services/ItemCollection.cs`, `Program.cs`, `MoneyTracking.Tests/ItemCollectionTests.cs`
- **Build:** zero errors, zero warnings
- **Tests:** static analysis confirms zero errors; run `dotnet test` to verify **33 tests pass** (30 existing + 3 new)

### Changes applied

| Change | Detail |
|--------|--------|
| New service overload | `GetFiltered(ItemType type, int? month)` — `null` = no month restriction; existing single-arg overload untouched |
| `FilterItems` updated | Calls new overload with result of `PromptOptionalMonth()` |
| `PromptOptionalMonth` added | Loops until 1–12 or Enter (returns `null`); consistent with existing prompt helpers |
| Menu label updated | Option 5: `"Filter by type and optional month"` |
| 3 new tests added | `GetFiltered_ByTypeAndMonth_*` — specific month, null month = same as type-only overload, no match |

### Findings not actioned (by decision)
None — all review findings (5-A menu label, 4-A test gap) were addressed.

### Learning
- A nullable `int?` parameter is the cleanest way to make a filter optional without adding a new overload for every combination.
- Always test that `null` month produces the same result as the original single-argument overload — it proves the overload is a strict extension, not a replacement.
- Menu labels should be updated whenever a feature is extended, even if the change is minor.
- **Reusable instruction added:** When adding an overload, include a test that proves `null` produces identical output to the original call — it guards against accidental behavior drift.

---

## Security slice — SEC-2, SEC-3, SEC-4 hardening + tests

- **Goal:** Fix three actionable security findings from the security best practices report and add unit tests for each fix.
- **Agent used:** CSharp Architect (fixes) → Test Designer (tests + `IsPathSafe` extraction) → Project Documenter (this entry)
- **Prompt files:** `02-design-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions affected:** O5 (evidence updated — path guard noted), O7 (test count updated to 39)

### Context provided
- `security_best_practices_report.md` (5 findings, 3 open at start of slice)
- All service source files and existing test files

### Outcome
- **Files edited:** `Services/CsvExport.cs`, `Services/JsonPersistence.cs`, `Program.cs`, `MoneyTracking.Tests/CsvExportTests.cs`, `MoneyTracking.Tests/PersistenceTests.cs`
- **Build:** zero errors, zero warnings (static analysis confirmed)
- **Tests:** 47 total — `dotnet test` result: **47 passed, 0 failed, 0 skipped** (verified 2026-09-23)

### Changes applied

| Finding | Fix | Test |
|---------|-----|------|
| SEC-2 — CSV path traversal | `ExportToCsv` in `Program.cs` resolves path with `Path.GetFullPath` and checks against `Environment.CurrentDirectory` | Extracted check into `CsvExport.IsPathSafe(string, string)`; 4 tests in `CsvExportTests` |
| SEC-3 — No explicit `MaxDepth` on deserialization | `JsonPersistence` added `_readOptions` with `MaxDepth = 8` passed to every `Deserialize` call | `Load_DeeplyNestedJson_ReturnsEmptyList` in `PersistenceTests` |
| SEC-4 — Orphaned `.tmp` on `File.Move` failure | `JsonPersistence.Save` wraps write+move in try/catch; deletes `.tmp` before re-throwing | `Save_WhenMoveSucceeds_NoTmpFileRemains` in `PersistenceTests` |

### Findings not actioned (by decision)

| Finding | Reason |
|---------|--------|
| SEC-1 — Data file in world-readable binary dir | Changing `AppContext.BaseDirectory` alters the documented data file path and may break saved data for existing users; requires a deliberate product decision before implementing |
| SEC-5 — No title length upper bound | Low risk (no buffer overflow in C#); cosmetic display issue only; out of scope for this slice |

### Design decision
Extracted `IsPathSafe(string fullPath, string safeDir)` as an `internal static` method on `CsvExport` rather than keeping the guard inline in `Program.cs`. This made the logic unit-testable without involving the console layer, and kept `Program.cs` calling a named predicate instead of a multi-line inline condition.

### Learning
- Path-guard logic belongs in a service method, not inline in a console function — it can then be unit-tested without any console mocking.
- `internal` visibility is the correct choice for test-supporting helpers that should not be part of the public API.
- `MaxDepth` on deserialization is cheap and explicit; always set it when reading untrusted files, even in a local app.
- A try/catch (not try/finally) is the right pattern for cleanup-then-rethrow when you want to ensure a side effect (file deletion) happens only on failure, not on success.
- **Reusable instruction added:** When a security guard is added inline in a console function, immediately ask whether it can be extracted into the service layer as a named predicate — testability almost always justifies the one-line extraction.

---

## Documentation pass — beginner comments + verified test count

- **Goal:** Add explanatory comments for C# beginners across all source and test files; update all documentation to reflect the verified test count of 47.
- **Agent used:** Project Documenter
- **Prompt files:** `05-document-slice.prompt.md`
- **Acceptance questions affected:** none — comment-only and doc changes

### Context provided
- All source files, test files, README, workflow log, acceptance checklist
- Live `dotnet test` result: **47 passed, 0 failed, 0 skipped**

### Outcome
- **Files edited (comments):** `MoneyTracking.Tests/ItemCollectionTests.cs`, `MoneyTracking.Tests/PersistenceTests.cs`, `MoneyTracking.Tests/CsvExportTests.cs`, `MoneyTracking.Tests/KeywordSearchTests.cs`, `Program.cs`
- **Files edited (counts):** `README.md`, `docs/acceptance-checklist.md`, `docs/workflow-log.md`
- **Build:** zero errors (confirmed via static analysis after every edit)
- **Tests:** 47 passed — verified by user running `dotnet test`

### Comments added

| File | What was explained |
|------|--------------------|
| `ItemCollectionTests.cs` | `[Fact]` discovery, Arrange/Act/Assert pattern |
| `PersistenceTests.cs` | Why real temp files are used; `finally` cleanup purpose |
| `CsvExportTests.cs` | Same temp-file/finally pattern explanation |
| `KeywordSearchTests.cs` | Default parameter values in the `Item` helper |
| `Program.cs` | C# property pattern `is { Length: > 0 } t` in `EditItem` |

### Decision
Only added comments where the code pattern is non-obvious to a beginner (advanced syntax, design rationale). Did not add comments that restate what the next line does.

### Learning
- Place file-level comments *inside* the namespace block (or on the line directly above the class declaration with an explicit newline) — a comment placed between a file-scoped `namespace` statement and a `class` keyword merges onto the class line if the replacement tool drops the trailing newline.
- **Reusable instruction:** Always verify with the error checker immediately after adding comments near class/namespace boundaries.

---

## SEC-1 fix — data file moved to user-specific directory

- **Goal:** Fix SEC-1 (HIGH): stop writing financial data to the world-readable binary directory; write to the OS-standard user-specific application data directory with restrictive permissions.
- **Agent used:** CSharp Implementer → Project Documenter (this entry)
- **Prompt files:** `03-implement-slice.prompt.md`, `05-document-slice.prompt.md`
- **Acceptance questions affected:** A3 (resolution updated), M17 (evidence updated), Q8 (README already updated by implementer)

### Context provided
- `security_best_practices_report.md` SEC-1 finding
- Current `Program.cs` and `Services/JsonPersistence.cs`

### Outcome
- **Files edited:** `Program.cs`, `Services/JsonPersistence.cs`, `security_best_practices_report.md`, `README.md`, `docs/architecture.md`, `docs/acceptance-checklist.md`
- **Build:** zero errors (static analysis confirmed)
- **Tests:** 47 tests unaffected — no test touches `DataFile` path construction; run `dotnet test` to confirm

### Changes applied

| File | Change |
|------|--------|
| `Program.cs` | `DataFile` now derived from `Environment.SpecialFolder.LocalApplicationData` + `"MoneyTracking"` subfolder; `Directory.CreateDirectory` ensures the folder exists on first run |
| `Services/JsonPersistence.cs` | `File.SetUnixFileMode(path, UserRead \| UserWrite)` called after every successful save; guarded by `OperatingSystem.IsWindows()` for cross-platform safety |
| `security_best_practices_report.md` | SEC-1 marked ✅ Fixed with applied code, OS-specific paths, and header date updated |
| `README.md` | Data file table expanded with per-OS paths and permissions row; roadmap updated |
| `docs/architecture.md` | A3 decision updated with new location and rationale |
| `docs/acceptance-checklist.md` | Header date, A3 resolution, and M17 evidence updated |

### Data file locations after fix

| OS | Path |
|----|------|
| macOS | `~/Library/Application Support/MoneyTracking/moneyitems.json` |
| Linux | `~/.local/share/MoneyTracking/moneyitems.json` |
| Windows | `%LOCALAPPDATA%\MoneyTracking\moneyitems.json` |

### Open risk
Existing saved data in `bin/Debug/net10.0/moneyitems.json` (or the old working directory) is **not migrated automatically**. Users who had data before this change must copy the file to the new location manually.

### Learning
- `Environment.SpecialFolder.LocalApplicationData` is the correct cross-platform choice for user-private app data: it maps to `~/Library/Application Support` on macOS, `~/.local/share` on Linux, and `%LOCALAPPDATA%` on Windows — all user-owned and not world-readable by default.
- `File.SetUnixFileMode` (available since .NET 7) is the clean way to enforce `600` permissions without invoking external processes. Always guard it with `OperatingSystem.IsWindows()` so the code compiles and runs on all platforms.
- `OperatingSystem.IsWindows()` is evaluated at runtime; `RuntimeInformation.IsOSPlatform` is the older equivalent — prefer `OperatingSystem.*` in .NET 5+.
- **Reusable instruction added:** For any app that stores user data, set the data directory to `LocalApplicationData/<AppName>/` and apply `600` permissions after the first write — both steps are required; the directory alone is not sufficient if it inherits a permissive umask.

---

## Documentation pass — README file structure updated (2026-09-24)

- **Goal:** Update `README.md` to reflect the actual workspace file structure, including files that had accumulated since the last documentation pass.
- **Agent used:** Project Documenter
- **Prompt files:** `05-document-slice.prompt.md`
- **Acceptance questions affected:** none — documentation only

### Context provided
- Workspace directory listing, all source files, current `README.md`, `docs/workflow-log.md`

### Outcome
- **Files edited:** `README.md` (Project Structure tree), `docs/workflow-log.md` (this entry)
- **Build:** not re-run (no code changes)
- **Tests:** not re-run (no code changes)

### Changes applied

| Change | Detail |
|--------|--------|
| Added `MoneyTracking.Tests.csproj` to tree | Was missing from the test folder listing |
| Added `docs/` subtree with all four files | `security_best_practices_report.md` was previously omitted |
| Added `media/` folder | `UML Diagram Money Tracker.drawio` now shown |
| Added `scripts/` folder | `render-uml.js` now shown |
| Added `MoneyTracking.sln` and `AGENTS.md` | Top-level files that were absent from the tree |
| Reorganised tree order | `docs/` moved above `Program.cs` to group supporting files together |
| Added legacy data file note | Explains that `moneyitems.json` at project root is a legacy artifact; live data writes to `LocalApplicationData` |

### Learning
- The Project Structure tree in a README drifts silently as files are added — verify it against the actual directory listing after every slice.
- **Reusable instruction:** After any slice that creates or moves files, open the README Project Structure tree and diff it against the actual workspace listing before closing the task.
