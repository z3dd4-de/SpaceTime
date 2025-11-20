// MarketPlace.cs - Planet oder Station mit Handelssystem
using Godot;
using System;
using System.Collections.Generic;

public partial class MarketPlace : Node
{
    [Export] public string MarketName { get; set; } = "Unbekannter Markt";
    
    public MarketInventory Inventory { get; private set; } = new();
    private Dictionary<string, Resource> availableResources = new();
    
    public override void _Ready()
    {
        GD.Print($"MarketPlace '{MarketName}' initialisiert");
        base._Ready();
    
        // Test-Daten hinzufügen
        CallDeferred(nameof(InitializeTestData));
    }

    private void InitializeTestData()
    {
        var tradingSystem = TradingSystem.Instance;
        if (tradingSystem == null) return;
        
        tradingSystem.RegisterMarket(this);
        
        // Stock hinzufügen
        Inventory.AddStock("oxygen", 500);
        Inventory.SetDemand("oxygen", 100);
        
        Inventory.AddStock("water", 300);
        Inventory.SetDemand("water", 150);
        
        Inventory.AddStock("iron", 200);
        Inventory.SetDemand("iron", 50);
    }
    
    public void RegisterResource(Resource resource)
    {
        if (!availableResources.ContainsKey(resource.Id))
        {
            availableResources[resource.Id] = resource;
            GD.Print($"Resource '{resource.Name}' zu Markt '{MarketName}' hinzugefügt");
        }
    }
    
    public int CalculatePrice(string resourceId)
    {
        if (!availableResources.ContainsKey(resourceId))
            return 0;
        
        Resource resource = availableResources[resourceId];
        int stock = Inventory.GetStock(resourceId);
        int demand = Inventory.GetDemand(resourceId);
        
        // Einfache Preisformel: BasePrice * (Nachfrage / Angebot)
        // Je weniger Stock, desto höher der Preis
        float multiplier = 1.0f;
        
        if (stock > 0)
        {
            multiplier = (float)demand / stock;
            multiplier = Mathf.Clamp(multiplier, 0.5f, 3.0f); // Preis zwischen 50% und 300%
        }
        else
        {
            multiplier = 3.0f; // Maximaler Preis wenn ausverkauft
        }
        
        return (int)(resource.BasePrice * multiplier);
    }
    
    public bool CanBuy(string resourceId, int amount)
    {
        return Inventory.GetStock(resourceId) >= amount;
    }
    
    public bool Buy(string resourceId, int amount, out int totalPrice)
    {
        totalPrice = 0;
        
        if (!CanBuy(resourceId, amount))
        {
            GD.Print($"Nicht genug {resourceId} auf Lager!");
            return false;
        }
        
        int pricePerUnit = CalculatePrice(resourceId);
        totalPrice = pricePerUnit * amount;
        
        Inventory.RemoveStock(resourceId, amount);
        GD.Print($"Gekauft: {amount}x {resourceId} für {totalPrice} Credits");
        
        return true;
    }
    
    public bool Sell(string resourceId, int amount, out int totalPrice)
    {
        totalPrice = 0;
        
        if (!availableResources.ContainsKey(resourceId))
        {
            GD.Print($"Resource {resourceId} wird hier nicht gehandelt!");
            return false;
        }
        
        int pricePerUnit = CalculatePrice(resourceId);
        totalPrice = pricePerUnit * amount;
        
        Inventory.AddStock(resourceId, amount);
        GD.Print($"Verkauft: {amount}x {resourceId} für {totalPrice} Credits");
        
        return true;
    }
    
    public void PrintMarketStatus()
    {
        GD.Print($"\n=== Markt: {MarketName} ===");
        foreach (var kvp in availableResources)
        {
            string id = kvp.Key;
            Resource res = kvp.Value;
            int stock = Inventory.GetStock(id);
            int demand = Inventory.GetDemand(id);
            int price = CalculatePrice(id);
            
            GD.Print($"{res.Name}: Lager={stock}, Nachfrage={demand}, Preis={price} Credits");
        }
        GD.Print("========================\n");
    }
}