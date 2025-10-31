using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private RichTextLabel _speedLabel;
    private RichTextLabel _positionLabel;
    private RichTextLabel _rotationLabel;
    private RichTextLabel _targetLabel;
    private RichTextLabel _distanceLabel;
    private TextureRect _targetArrow;

    private Node3D _target; // aktuell anvisiertes Ziel (Planet o.Ä.)
    private Camera3D _camera;
    private Tween _arrowTween;
    private bool _isPulsing = false;

    // Lichtgeschwindigkeit in m/s (zur Berechnung relativer Geschwindigkeit)
    private const float LIGHT_SPEED = 299_792_458f;
    private const float FAR_DISTANCE = 100_000_000f; // 100.000 km (in Metern)

    public override void _Ready()
    {
        _speedLabel = GetNode<RichTextLabel>("PanelContainer/VBoxContainer/SpeedLabel");
        _positionLabel = GetNode<RichTextLabel>("PanelContainer/VBoxContainer/PositionLabel");
        _rotationLabel = GetNode<RichTextLabel>("PanelContainer/VBoxContainer/RotationLabel");
        _targetLabel = GetNode<RichTextLabel>("PanelContainer/VBoxContainer/TargetLabel");
        _distanceLabel = GetNode<RichTextLabel>("PanelContainer/VBoxContainer/DistanceLabel");
        _targetArrow = GetNode<TextureRect>("TargetArrow");

        // Suche automatisch die Hauptkamera (kannst du bei Bedarf anpassen)
        _camera = GetViewport().GetCamera3D();
    }

    public void SetTarget(Node3D target)
    {
        _target = target;
    }

    public void UpdateHUD(Vector3 velocity, Vector3 position, Vector3 rotation)
    {
        float speed = velocity.Length();
        float relativeSpeed = speed / LIGHT_SPEED;

        _speedLabel.Text = $"Speed: {speed:F1} m/s ({relativeSpeed * 100:F4}% c)";
        _positionLabel.Text = $"Position: X={position.X:F1} Y={position.Y:F1} Z={position.Z:F1}";
        _rotationLabel.Text = $"Rotation: Pitch={rotation.X:F1}°  Yaw={rotation.Y:F1}°  Roll={rotation.Z:F1}°";

        if (_target != null)
        {
            float distance = position.DistanceTo(_target.GlobalPosition);
            _targetLabel.Text = $"Target: {_target.Name}";
            _distanceLabel.Text = $"Distance: {distance / 1000:F1} km";

            UpdateTargetArrow(_target.GlobalPosition, distance);
        }
        else
        {
            _targetLabel.Text = "Target: none";
            _distanceLabel.Text = "Distance: --";
            _targetArrow.Visible = false;
        }
    }

    private void UpdateTargetArrow(Vector3 targetPosition, float distance)
    {
        if (_camera == null)
            return;

        Vector3 camToTarget = targetPosition - _camera.GlobalPosition;
        bool isInFront = _camera.GlobalTransform.Basis.Z.Dot(camToTarget) < 0f;

        Vector2 screenPos = _camera.UnprojectPosition(targetPosition);
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        Vector2 screenCenter = viewportSize / 2f;

        float padding = 60f;

        // 🧭 Sichtfeld-Prüfung
        if (isInFront)
        {
            bool isOffscreen = screenPos.X < 0 || screenPos.X > viewportSize.X ||
                               screenPos.Y < 0 || screenPos.Y > viewportSize.Y;

            if (!isOffscreen)
            {
                _targetArrow.Visible = false;
                StopArrowPulse();
                return;
            }

            Vector2 dir = (screenPos - screenCenter).Normalized();
            float maxX = (viewportSize.X / 2f) - padding;
            float maxY = (viewportSize.Y / 2f) - padding;

            Vector2 arrowPos = screenCenter + dir * 10000f;
            float tX = Math.Abs((arrowPos.X - screenCenter.X) / maxX);
            float tY = Math.Abs((arrowPos.Y - screenCenter.Y) / maxY);
            float t = Math.Max(tX, tY);
            arrowPos = screenCenter + dir * (1f / t) * new Vector2(maxX, maxY).Length() * 0.5f;

            arrowPos.X = Mathf.Clamp(arrowPos.X, padding, viewportSize.X - padding);
            arrowPos.Y = Mathf.Clamp(arrowPos.Y, padding, viewportSize.Y - padding);

            _targetArrow.Position = arrowPos;
            _targetArrow.Rotation = dir.Angle() + Mathf.Pi / 2f;
            _targetArrow.Visible = true;
        }
        else
        {
            _targetArrow.Position = screenCenter;
            _targetArrow.Rotation = Mathf.Pi;
            _targetArrow.Visible = true;
        }

        // ✨ Blink-/Pulslogik:
        if (distance > FAR_DISTANCE || !isInFront)
        {
            StartArrowPulse();
        }
        else
        {
            StopArrowPulse();
        }
    }

    private void StartArrowPulse()
    {
        if (_isPulsing) return;

        _isPulsing = true;
        _arrowTween = CreateTween();
        _arrowTween.SetLoops();
        _arrowTween.TweenProperty(_targetArrow, "modulate:a", 0.2f, 0.6f)
                    .SetTrans(Tween.TransitionType.Sine)
                    .SetEase(Tween.EaseType.InOut);
        _arrowTween.TweenProperty(_targetArrow, "modulate:a", 1f, 0.6f)
                    .SetTrans(Tween.TransitionType.Sine)
                    .SetEase(Tween.EaseType.InOut);
    }

    private void StopArrowPulse()
    {
        if (!_isPulsing) return;

        _isPulsing = false;
        _arrowTween?.Kill();
        _targetArrow.Modulate = Colors.White; // zurücksetzen
    }
}
