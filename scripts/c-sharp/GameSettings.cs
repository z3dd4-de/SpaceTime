using Godot;
using System;
using System.IO;
using System.Text.Json;

public class JsonData
{
    // Video Settings
    public bool FullScreen { get; set; }
    public bool Vsync { get; set; }
    public bool Borderless { get; set; }
    public string Resolution { get; set; }

    // Audio Volumes
    public double MasterVolume { get; set; }
    public double MusicVolume { get; set; }
    public double SfxVolume { get; set; }

    // Default Audio Volumes
    public double DefaultMasterVolume { get; set; }
    public double DefaultMusicVolume { get; set; }
    public double DefaultSfxVolume { get; set; }
}

public static class GameSettings
{
    private static readonly string JsonFile = "user://settings.json";
    private static JsonData data;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Lädt die Einstellungen aus user://settings.json
    /// oder erstellt sie mit Standardwerten, falls sie fehlt.
    /// </summary>
    public static JsonData Load()
    {
        try
        {
            string absPath = ProjectSettings.GlobalizePath(JsonFile);

            if (!File.Exists(absPath))
            {
                GD.Print($"[GameSettings] Keine Settings-Datei gefunden – Erstelle neue Default-Datei: {absPath}");
                data = CreateDefault();
                Save(); // direkt speichern
                return data;
            }

            string json = File.ReadAllText(absPath);
            
            // JSON enthält ggf. ein Objekt "Settings" → das berücksichtigen
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Settings", out JsonElement settingsElement))
            {
                data = JsonSerializer.Deserialize<JsonData>(settingsElement.GetRawText(), JsonOptions);
            }
            else
            {
                data = JsonSerializer.Deserialize<JsonData>(json, JsonOptions);
            }

            GD.Print($"[GameSettings] Einstellungen erfolgreich geladen aus {absPath}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[GameSettings] Fehler beim Laden: {ex.Message}");
            data = CreateDefault();
        }

        return data;
    }

    /// <summary>
    /// Speichert die aktuellen Einstellungen in user://settings.json
    /// </summary>
    public static void Save()
    {
        try
        {
            if (data == null)
            {
                GD.PrintErr("[GameSettings] Keine Daten vorhanden, erstelle Defaults vor dem Speichern.");
                data = CreateDefault();
            }

            string absPath = ProjectSettings.GlobalizePath(JsonFile);

            // Save unter "Settings": {...}
            string wrappedJson = JsonSerializer.Serialize(new { Settings = data }, JsonOptions);
            File.WriteAllText(absPath, wrappedJson);

            GD.Print($"[GameSettings] Einstellungen gespeichert nach {absPath}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[GameSettings] Fehler beim Speichern: {ex.Message}");
        }
    }

    /// <summary>
    /// Erstellt Standardwerte gemäß deiner Vorgabe.
    /// </summary>
    private static JsonData CreateDefault()
    {
        return new JsonData
        {
            FullScreen = true,
            Vsync = true,
            Borderless = false,
            Resolution = "1280x720",
            MasterVolume = 1.0d,
            MusicVolume = 1.0d,
            SfxVolume = 1.0d,
            DefaultMasterVolume = 1.0d,
            DefaultMusicVolume = 1.0d,
            DefaultSfxVolume = 1.0d
        };
    }

    /// <summary>
    /// Gibt die aktuellen Settings zurück.
    /// </summary>
    public static JsonData Get()
    {
        if (data == null)
            return Load();
        return data;
    }
}

