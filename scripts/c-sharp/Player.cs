using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Node
{
    // öffentliche Properties, damit GameState / SaveManager einfach arbeiten können
    public Person Person { get; set; } = new Person();
    public Globals.PlayerClass Class { get; set; } = Globals.PlayerClass.WORKER;
    public int Level { get; set; } = 1;

    // Portrait als Texture im Laufzeit-Objekt (nicht direkt serialisiert)
    public Texture2D Image { get; set; } = null;

    // optional: Portrait/Spacecraft Pfadstrings für Save
    public string PortraitPath { get; set; } = "";
    public string SpacecraftPath { get; set; } = "";

    public Player() { Person = new Person(); }

    public void CreatePlayer(string name, Globals.Sex sex, Globals.PlayerClass playerClass)
    {
        Person = new Person(name, sex);
        Class = playerClass;
        Level = 1;
    }

    // Konvertiert diesen Player in einen SaveSlot (JSON-kompatibel)
    public SaveSlot ToSaveSlot()
    {
        var slot = new SaveSlot
        {
            PlayerName = Person?.Name ?? "Unknown",
            Sex = Person?.Sex.ToString() ?? Globals.Sex.CIS_MALE.ToString(),
            PlayerClass = Class.ToString(),
            Age = (int)Mathf.Round(Person?.Age ?? 20f),
            Year = GameState.Instance != null ? GameState.Instance.Year : 2500,
            Month = GameState.Instance != null ? GameState.Instance.Month : 1,
            Round = GameState.Instance != null ? GameState.Instance.Round : 1,
            Credits = GameState.Instance != null ? GameState.Instance.Credits : 1000,
            XP = GameState.Instance != null ? GameState.Instance.Experience : 0,
            TotalXP = GameState.Instance != null ? GameState.Instance.TotalExperience : 0,
            Skills = Person?.Genome?.Genes != null ? new Dictionary<string,int>(Person.Genome.Genes) : new Dictionary<string,int>(),
            CurrentScene = GameState.Instance != null ? GameState.Instance.LastScene : "res://scenes/game.tscn"
        };

        return slot;
    }

    // Erzeugt einen Player aus einem SaveSlot
    public static Player FromSaveSlot(SaveSlot slot)
    {
        var p = new Player();
        if (slot == null) return p;

        // Sex
        if (Enum.TryParse(typeof(Globals.Sex), slot.Sex, out var sexObj))
            p.Person.Sex = (Globals.Sex)sexObj;
        else
            p.Person.Sex = Globals.Sex.CIS_MALE;

        p.Person.Name = slot.PlayerName ?? "Unknown";
        p.Person.Age = slot.Age;
        p.Person.Birthdate = $"{slot.Year - 20}-01-01"; // grobe Schätzung

        // Genome / Skills in Player.Person.Genome
        var genome = new Genome();
        if (slot.Skills != null)
        {
            foreach (var kv in slot.Skills)
                genome.Genes[kv.Key] = kv.Value;
        }
        p.Person.Genome = genome;

        // Class
        if (Enum.TryParse(typeof(Globals.PlayerClass), slot.PlayerClass, out var classObj))
            p.Class = (Globals.PlayerClass)classObj;
        else
            p.Class = Globals.PlayerClass.WORKER;

        return p;
    }
}
