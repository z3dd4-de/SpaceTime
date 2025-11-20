using Godot;

public partial class TestPlayer : CharacterBody2D
{
    [Export] public float Speed = 200f;
    
    public override void _Ready()
    {
        AddToGroup("Player");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Vector2.Zero;
        
        if (Input.IsActionPressed("ui_right"))
            velocity.X += 1;
        if (Input.IsActionPressed("ui_left"))
            velocity.X -= 1;
        if (Input.IsActionPressed("ui_down"))
            velocity.Y += 1;
        if (Input.IsActionPressed("ui_up"))
            velocity.Y -= 1;
        
        velocity = velocity.Normalized() * Speed;
        Velocity = velocity;
        MoveAndSlide();
    }
}
