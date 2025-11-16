// ResourceListItem.cs - Einzelnes Item in der Liste
using Godot;
using System;

public partial class ResourceListItem : PanelContainer
{
    [Signal]
    public delegate void ItemSelectedEventHandler(string resourceId, bool isPlayer);
    
    private Label nameLabel;
    private Label amountLabel;
    private Label priceLabel;
    private Button selectButton;
    
    private string resourceId;
    private bool isPlayerItem;
    
    public override void _Ready()
    {
        nameLabel = GetNode<Label>("HBox/NameLabel");
        amountLabel = GetNode<Label>("HBox/AmountLabel");
        priceLabel = GetNode<Label>("HBox/PriceLabel");
        selectButton = GetNode<Button>("HBox/SelectButton");
        
        selectButton.Pressed += OnSelectPressed;
    }
    
    public void Setup(Resource resource, int amount, int price, bool isPlayer)
    {
        resourceId = resource.Id;
        isPlayerItem = isPlayer;
        
        nameLabel.Text = resource.Name;
        amountLabel.Text = $"x{amount}";
        
        if (isPlayer)
        {
            priceLabel.Text = ""; // Player items don't show price in list
        }
        else
        {
            priceLabel.Text = $"{price} C";
        }
    }
    
    private void OnSelectPressed()
    {
        EmitSignal(SignalName.ItemSelected, resourceId, isPlayerItem);
    }
}
