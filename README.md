# 💰 Money Tracking

> A C# (.NET 10) console application for tracking personal income and expenses — with color output, keyword search, CSV export, JSON persistence, and 47 unit tests.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![Tests](https://img.shields.io/badge/tests-47%20passed-brightgreen)
![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)
![License](https://img.shields.io/badge/license-MIT-blue)
![Status](https://img.shields.io/badge/status-active-success)

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Demo](#demo)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Testing](#testing)
- [Data File](#data-file)
- [Troubleshooting](#troubleshooting)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

Money Tracking is a command-line application that lets you record, view, sort, filter, and search income and expense items. Each item has a title, monetary amount, month, and type (income or expense).

**Problem solved:** keeping a clear, structured record of monthly income and expenses without a spreadsheet or external service.

**Built for:** student presentations covering OOP, LINQ, immutable domain records, JSON persistence, and console UI patterns in C#.

---

## Features

### Core features
| Feature | Detail |
|---------|--------|
| Add items | Prompted input with validation; accepts `.` or `,` as decimal separator |
| Edit items | Re-prompt title, amount, and month; press Enter to keep current value |
| Remove items | Select by 1-based display index; confirmed by stable `Guid` identity |
| Sort | By month, amount, or title — ascending or descending |
| Filter | Show only income or only expenses, with optional month restriction |
| Search | Case-insensitive substring match on title |
| Export | Write all items to a CSV file; path must be inside the current directory |
| Save | Atomic write to `moneyitems.json` (temp file + move, no data corruption) |
| Discard | Reload last saved state with confirmation prompt |

### Display features
- Live balance header before every menu (green when positive, red when negative)
- Color-coded rows — 🟢 green for income, 🔴 red for expense
- Summary line after every list: income total, expenses total, net balance

---

## Demo

```
=== Money Tracker ===
You currently have +59661,00 kr on your account.

Pick an option:
1. Show items (All / Expenses / Incomes)
2. Add New Expense / Income
3. Edit Item (edit, remove)
4. Sort items by month, amount, or title
5. Filter by type and optional month
6. Search by title keyword
7. Discard unsaved changes
8. Export to CSV
0. Save and Quit
>>
```

```
#    Month  Title                    Amount  Type
------------------------------------------------------
1    1      Salary                 32000,00  Income      ← green
2    1      Rent                    9500,00  Expense     ← red
...
------------------------------------------------------
Income:    74700,00  Expenses:   15039,00  Balance: +59661,00
```

---

## Prerequisites

| Requirement | Version |
|-------------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| OS | macOS, Windows, or Linux |

No internet access, database, or external service is required.

---

## Getting Started

### 1. Clone

```bash
git clone https://github.com/Kimlin71/MoneyTracking.git
cd MoneyTracking
```

### 2. Build

```bash
dotnet build
```

A successful build produces zero errors.

### 3. Run

```bash
dotnet run
```

The app loads `moneyitems.json` from the working directory. Ten sample items are included for demo purposes. If the file is missing the app starts with an empty list.

### 4. Run tests

```bash
dotnet test
```

Expected: **47 passed, 0 failed** (verified 2026-09-23 by `dotnet test`).

---

## Usage

### Adding an item

```
>> 2
Add: 1=Income  2=Expense
1
Title: Salary
Amount: 32000
Month (1-12): 1
Added. (Choose '0. Save and Quit' to save your changes.)
```

### Searching

```
>> 6
Search title: sal
```
Returns all items whose title contains "sal" (case-insensitive).

### Editing

```
>> 3
1=Edit  2=Remove
1
# ...list shown...
Edit #: 2
Title [Rent]: Company office rent
Amount [9500,00]:          ← press Enter to keep
Month [1]: 2
Updated. (Choose '0. Save and Quit' to save your changes.)
```

### Saving and quitting

```
>> 0
Saved. Goodbye!
```

---

## Project Structure

```
MoneyTracking/
├── Domain/
│   ├── ItemType.cs          # enum Income | Expense
│   ├── MoneyItem.cs         # immutable record: Id, Title, Amount, Month, Type
│   └── SortField.cs         # enum Month | Amount | Title
├── Services/
│   ├── CsvExport.cs         # Export to CSV; IsPathSafe path-traversal guard
│   ├── ItemCollection.cs    # Add, Remove, Replace, GetAll, GetSorted, GetFiltered, GetByKeyword
│   └── JsonPersistence.cs   # Save (atomic + tmp cleanup), Load (MaxDepth 8, safe on missing/malformed)
├── MoneyTracking.Tests/
│   ├── CsvExportTests.cs        # export format + IsPathSafe — 8 tests
│   ├── DecimalParsingTests.cs   # comma/dot separator — 9 tests
│   ├── ItemCollectionTests.cs   # sort and filter — 17 tests
│   ├── KeywordSearchTests.cs    # GetByKeyword — 8 tests
│   └── PersistenceTests.cs      # round-trip, missing file, malformed JSON, MaxDepth, tmp cleanup — 5 tests
├── Program.cs               # menu loop, console interaction, sub-menus
├── moneyitems.json          # persisted data (auto-loaded; auto-created on Save and Quit)
├── MoneyTracking.csproj
└── docs/
    ├── architecture.md
    ├── acceptance-checklist.md
    ├── workflow-log.md
    └── project-context.md
```

No class in `Domain/` or `Services/` references `System.Console` or `System.IO` (except `JsonPersistence`). `Program.cs` owns all console interaction.

---

## Architecture

```
┌─────────────┐     reads/writes     ┌──────────────────┐
│  Program.cs │ ──────────────────▶  │  ItemCollection  │
│ (console UI)│                      │  (in-memory list)│
└─────────────┘                      └──────────────────┘
       │                                      │
       │ calls                                │ domain types
       ▼                                      ▼
┌───────────────────┐              ┌──────────────────────┐
│  JsonPersistence  │              │  MoneyItem (record)  │
│  (file I/O only)  │              │  ItemType (enum)     │
└───────────────────┘              └──────────────────────┘
```

**Key design decisions:**
- `MoneyItem` is an immutable C# record — editing produces a new instance via `with`.
- Items are selected for edit/remove by display index, backed by a stable `Guid` ID.
- Save is explicit (option 0) — no accidental overwrites.
- `GetByKeyword` uses `StringComparison.OrdinalIgnoreCase` — locale-safe, no regex.

---

## Testing

Tests use **xUnit** and run against the domain and service layers only (no console or file I/O mocking needed in most cases).

```bash
dotnet test --logger "console;verbosity=normal"
```

| Test class | Count | What it covers |
|------------|-------|----------------|
| `CsvExportTests` | 8 | Header + data row, empty list, comma/quote escaping, path-traversal guard (4 cases) |
| `DecimalParsingTests` | 9 | Dot, comma, whole number, Swedish locale, zero, negative, empty, non-numeric |
| `ItemCollectionTests` | 17 | Sort (3 fields × 2 directions + mutation guard), filter (income, expense, type+month) |
| `KeywordSearchTests` | 8 | Exact, case-insensitive, partial, no match, multiple matches, empty collection |
| `PersistenceTests` | 5 | Save/load round-trip, missing file, malformed JSON, deeply-nested JSON (MaxDepth), no orphaned tmp |
| **Total** | **47** | **All pass ✅ (verified 2026-09-23)** |

---

## Data File

| Property | Value |
|----------|-------|
| File name | `moneyitems.json` |
| Location (macOS) | `~/Library/Application Support/MoneyTracking/moneyitems.json` |
| Location (Linux) | `~/.local/share/MoneyTracking/moneyitems.json` |
| Location (Windows) | `%LOCALAPPDATA%\MoneyTracking\moneyitems.json` |
| Permissions | `600` (owner read+write only) on macOS/Linux — set after every save |
| Format | JSON array — one `MoneyItem` object per element |
| Created by | Menu option **0. Save and Quit** |
| Missing file | App starts with an empty list — not an error |
| Malformed file | Error message printed to stderr; app starts with an empty list |

**Atomic write:** the file is written to a `.tmp` file first, then moved into place — a crash mid-write never leaves a corrupt file. If the move fails, the `.tmp` file is deleted before the error is surfaced.

**CSV export path guard:** the export path is resolved to an absolute path and rejected if it falls outside the current working directory, preventing path-traversal overwrites.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| App starts empty after editing | Quit without choosing **0. Save and Quit** | Always exit via option 0 to persist changes |
| Amount not accepted | Typed thousands separator (e.g. `1 250`) | Enter numbers without spaces: `1250` or `1250,50` |
| Duplicate assembly attribute error | `dotnet run` from wrong directory | `cd` to the project folder before running |
| Tests fail to find `[Fact]` | `using Xunit;` missing | Add `using Xunit;` to the test file |

---

## Roadmap

| Status | Feature |
|--------|---------|
| ✅ Done | Core CRUD, sort, filter by type + month, search, JSON persistence, color UI |
| ✅ Done | CSV export with path-traversal guard |
| ✅ Done | Security hardening: user-specific data dir (600 perms), MaxDepth guard, atomic tmp cleanup, path guard |
| ⚠️ Constrained | Pagination (requires separate display function to preserve edit/remove indexes) |

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make the smallest coherent change
4. Run `dotnet build` (zero warnings) and `dotnet test` (all pass)
5. Open a pull request with a clear description

Please follow the coding rules in [`.github/copilot-instructions.md`](.github/copilot-instructions.md):
- One public type per file
- `decimal` for all monetary amounts
- Domain logic separate from console I/O

---

## License

This project is licensed under Kimlin71

---

## Project Status

🟢 **Active** — developed as a student project. All mandatory acceptance criteria are met. See [docs/acceptance-checklist.md](docs/acceptance-checklist.md) for the full verification record.
