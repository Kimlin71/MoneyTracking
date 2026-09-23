# Security Best Practices Report — MoneyTracking

**Date:** 2026-09-23  
**Language:** C# (.NET 10, console application)  
**Scope:** All source files in the workspace

---

## Executive Summary

This is a local-only C# console application with no network exposure, no authentication layer, and no web framework. The attack surface is therefore narrow: file-system access, user input, and data deserialization. The codebase already follows several good practices (atomic writes, input validation loops, CSV escaping, nullable-enabled). Three actionable findings are identified below.

---

## Findings by Severity

### HIGH

#### SEC-1 — Data file written to the application binary directory (world-readable on multi-user systems)

**Impact:** Any local user can read or tamper with the saved financial data.

**Location:** [Program.cs](Program.cs#L5)

```csharp
string DataFile = Path.Combine(AppContext.BaseDirectory, "moneyitems.json");
```

`AppContext.BaseDirectory` resolves to the directory that contains the compiled executable (e.g. `bin/Debug/net10.0/`). On macOS and Linux, that directory is often world-readable (`755`). Storing personal financial data there means any other local account can read the file without further privileges.

**Recommendation:** Write the data file to a user-specific directory (e.g. `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)`) and set restrictive permissions (`600`) on the file after writing.

---

#### SEC-2 — CSV export path is accepted without any sanitisation or boundary check

**Impact:** A crafted path (e.g. `../../.ssh/authorized_keys`) can overwrite arbitrary files the process owner can write to.

**Location:** [Program.cs](Program.cs#L153) and [Services/CsvExport.cs](Services/CsvExport.cs#L9)

```csharp
// Program.cs ~line 153
string path = PromptNonEmpty("Export file path (e.g. export.csv): ");
// ...
CsvExport.Export(items, path);
```

`PromptNonEmpty` only checks that the string is non-empty. No validation is done to keep the output inside a safe directory. An attacker who can interact with the console (or replay a script) can supply a path traversal string.

**Recommendation:** Resolve the path with `Path.GetFullPath` and verify it stays within an expected base directory, or at minimum confirm with the user before writing to an absolute path outside the current directory. Example guard:

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

#### SEC-3 — `JsonSerializer.Deserialize` runs without a `JsonSerializerOptions` max-depth guard

**Impact:** A maliciously crafted or accidentally deeply-nested JSON file can cause a stack overflow, crashing the process.

**Location:** [Services/JsonPersistence.cs](Services/JsonPersistence.cs#L34)

```csharp
return JsonSerializer.Deserialize<List<MoneyItem>>(json) ?? [];
```

The default `System.Text.Json` `MaxDepth` is 64, which is fine for normal use. However, the deserialization happens with no explicit options, so it silently inherits the library default. If the default were ever changed (e.g. by a future .NET version), or if someone crafted a deeply-nested file, there is no explicit guard.

**Recommendation:** Pass explicit `JsonSerializerOptions` with a reasonable `MaxDepth`:

```csharp
private static readonly JsonSerializerOptions _readOptions = new()
{
    MaxDepth = 8  // MoneyItem has no nesting; 8 is more than sufficient
};
// ...
return JsonSerializer.Deserialize<List<MoneyItem>>(json, _readOptions) ?? [];
```

---

### LOW / INFORMATIONAL

#### SEC-4 — Temporary file left on disk if `File.Move` throws

**Location:** [Services/JsonPersistence.cs](Services/JsonPersistence.cs#L17-L21)

```csharp
string tmp = path + ".tmp";
string json = JsonSerializer.Serialize(items, _options);
File.WriteAllText(tmp, json);
File.Move(tmp, path, overwrite: true);
```

If `File.Move` throws (e.g. cross-device move, permissions error), the `.tmp` file containing the full data set is left on disk undeleted. This is not a data-loss risk (the move throwing means the old file is still intact), but the orphaned `.tmp` file contains sensitive financial data.

**Recommendation:** Wrap in a try/finally to clean up:

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

| ID    | Severity    | Finding                                         |
|-------|-------------|-------------------------------------------------|
| SEC-1 | HIGH        | Data file written to world-readable binary dir  |
| SEC-2 | HIGH        | CSV export path allows path traversal           |
| SEC-3 | MEDIUM      | JSON deserialisation has no explicit MaxDepth   |
| SEC-4 | LOW         | Orphaned `.tmp` file on failed atomic save      |
| SEC-5 | LOW         | No upper bound on title length                  |

---

*Report written by GitHub Copilot security analysis. Offer to fix any finding on request.*
