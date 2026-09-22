using System.Text.Json;
using MoneyTracking.Domain;

namespace MoneyTracking.Services;

// static means you never create an instance — just call JsonPersistence.Save(...) directly
public static class JsonPersistence
{
    // WriteIndented makes the JSON file human-readable (one field per line)
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    // Converts the list to JSON text and writes it to disk
    public static void Save(IReadOnlyList<MoneyItem> items, string path)
    {
        // Write to a temp file first so a crash mid-write never leaves a corrupt data file
        string tmp = path + ".tmp";
        string json = JsonSerializer.Serialize(items, _options);
        File.WriteAllText(tmp, json);
        // overwrite: true replaces the old file atomically — the old file is never half-written
        File.Move(tmp, path, overwrite: true);
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
            return JsonSerializer.Deserialize<List<MoneyItem>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            // Console.Error is the standard error stream; it shows in red in most terminals
            Console.Error.WriteLine($"Data file is malformed and could not be loaded: {ex.Message}");
            return [];
        }
    }
}
