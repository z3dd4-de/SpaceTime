using Godot;
using System;
using System.Collections.Generic;

public partial class PlanetData : Node
{
    public string[] YesNo = ["Yes", "No"];
    
    public class PlanetInfo
    {
        public string Name { get; set; }
        public int Inhabitants { get; set; }
        public float DiameterKm { get; set; }
        public float DistanceToParentKm { get; set; }
        public float OrbitPeriodDays { get; set; }
        public float DayLengthHours { get; set; }
        public int Moons { get; set; }

        public string Colonized { get; set; }
        public string Mining { get; set; }
        public bool IsMoon { get; set; }
        public bool IsPlanet { get; set; }
        public string ParentName { get; set; }
    }

    private static Dictionary<string, PlanetInfo> planets = new();

    public override void _Ready()
    {
        LoadPlanetData("res://Data/planets.json");
    }

    public bool GetYesNoAsBool(string value)
    {
        if (value == "Yes") return true;
        else if (value == "No") return false;
        else return false;
    }

    private void LoadPlanetData(string filePath)
    {
        if (!FileAccess.FileExists(filePath))
        {
            GD.PrintErr($"PlanetData: Datei {filePath} nicht gefunden!");
            return;
        }

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        var jsonText = file.GetAsText();

        // JSON-String zu Godot.JsonNode parsen
        var json = new Json();
        var error = json.Parse(jsonText);
        if (error != Error.Ok)
        {
            GD.PrintErr($"PlanetData: Fehler beim Parsen ({error})");
            return;
        }

        var root = json.Data.AsGodotDictionary();

        planets.Clear();
        foreach (string key in root.Keys)
        {
            var planetDict = (Godot.Collections.Dictionary<string, Variant>)root[key];
            var info = new PlanetInfo
            {
                Name = planetDict.GetValueOrDefault("Name", key).AsString(),
                DiameterKm = planetDict.GetValueOrDefault("DiameterKm", 0).AsSingle(),
                DistanceToParentKm = planetDict.GetValueOrDefault("DistanceToParentKm", 0).AsSingle(),
                OrbitPeriodDays = planetDict.GetValueOrDefault("OrbitPeriodDays", 0).AsSingle(),
                DayLengthHours = planetDict.GetValueOrDefault("DayLengthHours", 0).AsSingle(),
                Moons = planetDict.GetValueOrDefault("Moons", 0).AsInt32(),
                Colonized = planetDict.GetValueOrDefault("Colonie", YesNo[1]).AsString(),
                Mining = planetDict.GetValueOrDefault("Mining", YesNo[1]).AsString(),
                IsMoon = GetYesNoAsBool(planetDict.GetValueOrDefault("IsMoon", YesNo[1]).AsString()),
                IsPlanet = GetYesNoAsBool(planetDict.GetValueOrDefault("IsPlanet", YesNo[1]).AsString()),
                ParentName = planetDict.GetValueOrDefault("Parent", "Sun").AsString()
            };
            planets[key] = info;
        }

        GD.Print($"PlanetData: {planets.Count} Planeten und Monde geladen.");
    }

    // --- Öffentliche Zugriffsmethoden ---
    public static PlanetInfo GetPlanet(string name)
    {
        if (planets.TryGetValue(name, out var planet))
            return planet;

        GD.PrintErr($"Planet oder Mond '{name}' nicht gefunden!");
        return null;
    }

    public static IEnumerable<PlanetInfo> GetAllPlanets() => planets.Values;
}

