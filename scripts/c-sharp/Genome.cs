using Godot;
using System;
using System.Collections.Generic;

public partial class Genome : Node
{
    // öffentliche, serialisierbare Struktur der Gene
    public Dictionary<string, int> Genes { get; set; } = new();

    public int Length => Genes?.Count ?? 0;

    public Genome()
    {
        // Default-Werte wie in deinem GDScript
        Genes = new Dictionary<string, int>
        {
            ["Health"] = 5,
            ["Size"] = 5,
            ["Intelligence"] = 5,
            ["MaxAge"] = 5,
            ["Class"] = 5
        };
    }

    public override void _Ready()
    {
    }

    // Beispiel: kombiniere mit Partner (noch sehr einfach)
    public Dictionary<string, int> Mate(Genome partner)
    {
        var result = new Dictionary<string, int>(Genes);
        if (partner == null) return result;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        foreach (var key in partner.Genes.Keys)
        {
            if (!result.ContainsKey(key))
            {
                result[key] = partner.Genes[key];
                continue;
            }

            // Simple mixing: pick randomly parent's value or average
            if (rng.RandiRange(0, 1) == 0)
                result[key] = Genes[key];
            else
                result[key] = partner.Genes[key];
        }

        return result;
    }

    // Mutation nach Gen-Key
    public int Mutate(string geneKey)
    {
        if (!Genes.ContainsKey(geneKey)) return 0;
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        int delta = rng.RandiRange(-1, 1);
        Genes[geneKey] = Mathf.Clamp(Genes[geneKey] + delta, 1, 10);
        return Genes[geneKey];
    }
}
