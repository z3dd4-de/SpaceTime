using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using GFileAccess = Godot.FileAccess;

/// <summary>
/// Verwaltet alle Savegames (JSON) im Ordner user://save/
/// </summary>
public static class SaveManager
{
    private static readonly string SaveDirectory = "user://save/";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,           // besser lesbare JSON-Dateien
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true
    };

    // ----------------------------
    // Hilfsfunktionen
    // ----------------------------

    /// <summary>
    /// Stellt sicher, dass der Save-Ordner existiert.
    /// </summary>
    private static void EnsureSaveDirectory()
    {
        string absPath = ProjectSettings.GlobalizePath(SaveDirectory);
        if (!DirAccess.DirExistsAbsolute(absPath))
        {
            DirAccess.MakeDirRecursiveAbsolute(absPath);
            GD.Print($"[SaveManager] Save-Ordner erstellt: {absPath}");
        }
    }

    /// <summary>
    /// Gibt den vollständigen (virtuellen) Pfad eines Save-Slots zurück.
    /// </summary>
    private static string GetSlotPath(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
            slotName = "slot1";
        return $"{SaveDirectory}{slotName}.json";
    }

    // ----------------------------
    // Speichern
    // ----------------------------
    public static void SaveSlot(string slotName, SaveSlot slot)
    {
        try
        {
            EnsureSaveDirectory();

            string path = GetSlotPath(slotName);
            string json = JsonSerializer.Serialize(slot, JsonOptions);

            using var file = GFileAccess.Open(path, GFileAccess.ModeFlags.Write);
            file.StoreString(json);
            file.Close();

            GD.Print($"[SaveManager] Slot '{slotName}' gespeichert -> {path}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveManager] Fehler beim Speichern von Slot '{slotName}': {ex.Message}");
        }
    }

    // ----------------------------
    // Laden
    // ----------------------------
    public static SaveSlot LoadSlot(string slotName)
    {
        try
        {
            string path = GetSlotPath(slotName);

            if (!GFileAccess.FileExists(path))
            {
                GD.Print($"[SaveManager] Slot '{slotName}' existiert nicht.");
                return null;
            }

            using var file = GFileAccess.Open(path, GFileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            file.Close();

            if (string.IsNullOrWhiteSpace(json))
            {
                GD.PrintErr($"[SaveManager] Slot '{slotName}' ist leer oder beschädigt.");
                return null;
            }

            var slot = JsonSerializer.Deserialize<SaveSlot>(json, JsonOptions);
            GD.Print($"[SaveManager] Slot '{slotName}' erfolgreich geladen.");
            return slot;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveManager] Fehler beim Laden von Slot '{slotName}': {ex.Message}");
            return null;
        }
    }

    // ----------------------------
    // Slots auflisten
    // ----------------------------
    public static List<string> GetAvailableSlots()
    {
        EnsureSaveDirectory();

        List<string> slots = new();
        using var dir = DirAccess.Open(SaveDirectory);
        if (dir == null)
        {
            GD.PrintErr($"[SaveManager] Konnte Save-Ordner nicht öffnen: {SaveDirectory}");
            return slots;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
            {
                string slotName = fileName[..^5]; // entfernt ".json"
                slots.Add(slotName);
            }
            fileName = dir.GetNext();
        }

        dir.ListDirEnd();

        slots.Sort(StringComparer.OrdinalIgnoreCase);
        GD.Print($"[SaveManager] {slots.Count} Slots gefunden.");
        return slots;
    }

    // Alias für alte Funktion (Kompatibilität mit SaveMenu)
    public static List<string> ListSlots() => GetAvailableSlots();

    // ----------------------------
    // Slot löschen
    // ----------------------------
    public static void DeleteSlot(string slotName)
    {
        string vpath = GetSlotPath(slotName);
        string absolutePath = ProjectSettings.GlobalizePath(vpath);

        if (File.Exists(absolutePath))
        {
            try
            {
                File.Delete(absolutePath);
                GD.Print($"[SaveManager] Slot '{slotName}' gelöscht -> {absolutePath}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SaveManager] Fehler beim Löschen von Slot '{slotName}': {ex.Message}");
            }
        }
        else
        {
            GD.Print($"[SaveManager] Slot '{slotName}' nicht gefunden – nichts gelöscht.");
        }
    }
}
