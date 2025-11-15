// MarketInventory.cs - Inventar eines Planeten/Station
using Godot;
using System;
using System.Collections.Generic;

public partial class MarketInventory : GodotObject
{
    public Dictionary<string, int> Stock { get; set; } = new();
    public Dictionary<string, int> Demand { get; set; } = new();
    
    public void AddStock(string resourceId, int amount)
    {
        if (Stock.ContainsKey(resourceId))
            Stock[resourceId] += amount;
        else
            Stock[resourceId] = amount;
    }
    
    public void RemoveStock(string resourceId, int amount)
    {
        if (Stock.ContainsKey(resourceId))
        {
            Stock[resourceId] = Math.Max(0, Stock[resourceId] - amount);
        }
    }
    
    public int GetStock(string resourceId)
    {
        return Stock.ContainsKey(resourceId) ? Stock[resourceId] : 0;
    }
    
    public void SetDemand(string resourceId, int demand)
    {
        Demand[resourceId] = demand;
    }
    
    public int GetDemand(string resourceId)
    {
        return Demand.ContainsKey(resourceId) ? Demand[resourceId] : 0;
    }
}
