# Acceptance checklist

**Last updated:** 2026-09-22 — Slice 3 delivered: O1, O2, O4 ✅

---

## Conflicts and ambiguities — all resolved

| # | Issue | Resolution |
|---|-------|-----------|
| A1 | Month representation | Plain `int` (1–12), validated in `PromptMonth()` before `MoneyItem` is constructed |
| A2 | Save trigger | Explicit menu option 8 (Save); auto-load on startup |
| A3 | File format and location | `moneyitems.json` in working directory, JSON via `System.Text.Json`; documented in README |
| A4 | Editable fields | All four fields (title, amount, month, type); pressing Enter keeps the current value |
| A5 | Item selection | 1-based display index shown in list; backed by stable `Guid Id` for the actual operation |
| A6 | Income/expense model | `ItemType` enum (`Income` / `Expense`) in `Domain/ItemType.cs` |
| A7 | Sort + filter combination | Independent operations applied in sequence (filter first, then sort on the filtered result) |

---

## Mandatory requirements

**Key:** ✅ Yes — verified &nbsp;|&nbsp; ⚠️ Partial &nbsp;|&nbsp; ❌ Not yet

### Domain model

| # | Question | Status | Evidence |
|---|----------|--------|----------|
| M1 | Does a `MoneyItem` (or equivalent) class exist with `Title`, `Amount`, and `Month` properties? | ✅ | `Domain/MoneyItem.cs` — record with `Title`, `Amount`, `Month`, `Type`, `Id` |
| M2 | Is `decimal` used for `Amount` (not `double` or `float`)? | ✅ | `Domain/MoneyItem.cs` line 7: `decimal Amount` |
| M3 | Is income/expense explicitly distinguished in the domain model (enum, subclass, or equivalent — not a raw bool)? | ✅ | `Domain/ItemType.cs` — `enum ItemType { Income, Expense }` |
| M4 | Is month represented by a validated type or value that rejects out-of-range input (e.g., 0 or 13)? | ✅ | `PromptMonth()` in `Program.cs` loops until `int.TryParse` succeeds and value is 1–12 |

### Collection and display

| # | Question | Status | Evidence |
|---|----------|--------|----------|
| M5 | Can the application display all items showing title, amount, month, and income/expense type? | ✅ | `PrintList()` in `Program.cs` — menu option 1 |
| M6 | Can items be sorted by month ascending and descending? | ✅ | `ItemCollection.GetSorted(SortField.Month, …)` — menu option 4 |
| M7 | Can items be sorted by amount ascending and descending? | ✅ | `ItemCollection.GetSorted(SortField.Amount, …)` — menu option 4 |
| M8 | Can items be sorted by title ascending and descending? | ✅ | `ItemCollection.GetSorted(SortField.Title, …)` — menu option 4 |
| M9 | Can the display be filtered to show only expense items? | ✅ | `ItemCollection.GetFiltered(ItemType.Expense)` — menu option 5 |
| M10 | Can the display be filtered to show only income items? | ✅ | `ItemCollection.GetFiltered(ItemType.Income)` — menu option 5 |

### Interaction

| # | Question | Status | Evidence |
|---|----------|--------|----------|
| M11 | Does the application provide a text-based menu that makes add, list, sort, filter, edit, remove, save, load, and quit discoverable? | ✅ | Menu printed in `Program.cs` before every input prompt |
| M12 | Can a user add an income item through the CLI? | ✅ | `AddItem(collection, ItemType.Income)` — menu option 2 |
| M13 | Can a user add an expense item through the CLI? | ✅ | `AddItem(collection, ItemType.Expense)` — menu option 3 |
| M14 | Can a user edit an existing item (at least one field)? | ✅ | `EditItem()` re-prompts all four fields; Enter keeps current value |
| M15 | Can a user remove an existing item? | ✅ | `RemoveItem()` — menu option 7; removes by `Guid` |
| M16 | Can a user quit the application via a menu option? | ✅ | `case "0": return;` in menu loop |

### Persistence

| # | Question | Status | Evidence |
|---|----------|--------|----------|
| M17 | Is the item list saved to a file (format and location documented in README)? | ✅ | `JsonPersistence.Save()` — menu option 8; README Data file section |
| M18 | Is the saved state restored when the application is restarted? | ✅ | Auto-load via `JsonPersistence.Load()` at startup in `Program.cs` |
| M19 | Does the application handle a missing data file without crashing? | ✅ | `Load()` returns `[]` when `!File.Exists(path)`; unit test `Load_MissingFile_ReturnsEmptyList` |
| M20 | Does the application handle a malformed data file explicitly? | ✅ | `Load()` catches `JsonException`, writes to `Console.Error`, returns `[]`; unit test `Load_MalformedJson_ReturnsEmptyList` |

---

## Quality and verification

| # | Question | Status | Evidence |
|---|----------|--------|----------|
| Q1 | Does the solution build with zero errors and zero suppressed warnings? | ✅ | `dotnet build` — no errors, no warnings (static analysis confirmed) |
| Q2 | Are invalid amounts (non-numeric, negative if disallowed) handled without crashing? | ✅ | `PromptDecimal()` loops on `!TryParse` or `value <= 0` |
| Q3 | Are invalid month values handled without crashing? | ✅ | `PromptMonth()` loops on `!TryParse` or value outside 1–12 |
| Q4 | Are invalid menu choices handled without crashing? | ✅ | `default: Console.WriteLine("Invalid choice…")` in switch |
| Q5 | Are sorting and filtering behaviors covered by at least one deterministic unit test each? | ✅ | `ItemCollectionTests.cs` — 10 tests covering all 3 sort fields × 2 directions, both filter types, and mutation guard |
| Q6 | Is the save/load round trip covered by a test using an isolated temporary file? | ✅ | `PersistenceTests.SaveAndLoad_RoundTrip_RestoresAllFields` uses `Path.GetTempFileName()` |
| Q7 | Is domain logic (sorting, filtering, model) kept in classes separate from console I/O and file I/O? | ✅ | `Domain/` and `Services/` contain no `Console` or `File` calls except `JsonPersistence` |
| Q8 | Are setup, build, run, and data-file location instructions present in README? | ✅ | README contains Requirements, Build, Run, Run tests, and Data file sections |

---

## Optional features

These are not required for acceptance but add value if present. **Each must not break any mandatory behavior.**

**Key:** ✅ Done &nbsp;|&nbsp; 🔲 Not yet implemented &nbsp;|&nbsp; ⚠️ Implementation constraint noted

| # | Question | Status | Evidence | Implementation constraint |
|---|----------|--------|----------|--------------------------|
| O1 | Does the application display a running total or balance (income minus expenses)? | ✅ | Summary line below every list — `Balance: +X.XX` | Uses `decimal` arithmetic over passed-in list; empty list returns `0m` cleanly |
| O2 | Does the application support totals broken down by type? | ✅ | Summary line shows `Income: X.XX` and `Expenses: X.XX` alongside balance | Reflects filtered/sorted view — summary follows whichever list is displayed |
| O3 | Does the application support a text search or keyword filter? | 🔲 | Console — items matching search term shown | Must be a **new** menu option; must not replace or disable existing type-filter (M9/M10) |
| O4 | Does the application use colored console output to distinguish income from expense? | ✅ | Income rows green, expense rows red; `Console.ResetColor()` called after every row | Color reset per-row prevents terminal bleed on exceptions |
| O5 | Does the application support CSV export? | 🔲 | `moneyitems.csv` present after export; file name in README | Must use a distinct file name — must not overwrite `moneyitems.json` |
| O6 | Does the application support pagination for large lists? | ⚠️ | Console — items shown in pages with navigation | `PrintList` is shared with edit/remove index display; introduce a separate `PrintListPaged` function |
| O7 | Does the application include unit tests beyond the mandatory behaviors listed in Q5–Q6? | ✅ | `ItemCollectionTests.cs` (10 tests) beyond the 3 in `PersistenceTests.cs` | Follow existing xunit pattern; isolated temp files for I/O |

### Conflicts and constraints in optional features

| # | Conflict | Resolution required before implementation |
|---|----------|------------------------------------------|
| C1 | O6 pagination changes `PrintList` — shared with edit/remove index display | Introduce `PrintListPaged`; keep `PrintList` for edit/remove contexts unchanged |
| C2 | O5 CSV export writes a file — could conflict with `moneyitems.json` name | Use `moneyitems.csv`; document in README |
| C3 | O3 keyword search could be confused with type-filter (menu option 5) | Assign a new menu number; make the distinction explicit in the prompt text |