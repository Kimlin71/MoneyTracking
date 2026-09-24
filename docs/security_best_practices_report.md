# Security Best Practices Report — MoneyTracking

**Date:** 2026-09-23 (updated 2026-09-23 — SEC-1, SEC-2, SEC-3, SEC-4 fixed)  
**Language:** C# (.NET 10, console application)  
**Scope:** All source files in the workspace

---

## Executive Summary

This is a local-only C# console application with no network exposure, no authentication layer, and no web framework. The attack surface is therefore narrow: file-system access, user input, and data deserialization. The codebase already follows several good practices (atomic writes, input validation loops, CSV escaping, nullable-enabled). Three actionable findings are identified below.

---

## Findings by Severity

### HIGH

#### SEC-1 — Data file written to the application binary directory (world-readable on multi-user systems) ✅ FIXED

**Impact:** Any local user can read or tamper with the saved financial data.

**Location:** [Program.cs](Program.cs#L5)

**Fix applied in `Program.cs`:** Data file is now written to `LocalApplicationData/MoneyTracking/moneyitems.json` — a directory owned exclusively by the current user. After each save, `File.SetUnixFileMode` sets permissions to `600` (owner read+write only) on macOS and Linux. The call is guarded by `OperatingSystem.IsWindows()` so it compiles and runs on all platforms.

```csharp
string DataDir  = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoneyTracking");
Directory.CreateDirectory(DataDir);
string DataFile = Path.Combine(DataDir, "moneyitems.json");
// ...
if (!OperatingSystem.IsWindows())
    File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
```

**Data file locations by OS:**
- macOS: `~/Library/Application Support/MoneyTracking/moneyitems.json`
- Linux: `~/.local/share/MoneyTracking/moneyitems.json`
- Windows: `%LOCALAPPDATA%\MoneyTracking\moneyitems.json`

---

#### SEC-2 — CSV export path is accepted without any sanitisation or boundary check ✅ FIXED

**Impact:** A crafted path (e.g. `../../.ssh/authorized_keys`) can overwrite arbitrary files the process owner can write to.

**Location:** [Program.cs](Program.cs#L153) and [Services/CsvExport.cs](Services/CsvExport.cs#L9)

**Fix applied in `Program.cs` `ExportToCsv()`:** `Path.GetFullPath` resolves the user-supplied path, then a `StartsWith` check against `Environment.CurrentDirectory` rejects any path outside the working directory before the file is opened. The guard logic is extracted into `CsvExport.IsPathSafe(string, string)` so it can be unit-tested independently of the console.

**Tests added in `MoneyTracking.Tests/CsvExportTests.cs`:** `IsPathSafe_PathInsideBaseDir_ReturnsTrue`, `IsPathSafe_PathTraversal_ReturnsFalse`, `IsPathSafe_ExactlyBaseDir_ReturnsTrue`, `IsPathSafe_AbsolutePathOutsideDir_ReturnsFalse`.

```csharp
string fullPath = Path.GetFullPath(path);
string safeDir  = Path.GetFullPath(Environment.CurrentDirectory);
if (!fullPath.StartsWith(safeDir + Path.DirectorySeparatorChar, StringComparison.Ordinal)
    && fullPath != safeDir)
{
    Console.Error.WriteLine("Export path must be inside the current directory.");
    return;
}
```

---

### MEDIUM

#### SEC-3 — `JsonSerializer.Deserialize` runs without a `JsonSerializerOptions` max-depth guard ✅ FIXED

**Impact:** A maliciously crafted or accidentally deeply-nested JSON file can cause a stack overflow, crashing the process.

**Location:** [Services/JsonPersistence.cs](Services/JsonPersistence.cs#L34)

**Fix applied in `Services/JsonPersistence.cs`:** A dedicated `_readOptions` with `MaxDepth = 8` is now passed to every `Deserialize` call. `MoneyItem` is a flat record; 8 levels is more than sufficient and tighter than the library default of 64.

**Test added in `MoneyTracking.Tests/PersistenceTests.cs`:** `Load_DeeplyNestedJson_ReturnsEmptyList` — writes a 20-level nested JSON array to a temp file and asserts the result is an empty list (no crash).

```csharp
private static readonly JsonSerializerOptions _readOptions = new() { MaxDepth = 8 };
// ...
return JsonSerializer.Deserialize<List<MoneyItem>>(json, _readOptions) ?? [];
```

---

### LOW / INFORMATIONAL

#### SEC-4 — Temporary file left on disk if `File.Move` throws ✅ FIXED

**Location:** [Services/JsonPersistence.cs](Services/JsonPersistence.cs#L17-L21)

**Fix applied in `Services/JsonPersistence.cs` `Save()`:** The write and move are now wrapped in a try/catch that deletes the `.tmp` file before re-throwing, so no orphaned file containing financial data is ever left behind.

**Test added in `MoneyTracking.Tests/PersistenceTests.cs`:** `Save_WhenMoveSucceeds_NoTmpFileRemains` — asserts the `.tmp` file is absent after a successful save.

```csharp
string tmp = path + ".tmp";
try
{
    File.WriteAllText(tmp, JsonSerializer.Serialize(items, _options));
    File.Move(tmp, path, overwrite: true);
}
catch
{
    if (File.Exists(tmp)) File.Delete(tmp);
    throw;
}
```

#### SEC-5 — No upper bound on user-supplied title length

**Location:** [Program.cs](Program.cs#L215) (`PromptNonEmpty`)

Long strings (thousands of characters) are accepted as a title without any upper-bound validation. For a local console app this is low risk (no buffer overflow in C#), but it can cause display formatting issues and grow the JSON data file unexpectedly.

**Recommendation:** Add an optional max-length check in `PromptNonEmpty`, or add a specific `PromptTitle` helper:

```csharp
if (value.Length > 100)
{
    Console.WriteLine("Title must be 100 characters or fewer.");
    continue;
}
```

---

## Summary Table

| ID    | Severity    | Finding                                         | Status         |
|-------|-------------|--------------------------------------------------|----------------|
| SEC-1 | HIGH        | Data file written to world-readable binary dir  | ✅ Fixed       |
| SEC-2 | HIGH        | CSV export path allows path traversal           | ✅ Fixed + tested |
| SEC-3 | MEDIUM      | JSON deserialisation has no explicit MaxDepth   | ✅ Fixed + tested |
| SEC-4 | LOW         | Orphaned `.tmp` file on failed atomic save      | ✅ Fixed + tested |
| SEC-5 | LOW         | No upper bound on title length                  | Open           |

---

*Report written by GitHub Copilot security analysis. Offer to fix any finding on request.*
