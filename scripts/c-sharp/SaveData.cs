using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class SaveSlot
{
    [JsonPropertyName("PlayerName")]
    public string PlayerName { get; set; } = "Name";

    [JsonPropertyName("Sex")]
    public string Sex { get; set; } = "Male";

    [JsonPropertyName("PlayerClass")]
    public string PlayerClass { get; set; } = "Worker";

    [JsonPropertyName("Age")]
    public int Age { get; set; } = 20;

    [JsonPropertyName("Year")]
    public int Year { get; set; } = 2500;

    [JsonPropertyName("Month")]
    public int Month { get; set; } = 1;

    [JsonPropertyName("Round")]
    public int Round { get; set; } = 1;

    [JsonPropertyName("Credits")]
    public int Credits { get; set; } = 1000;

    [JsonPropertyName("XP")]
    public int XP { get; set; } = 0;

    [JsonPropertyName("TotalXP")]
    public int TotalXP { get; set; } = 0;

    [JsonPropertyName("Skills")]
    public Dictionary<string, int> Skills { get; set; } = new();

    [JsonPropertyName("CurrentScene")]
    public string CurrentScene { get; set; } = "Game.tscn";

    public override string ToString()
    {
        return $"{PlayerName}, {PlayerClass}, Credits={Credits}, Scene={CurrentScene}";
    }
}
