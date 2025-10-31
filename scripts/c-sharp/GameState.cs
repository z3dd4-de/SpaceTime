using Godot;
using System;
using System.Collections.Generic;

public partial class GameState : Node
{
    public static GameState Instance { get; private set; }

    // Der aktuell geladene SaveSlot (JSON-kompatibel)
    public SaveSlot CurrentSave { get; private set; } = new();

    // Der laufende Player im Spiel (nicht JSON; ist runtime-Objekt)
    public Player Player { get; private set; } = new();

    // Metadaten
    public string CurrentSlotName { get; set; } = "slot1";
    public string LastScene { get; set; } = "res://scenes/game.tscn";

    // Spielwerte (früher in globals.gd)
    public int Credits { get; set; } = 1000;
    public int Experience { get; set; } = 0;
    public int TotalExperience { get; set; } = 0;
    public int Round { get; private set; } = 1;
    public int Month { get; private set; } = 1;
    public int Year { get; private set; } = 2500;

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        GD.Print("[GameState] Initialisiert.");
    }

    // ----------------------------
    // Neues Spiel anlegen
    // ----------------------------
    public void NewGame(string playerName, Globals.Sex sex, Globals.PlayerClass playerClass)
    {
        // Player runtime erstellen
        Player = new Player();
        Player.CreatePlayer(playerName, sex, playerClass);

        // Grundwerte setzen
        Credits = 1000;
        Experience = 0;
        TotalExperience = 0;
        Round = 1;
        Month = 1;
        Year = 2500;
        LastScene = "res://scenes/game.tscn";

        // SaveSlot initial befüllen (strings für JSON)
        CurrentSave = new SaveSlot
        {
            PlayerName = playerName,
            Sex = sex.ToString(),
            PlayerClass = playerClass.ToString(),
            Age = (int)Player.Person.Age,
            Year = Year,
            Month = Month,
            Round = Round,
            Credits = Credits,
            XP = Experience,
            TotalXP = TotalExperience,
            Skills = Player.Person?.Genome?.Genes != null ? new Dictionary<string, int>(Player.Person.Genome.Genes) : new Dictionary<string, int>(),
            CurrentScene = LastScene
        };

        SaveGame(); // optional erstes Speichern
        GD.Print($"[GameState] Neues Spiel: {playerName} ({playerClass})");
    }

    // ----------------------------
    // Laden eines SaveSlots
    // ----------------------------
    public bool LoadGame(string slotName)
    {
        var slot = SaveManager.LoadSlot(slotName);
        if (slot == null)
        {
            GD.PrintErr($"[GameState] Kein SaveSlot '{slotName}' gefunden.");
            return false;
        }

        CurrentSlotName = slotName;
        CurrentSave = slot;

        // Spielwerte übernehmen
        Credits = slot.Credits;
        Experience = slot.XP;
        TotalExperience = slot.TotalXP;
        Round = slot.Round;
        Month = slot.Month;
        Year = slot.Year;
        LastScene = slot.CurrentScene ?? LastScene;

        // Player runtime aus slot erstellen
        Player = Player.FromSaveSlot(slot);

        GD.Print($"[GameState] SaveSlot '{slotName}' geladen -> Player: {slot.PlayerName}");
        return true;
    }

    // ----------------------------
    // Speichern (synchronisiert CurrentSave mit Runtime)
    // ----------------------------
    public void SaveGame()
    {
        if (Player == null)
        {
            GD.PrintErr("[GameState] Kein Player vorhanden, Save abgebrochen.");
            return;
        }

        // Synchronisiere CurrentSave mit laufenden Werten
        CurrentSave ??= new SaveSlot();

        CurrentSave.PlayerName = Player.Person?.Name ?? "Unknown";
        CurrentSave.Sex = Player.Person?.Sex.ToString() ?? Globals.Sex.CIS_MALE.ToString();
        CurrentSave.PlayerClass = Player.Class.ToString();
        CurrentSave.Age = (int)Mathf.Round(Player.Person?.Age ?? 20f);

        CurrentSave.Year = Year;
        CurrentSave.Month = Month;
        CurrentSave.Round = Round;

        CurrentSave.Credits = Credits;
        CurrentSave.XP = Experience;
        CurrentSave.TotalXP = TotalExperience;

        CurrentSave.Skills = Player.Person?.Genome?.Genes != null
            ? new Dictionary<string, int>(Player.Person.Genome.Genes)
            : new Dictionary<string, int>();

        CurrentSave.CurrentScene = LastScene ?? CurrentSave.CurrentScene;

        SaveManager.SaveSlot(CurrentSlotName, CurrentSave);
        GD.Print($"[GameState] Gespeichert: {CurrentSlotName}");
    }

    // ----------------------------
    // Autosave (statisch aufrufbar)
    // ----------------------------
    public static void AutoSave()
    {
        if (Instance == null)
        {
            GD.PrintErr("[GameState] Instance == null, AutoSave übersprungen.");
            return;
        }

        Instance.SaveGame();
        GD.Print("[GameState] AutoSave durchgeführt.");
    }

    // ----------------------------
    // Hilfsfunktionen: Runden/Datum
    // ----------------------------
    public void IncRound()
    {
        Round++;
        IncDate();
        EmitSignal(nameof(NextRoundEventHandler));
    }

    private void IncDate()
    {
        Month++;
        if (Month > 12)
        {
            Month = 1;
            Year++;
        }
        GD.Print($"Current date: {Month}-{Year}");
    }

    public string GetDateInGame()
    {
        return $"{Year:D4}-{Month:D2}-01";
    }

    // ----------------------------
    // Signale (falls gewünscht)
    // ----------------------------
    [Signal] public delegate void NextRoundEventHandler();
}
