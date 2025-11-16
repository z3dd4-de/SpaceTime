// TradingTrigger.cs - Area2D die das Trading öffnet
using Godot;

public partial class TradingTrigger : Area2D
{
    [Export] public NodePath MarketPlacePath { get; set; }
    [Export] public NodePath TradingUIPath { get; set; }
    [Export] public NodePath PlayerInventoryPath { get; set; }
    
    private MarketPlace market;
    private TradingUI tradingUI;
    private PlayerInventory player;
    private bool playerInRange = false;
    private Label promptLabel;
    
    public override void _Ready()
    {
        market = GetNode<MarketPlace>(MarketPlacePath);
        tradingUI = GetNode<TradingUI>(TradingUIPath);
        player = GetNode<PlayerInventory>(PlayerInventoryPath);
        
        // Create prompt label
        promptLabel = new Label();
        promptLabel.Text = "[E] Handeln";
        promptLabel.Position = new Vector2(-50, -60);
        promptLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        promptLabel.Hide();
        AddChild(promptLabel);
        
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            playerInRange = true;
            promptLabel.Show();
        }
    }
    
    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            playerInRange = false;
            promptLabel.Hide();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (playerInRange && @event.IsActionPressed("ui_accept")) // E-Taste
        {
            tradingUI.OpenMarket(market, player);
        }
    }
}