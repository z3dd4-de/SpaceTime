using Godot;
using System;

public partial class PlayerShipController : Node3D
{
    [Export] public Camera3D Camera; // Kamera-Referenz
    [Export] public float ThrustPower = 5f;           // Beschleunigungskraft (m/s²)
    [Export] public float RotationSpeed = 1.5f;       // Drehgeschwindigkeit
    [Export] public float RollSpeed = 1.5f;           // Rollgeschwindigkeit
    [Export] public float MaxSpeed = 100f;            // Maximale Geschwindigkeit (m/s)
    [Export] public float Drag = 0.1f;                // Luftwiderstand im All (eigentlich 0, aber fürs Gameplay ok)
    [Export] public float MouseSensitivity = 0.1f;    // Umschau-Empfindlichkeit
    [Export] public float ZoomSpeed = 5f;       // Zoomgeschwindigkeit (FOV)
    [Export] public float MinFov = 30f;
    [Export] public float MaxFov = 90f;

    private Vector3 _velocity = Vector3.Zero;
    private bool _isRotatingView = false;
    private HUD _hud;
    private Node3D _targetPlanet;


    public override void _Process(double delta)
    {
        float dt = (float)delta;
        HandleRotation(dt);
        HandleMovement(dt);
        HandleMouseLook(dt);
        HandleZoom((float)delta);

        // Bewegung anwenden
        GlobalPosition += _velocity * dt;

        if (_hud != null)
        {
            Vector3 euler = RotationDegrees; // einfacher als Quaternion
            _hud.UpdateHUD(_velocity, GlobalPosition, euler);
        }
    }

    public override void _Ready()
    {
        _hud = GetTree().Root.GetNode<HUD>("SolarSystem/HUD");
        if (Camera == null)
            Camera = GetNode<Camera3D>("Camera3D");

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void HandleZoom(float delta)
    {
        if (Camera == null)
            return;

        if (Input.IsMouseButtonPressed(MouseButton.Middle))
        {
            Vector2 mouseMotion = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            Camera.Fov = Mathf.Clamp(Camera.Fov - mouseMotion.Y * ZoomSpeed * delta, MinFov, MaxFov);
        }

        // Alternativ: Mausrad-Zoom
        if (Input.IsActionJustPressed("zoom_in"))
            Camera.Fov = Mathf.Max(MinFov, Camera.Fov - ZoomSpeed);
        if (Input.IsActionJustPressed("zoom_out"))
            Camera.Fov = Mathf.Min(MaxFov, Camera.Fov + ZoomSpeed);
    }

    private void HandleMovement(float delta)
    {
        Vector3 direction = Vector3.Zero;

        // Vorwärts / Rückwärts (W/S)
        if (Input.IsActionPressed("move_forward"))
            direction -= Transform.Basis.Z; // Vorwärts ist -Z in Godot
        if (Input.IsActionPressed("move_backward"))
            direction += Transform.Basis.Z;

        // Links / Rechts (A/D)
        if (Input.IsActionPressed("turn_left"))
            direction -= Transform.Basis.X;
        if (Input.IsActionPressed("turn_right"))
            direction += Transform.Basis.X;

        // Beschleunigung anwenden
        if (direction != Vector3.Zero)
        {
            _velocity += direction.Normalized() * ThrustPower * delta;
            _velocity = _velocity.LimitLength(MaxSpeed);
        }
        else
        {
            // Leichtes „Abbremsen“ (Trägheit simulieren)
            _velocity = _velocity.Lerp(Vector3.Zero, Drag * delta);
        }
    }

    private void HandleRotation(float delta)
    {
        // Pitch (Up/Down)
        float pitch = 0f;
        if (Input.IsActionPressed("pitch_up"))
            pitch -= 1f;
        if (Input.IsActionPressed("pitch_down"))
            pitch += 1f;

        // Yaw (Left/Right)
        float yaw = 0f;
        if (Input.IsActionPressed("turn_left"))
            yaw += 1f;
        if (Input.IsActionPressed("turn_right"))
            yaw -= 1f;

        // Roll (Q/E)
        float roll = 0f;
        if (Input.IsActionPressed("roll_left"))
            roll += 1f;
        if (Input.IsActionPressed("roll_right"))
            roll -= 1f;

        // Drehung anwenden
        RotateObjectLocal(Vector3.Right, pitch * RotationSpeed * delta); // Pitch
        RotateY(yaw * RotationSpeed * delta);                            // Yaw
        RotateObjectLocal(Vector3.Back, roll * RollSpeed * delta);       // Roll
    }

    private void HandleMouseLook(float delta)
    {
        if (Input.IsActionPressed("rotate_view"))
        {
            if (!_isRotatingView)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                _isRotatingView = true;
            }

            Vector2 mouseDelta = Input.GetLastMouseVelocity() * delta * MouseSensitivity;
            RotateY(-mouseDelta.X);
            RotateObjectLocal(Vector3.Right, -mouseDelta.Y);
        }
        else if (_isRotatingView)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _isRotatingView = false;
        }
    }
}

