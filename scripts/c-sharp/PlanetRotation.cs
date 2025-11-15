using Godot;
using System;

public partial class PlanetRotation : Node3D
{
    // Drehgeschwindigkeit in Grad pro Sekunde
    [Export] public Vector3 RotationSpeed { get; set; } = new Vector3(0, 15, 0);

    public override void _PhysicsProcess(double delta)
    {
        // Umrechnung in Radiant pro Frame
        Vector3 radians = RotationSpeed * Mathf.DegToRad((float)delta);
        RotateObjectLocal(Vector3.Right, radians.X);
        RotateObjectLocal(Vector3.Up, radians.Y);
        RotateObjectLocal(Vector3.Back, radians.Z);
    }
}
