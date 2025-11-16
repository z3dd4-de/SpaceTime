// PlayerInventory.cs - ERWEITERT mit GetAllCargo
using Godot;
using System.Collections.Generic;

public partial class PlayerInventory : Node
{
    [Export] public int Credits { get; set; } = 10000;
    
    private Dictionary<string, int> cargo = new();
    
    public void AddCargo(string resourceId, int amount)
    {
        if (cargo.ContainsKey(resourceId))
            cargo[resourceId] += amount;
        else
            cargo[resourceId] = amount;
        
        GD.Print($"Cargo: +{amount} {resourceId}");
    }
    
    public bool RemoveCargo(string resourceId, int amount)
    {
        if (!cargo.ContainsKey(resourceId) || cargo[resourceId] < amount)
        {
            GD.Print($"Nicht genug {resourceId} im Inventar!");
            return false;
        }
        
        cargo[resourceId] -= amount;
        if (cargo[resourceId] == 0)
        {
            cargo.Remove(resourceId);
        }
        GD.Print($"Cargo: -{amount} {resourceId}");
        return true;
    }
    
    public int GetCargoAmount(string resourceId)
    {
        return cargo.ContainsKey(resourceId) ? cargo[resourceId] : 0;
    }
    
    public Dictionary<string, int> GetAllCargo()
    {
        return new Dictionary<string, int>(cargo);
    }
    
    public bool AddCredits(int amount)
    {
        Credits += amount;
        GD.Print($"Credits: +{amount} (Total: {Credits})");
        return true;
    }
    
    public bool RemoveCredits(int amount)
    {
        if (Credits < amount)
        {
            GD.Print("Nicht genug Credits!");
            return false;
        }
        
        Credits -= amount;
        GD.Print($"Credits: -{amount} (Total: {Credits})");
        return true;
    }
    
    public void PrintInventory()
    {
        GD.Print("\n=== Spieler Inventar ===");
        GD.Print($"Credits: {Credits}");
        GD.Print("Cargo:");
        foreach (var kvp in cargo)
        {
            GD.Print($"  {kvp.Key}: {kvp.Value}");
        }
        GD.Print("========================\n");
    }
}