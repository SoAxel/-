using System;
using System.Windows;
using System.IO;
using System.Text.Json;

public static class Storage
{
    private const string FileName = "sklad.json";

    public static void Save(Sklad sklad)
    {
        var json = JsonSerializer.Serialize(sklad, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FileName, json);
    }

    public static Sklad Load()
    {
        if (!File.Exists(FileName)) return new Sklad();
        var json = File.ReadAllText(FileName);
        return JsonSerializer.Deserialize<Sklad>(json) ?? new Sklad();
    }
}
