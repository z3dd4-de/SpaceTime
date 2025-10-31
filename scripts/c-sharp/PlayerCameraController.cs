using Godot;
using System;

public partial class PlayerCameraController : Node3D
{
    [Export] public Camera3D Camera; // Kamera-Referenz
    [Export] public float Acceleration = 0.5f; // Beschleunigungskraft 0.1
    [Export] public float MaxSpeed = 5f;      // Maximalgeschwindigkeit 1.0 - 5.0
    [Export] public float RotationSpeed = 1.5f; // Rotationsgeschwindigkeit
    [Export] public float ZoomSpeed = 5f;       // Zoomgeschwindigkeit (FOV)
    [Export] public float MinFov = 30f;
    [Export] public float MaxFov = 90f;
    [Export] public float MouseSensitivity = 0.3f;

    private float _speed = 0f;
    private Vector3 _velocity = Vector3.Zero;
    private float _targetSpeed = 0f;

    private bool _isRotatingView = false;

    public override void _Ready()
    {
        if (Camera == null)
            Camera = GetNode<Camera3D>("Camera3D");

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _Process(double delta)
    {
        HandleMovement((float)delta);
        HandleRotation((float)delta);
        HandleZoom((float)delta);
        HandleMouseLook((float)delta);
    }

    private void HandleMovement(float delta)
    {
        // Vorwärts / Rückwärts (W/S oder Pfeiltasten)
        float forwardInput = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward");

        // Zielgeschwindigkeit anpassen
        _targetSpeed = forwardInput * MaxSpeed;

        // Allmähliche Annäherung (weiche Beschleunigung)
        _speed = Mathf.Lerp(_speed, _targetSpeed, Acceleration * delta);

        // Bewegung in Blickrichtung
        _velocity = -Transform.Basis.Z * _speed * delta; // -Z = Forward
        Translate(_velocity);
    }

    private void HandleRotation(float delta)
    {
        float yaw = Input.GetActionStrength("turn_right") - Input.GetActionStrength("turn_left");
        float roll = Input.GetActionStrength("roll_right") - Input.GetActionStrength("roll_left");
        float pitch = Input.GetActionStrength("pitch_down") - Input.GetActionStrength("pitch_up");

        // Rotationsgeschwindigkeit anpassen
        Vector3 rotationDelta = new Vector3(pitch, yaw, roll) * RotationSpeed * delta;

        RotateObjectLocal(Vector3.Right, rotationDelta.X);  // Pitch
        RotateObjectLocal(Vector3.Up, rotationDelta.Y);     // Yaw
        RotateObjectLocal(Vector3.Back, rotationDelta.Z);   // Roll
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

    private void HandleMouseLook(float delta)
    {
        // Wenn die Action "rotate_view" gedrückt ist (rechte Maustaste)
        if (Input.IsActionPressed("rotate_view"))
        {
            if (!_isRotatingView)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                _isRotatingView = true;
            }

            // Mausbewegung lesen
            Vector2 mouseDelta = Input.GetLastMouseVelocity() * delta * MouseSensitivity; // Skalierung anpassen
            RotateY(-mouseDelta.X * 0.5f); // Yaw (links/rechts)
            RotateObjectLocal(Vector3.Right, -mouseDelta.Y * 0.5f); // Pitch (hoch/runter)
        }
        else if (_isRotatingView)
        {
            // Maussteuerung wieder freigeben, wenn losgelassen
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _isRotatingView = false;
        }
    }

}

