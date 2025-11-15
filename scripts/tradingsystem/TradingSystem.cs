// TradingSystem.cs - Zentrale Verwaltung des Handelssystems
using Godot;
using System.Collections.Generic;
using System.Text.Json;
//using static Godot.FileAccess;

public partial class TradingSystem : Node
{
    private static TradingSystem instance;
    public static TradingSystem Instance => instance;
    
    private Dictionary<string, Resource> resourceDatabase = new();
    private List<MarketPlace> markets = new();
    
    [Export] public string ResourceDataPath { get; set; } = "res://data/market_data.json";
    
    public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            QueueFree();
            return;
        }
        
        LoadResourceData();
    }
    
    public void LoadResourceData()
    {
        GD.Print("Lade Resource-Daten...");
        
        if (!Godot.FileAccess.FileExists(ResourceDataPath))
        {
            GD.PrintErr($"Datei nicht gefunden: {ResourceDataPath}");
            CreateDefaultResourceData();
            return;
        }
        
        using var file = Godot.FileAccess.Open(ResourceDataPath, Godot.FileAccess.ModeFlags.Read);
        string jsonText = file.GetAsText();
        
        try
        {
            var data = JsonSerializer.Deserialize<ResourceDataJson>(jsonText);
            
            if (data?.Resources != null)
            {
                foreach (var resData in data.Resources)
                {
                    var resource = new Resource(
                        resData.Id,
                        resData.Name,
                        resData.BasePrice,
                        resData.Category,
                        resData.Description
                    );
                    
                    resourceDatabase[resource.Id] = resource;
                    GD.Print($"Resource geladen: {resource.Name} (BasePrice: {resource.BasePrice})");
                }
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Fehler beim Laden der JSON: {ex.Message}");
        }
    }
    
    private void CreateDefaultResourceData()
    {
        GD.Print("Erstelle Standard-Resource-Daten...");
        
        var defaultData = new ResourceDataJson
        {
            Resources = new List<ResourceJson>
            {
                new ResourceJson { Id = "oxygen", Name = "Sauerstoff", BasePrice = 50, Category = "gas", Description = "Lebensnotwendiges Gas" },
                new ResourceJson { Id = "water", Name = "Wasser", BasePrice = 30, Category = "liquid", Description = "H2O für Kolonisten" },
                new ResourceJson { Id = "iron", Name = "Eisen", BasePrice = 100, Category = "metal", Description = "Grundlegendes Baumaterial" },
                new ResourceJson { Id = "food", Name = "Nahrung", BasePrice = 80, Category = "consumable", Description = "Lebensmittel" }
            }
        };
        
        string json = JsonSerializer.Serialize(defaultData, new JsonSerializerOptions { WriteIndented = true });
        
        // Erstelle Verzeichnis falls nicht vorhanden
        string dirPath = ResourceDataPath.GetBaseDir();
        if (!DirAccess.DirExistsAbsolute(dirPath))
        {
            DirAccess.MakeDirRecursiveAbsolute(dirPath);
        }
        
        using var file = Godot.FileAccess.Open(ResourceDataPath, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(json);
        
        GD.Print($"Standard-Daten gespeichert in: {ResourceDataPath}");
        
        // Lade die gerade erstellten Daten
        LoadResourceData();
    }
    
    public Resource GetResource(string id)
    {
        return resourceDatabase.ContainsKey(id) ? resourceDatabase[id] : null;
    }
    
    public void RegisterMarket(MarketPlace market)
    {
        if (!markets.Contains(market))
        {
            markets.Add(market);
            
            // Registriere alle bekannten Resources beim Markt
            foreach (var res in resourceDatabase.Values)
            {
                market.RegisterResource(res);
            }
            
            GD.Print($"Markt '{market.MarketName}' registriert");
        }
    }
    
    public List<MarketPlace> GetAllMarkets()
    {
        return markets;
    }
}

// JSON Datenstrukturen
public class ResourceJson
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int BasePrice { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
}

public class ResourceDataJson
{
    public List<ResourceJson> Resources { get; set; }
}