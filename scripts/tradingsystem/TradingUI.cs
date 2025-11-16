// TradingUI.cs - Hauptsteuerung der Trading-Oberfläche
using Godot;
using System;
using System.Collections.Generic;

public partial class TradingUI : CanvasLayer
{
    // UI References
    private Panel mainPanel;
    private Label marketNameLabel;
    private Label playerCreditsLabel;
    private Button closeButton;
    
    private VBoxContainer playerInventoryList;
    private VBoxContainer marketInventoryList;
    
    private Label selectedItemNameLabel;
    private Label selectedItemDescLabel;
    private SpinBox amountSpinBox;
    private Label totalPriceLabel;
    private Label unitPriceLabel;
    private Button buyButton;
    private Button sellButton;
    
    // Data
    private MarketPlace currentMarket;
    private PlayerInventory player;
    private string selectedResourceId = "";
    private bool isPlayerSide = true; // true = player selected, false = market selected
    
    public override void _Ready()
    {
        // Get references
        mainPanel = GetNode<Panel>("Panel");
        
        // Top bar
        marketNameLabel = GetNode<Label>("Panel/MarginContainer/VBox/TopBar/MarketName");
        playerCreditsLabel = GetNode<Label>("Panel/MarginContainer/VBox/TopBar/Credits");
        closeButton = GetNode<Button>("Panel/MarginContainer/VBox/TopBar/CloseButton");
        
        // Inventory lists
        playerInventoryList = GetNode<VBoxContainer>("Panel/MarginContainer/VBox/Content/LeftSide/ScrollContainer/InventoryList");
        marketInventoryList = GetNode<VBoxContainer>("Panel/MarginContainer/VBox/Content/RightSide/ScrollContainer/InventoryList");
        
        // Middle section
        selectedItemNameLabel = GetNode<Label>("Panel/MarginContainer/VBox/Content/MiddleSection/SelectedItem/ItemName");
        selectedItemDescLabel = GetNode<Label>("Panel/MarginContainer/VBox/Content/MiddleSection/SelectedItem/ItemDesc");
        amountSpinBox = GetNode<SpinBox>("Panel/MarginContainer/VBox/Content/MiddleSection/AmountSection/AmountSpinBox");
        totalPriceLabel = GetNode<Label>("Panel/MarginContainer/VBox/Content/MiddleSection/PriceInfo/TotalPrice");
        unitPriceLabel = GetNode<Label>("Panel/MarginContainer/VBox/Content/MiddleSection/PriceInfo/UnitPrice");
        buyButton = GetNode<Button>("Panel/MarginContainer/VBox/Content/MiddleSection/Actions/BuyButton");
        sellButton = GetNode<Button>("Panel/MarginContainer/VBox/Content/MiddleSection/Actions/SellButton");
        
        // Connect signals
        closeButton.Pressed += OnClosePressed;
        buyButton.Pressed += OnBuyPressed;
        sellButton.Pressed += OnSellPressed;
        amountSpinBox.ValueChanged += OnAmountChanged;
        
        // Start hidden
        Hide();
    }
    
    public void OpenMarket(MarketPlace market, PlayerInventory playerInventory)
    {
        currentMarket = market;
        player = playerInventory;
        
        marketNameLabel.Text = market.MarketName;
        UpdateUI();
        Show();
    }
    
    private void UpdateUI()
    {
        if (player == null || currentMarket == null) return;
        
        // Update credits
        playerCreditsLabel.Text = $"Credits: {player.Credits}";
        
        // Clear lists
        ClearList(playerInventoryList);
        ClearList(marketInventoryList);
        
        // Populate player inventory
        var tradingSystem = TradingSystem.Instance;
        foreach (var kvp in player.GetAllCargo())
        {
            var resource = tradingSystem.GetResource(kvp.Key);
            if (resource != null)
            {
                var item = CreateResourceItem(resource, kvp.Value, true);
                playerInventoryList.AddChild(item);
            }
        }
        
        // Populate market inventory
        foreach (var kvp in currentMarket.Inventory.Stock)
        {
            var resource = tradingSystem.GetResource(kvp.Key);
            if (resource != null)
            {
                int stock = kvp.Value;
                int price = currentMarket.CalculatePrice(kvp.Key);
                var item = CreateResourceItem(resource, stock, false, price);
                marketInventoryList.AddChild(item);
            }
        }
        
        // Reset selection
        ClearSelection();
    }
    
    private ResourceListItem CreateResourceItem(Resource resource, int amount, bool isPlayer, int price = 0)
    {
        var item = GD.Load<PackedScene>("res://scenes/ui/resource_list_item.tscn").Instantiate<ResourceListItem>();
        item.Setup(resource, amount, price, isPlayer);
        item.ItemSelected += OnResourceSelected;
        return item;
    }
    
    private void ClearList(VBoxContainer list)
    {
        foreach (Node child in list.GetChildren())
        {
            child.QueueFree();
        }
    }
    
    private void OnResourceSelected(string resourceId, bool fromPlayer)
    {
        selectedResourceId = resourceId;
        isPlayerSide = fromPlayer;
        
        var resource = TradingSystem.Instance.GetResource(resourceId);
        if (resource == null) return;
        
        selectedItemNameLabel.Text = resource.Name;
        selectedItemDescLabel.Text = resource.Description;
        
        // Set max amount for spinbox
        if (fromPlayer)
        {
            // Selling - max is player's cargo
            amountSpinBox.MaxValue = player.GetCargoAmount(resourceId);
            buyButton.Disabled = true;
            sellButton.Disabled = false;
        }
        else
        {
            // Buying - max is market's stock
            amountSpinBox.MaxValue = currentMarket.Inventory.GetStock(resourceId);
            buyButton.Disabled = false;
            sellButton.Disabled = true;
        }
        
        amountSpinBox.Value = 1;
        UpdatePriceDisplay();
    }
    
    private void OnAmountChanged(double value)
    {
        UpdatePriceDisplay();
    }
    
    private void UpdatePriceDisplay()
    {
        if (string.IsNullOrEmpty(selectedResourceId)) return;
        
        int unitPrice = currentMarket.CalculatePrice(selectedResourceId);
        int amount = (int)amountSpinBox.Value;
        int totalPrice = unitPrice * amount;
        
        unitPriceLabel.Text = $"Preis pro Einheit: {unitPrice} Credits";
        totalPriceLabel.Text = $"Gesamt: {totalPrice} Credits";
    }
    
    private void OnBuyPressed()
    {
        if (string.IsNullOrEmpty(selectedResourceId)) return;
        
        int amount = (int)amountSpinBox.Value;
        
        if (currentMarket.Buy(selectedResourceId, amount, out int totalPrice))
        {
            if (player.RemoveCredits(totalPrice))
            {
                player.AddCargo(selectedResourceId, amount);
                UpdateUI();
            }
            else
            {
                // Nicht genug Credits - Trade rückgängig machen
                currentMarket.Inventory.AddStock(selectedResourceId, amount);
            }
        }
    }
    
    private void OnSellPressed()
    {
        if (string.IsNullOrEmpty(selectedResourceId)) return;
        
        int amount = (int)amountSpinBox.Value;
        
        if (player.RemoveCargo(selectedResourceId, amount))
        {
            if (currentMarket.Sell(selectedResourceId, amount, out int totalPrice))
            {
                player.AddCredits(totalPrice);
                UpdateUI();
            }
            else
            {
                // Verkauf fehlgeschlagen - Cargo zurückgeben
                player.AddCargo(selectedResourceId, amount);
            }
        }
    }
    
    private void ClearSelection()
    {
        selectedResourceId = "";
        selectedItemNameLabel.Text = "Keine Auswahl";
        selectedItemDescLabel.Text = "";
        totalPriceLabel.Text = "Gesamt: 0 Credits";
        unitPriceLabel.Text = "Preis pro Einheit: 0 Credits";
        buyButton.Disabled = true;
        sellButton.Disabled = true;
    }
    
    private void OnClosePressed()
    {
        Hide();
    }
}
