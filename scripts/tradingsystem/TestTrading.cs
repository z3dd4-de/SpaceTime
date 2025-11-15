// TestTrading.cs - Beispiel-Setup zum Testen
using Godot;

public partial class TestTrading : Node
{
    private TradingSystem tradingSystem;
    private PlayerInventory player;
    private MarketPlace planetA;
    private MarketPlace planetB;
    
    public override void _Ready()
    {
        GD.Print("=== Trading System Test ===\n");
        
        // Setup
        tradingSystem = GetNode<TradingSystem>("/root/TradingSystem");
        
        player = new PlayerInventory();
        AddChild(player);
        
        // Planet A - Bergbau-Planet (viel Eisen, wenig Nahrung)
        planetA = new MarketPlace { MarketName = "Planet Alpha (Bergbau)" };
        AddChild(planetA);
        tradingSystem.RegisterMarket(planetA);
        
        planetA.Inventory.AddStock("iron", 1000);
        planetA.Inventory.SetDemand("iron", 100);
        planetA.Inventory.AddStock("food", 50);
        planetA.Inventory.SetDemand("food", 500);
        
        // Planet B - Agrar-Planet (viel Nahrung, wenig Eisen)
        planetB = new MarketPlace { MarketName = "Planet Beta (Agrar)" };
        AddChild(planetB);
        tradingSystem.RegisterMarket(planetB);
        
        planetB.Inventory.AddStock("food", 1000);
        planetB.Inventory.SetDemand("food", 100);
        planetB.Inventory.AddStock("iron", 50);
        planetB.Inventory.SetDemand("iron", 500);
        
        // Zeige Märkte
        planetA.PrintMarketStatus();
        planetB.PrintMarketStatus();
        
        // Simuliere Handel
        SimulateTrade();
    }
    
    private void SimulateTrade()
    {
        GD.Print("\n=== Handel Simulation ===\n");
        
        player.PrintInventory();
        
        // Kaufe Eisen auf Planet A (billig)
        GD.Print("Kaufe 100 Eisen auf Planet Alpha...");
        if (planetA.Buy("iron", 100, out int buyPrice))
        {
            if (player.RemoveCredits(buyPrice))
            {
                player.AddCargo("iron", 100);
            }
        }
        
        player.PrintInventory();
        
        // Verkaufe Eisen auf Planet B (teuer)
        GD.Print("\nVerkaufe 100 Eisen auf Planet Beta...");
        if (player.RemoveCargo("iron", 100))
        {
            if (planetB.Sell("iron", 100, out int sellPrice))
            {
                player.AddCredits(sellPrice);
            }
        }
        
        player.PrintInventory();
        
        GD.Print($"\nGewinn: {player.Credits - 10000} Credits!");
        
        planetA.PrintMarketStatus();
        planetB.PrintMarketStatus();
    }
}
