using System.Text.Json;
using MoneyTracking.Domain;

namespace MoneyTracking.Services;

// static means you never create an instance — just call JsonPersistence.Save(...) directly
public static class JsonPersistence
{
    // WriteIndented makes the JSON file human-readable (one field per line)
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    // MaxDepth 8 is generous for a flat MoneyItem list; explicit guard against crafted files
    private static readonly JsonSerializerOptions _readOptions = new() { MaxDepth = 8 };

    // Converts the list to JSON text and writes it to disk
    public static void Save(IReadOnlyList<MoneyItem> items, string path)
    {
        // Write to a temp file first so a crash mid-write never leaves a corrupt data file
        string tmp = path + ".tmp";
        try
        {
            File.WriteAllText(tmp, JsonSerializer.Serialize(items, _options));
            // overwrite: true replaces the old file atomically — the old file is never half-written
            File.Move(tmp, path, overwrite: true);
            // 600 = owner read+write only; ignored on Windows where ACLs control access instead
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        catch
        {
            if (File.Exists(tmp)) File.Delete(tmp);
            throw;
        }
    }

    // Reads the JSON file and returns the list of items; never throws — bad situations return []
    public static IReadOnlyList<MoneyItem> Load(string path)
    {
        // [] is the empty-list shorthand in C# 12; returning it means "no items, but no crash"
        if (!File.Exists(path))
            return [];

        try
        {
            string json = File.ReadAllText(path);
            // ?? [] means: if Deserialize returns null (empty file), use an empty list instead
            return JsonSerializer.Deserialize<List<MoneyItem>>(json, _readOptions) ?? [];
        }
        catch (JsonException ex)
        {
            // Console.Error is the standard error stream; it shows in red in most terminals
            Console.Error.WriteLine($"Data file is malformed and could not be loaded: {ex.Message}");
            return [];
        }
    }
}
