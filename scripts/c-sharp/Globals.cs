using Godot;
using System;
using System.Text.Json.Serialization;

public static class Globals
{
    // ----------------------------
    // Enums
    // ----------------------------
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Sex
    {
        CIS_MALE,
        CIS_FEMALE,
        TRANS_MALE,
        TRANS_FEMALE,
        MALE_GAY,
        FEMALE_GAY,
        DIVERS
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PlayerClass
    {
        MINER,
        WORKER,
        SCIENTIST,
        CORPORATE
    }

    // ----------------------------
    // Globale Referenzen
    // ----------------------------
    public static Player Player { get; set; } = new Player();
    public static GameState GameState => GameState.Instance;

    // Beispielwerte (werden im GameState überschrieben)
    public static int StartingCredits { get; set; } = 1000;
    public static int StartingYear { get; set; } = 2500;

    // ----------------------------
    // Hilfsfunktionen
    // ----------------------------
    public static Sex ParseSex(string value)
    {
        if (Enum.TryParse(value, true, out Sex result))
            return result;
        return Sex.CIS_MALE;
    }

    public static PlayerClass ParseClass(string value)
    {
        if (Enum.TryParse(value, true, out PlayerClass result))
            return result;
        return PlayerClass.WORKER;
    }
}
