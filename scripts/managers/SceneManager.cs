using Godot;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    public Dictionary<string, string> Scenes { get; private set; } = new()
    {
        { "Start", "res://scenes/start_screen.tscn" },
        { "Menu", "res://scenes/main_menu.tscn" },
        { "Credits", "res://scenes/credits.tscn" },
        { "Game", "res://scenes/game.tscn" },
        { "GameCreation", "res://scenes/game_creation.tscn" },
        { "GameCreation2", "res://scenes/game_creation_2.tscn" }
    };

    private string _currentSceneAlias = "";
    private ColorRect _fadeRect;
    private Tween _fadeTween;
    private bool _isFading = false;

    [Export] private float fadeDuration = 0.5f;
    [Export] private Color fadeColor = new Color(0, 0, 0); // Schwarz

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        _currentSceneAlias = "Start";

        // === Fade-Overlay erstellen ===
        _fadeRect = new ColorRect
        {
            Color = new Color(fadeColor, 0f), // Start transparent
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };

        var layer = new CanvasLayer();
        AddChild(layer);
        layer.AddChild(_fadeRect);

        // Starte Fade-In (optional)
        FadeIn();
    }

    private async void FadeIn()
    {
        _fadeRect.Color = new Color(fadeColor, 1.0f);
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(_fadeRect, "color:a", 0.0f, fadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        await ToSignal(_fadeTween, Tween.SignalName.Finished);
    }

    public async void SwitchScene(string alias, bool autoSave = true)
    {
        if (_isFading)
            return;

        if (!Scenes.ContainsKey(alias))
        {
            GD.PrintErr($"Unbekannter Szenen-Alias: {alias}");
            return;
        }

        _isFading = true;
        string scenePath = Scenes[alias];
        GD.Print($"Fading to scene '{alias}' ({scenePath})");

        // Fade-Out
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(_fadeRect, "color:a", 1.0f, fadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        await ToSignal(_fadeTween, Tween.SignalName.Finished);

        // === GameState aktualisieren ===
        if (GameState.Instance != null)
        {
            GameState.Instance.LastScene = scenePath;
            if (autoSave)
                GameState.AutoSave();
        }

        // Szene wechseln
        GetTree().ChangeSceneToFile(scenePath);
        _currentSceneAlias = alias;

        await ToSignal(GetTree(), SceneTree.SignalName.SceneChanged);

        // Fade-In
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(_fadeRect, "color:a", 0.0f, fadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        await ToSignal(_fadeTween, Tween.SignalName.Finished);

        _isFading = false;
    }

    public void RestartScene()
    {
        GD.Print($"Starte Szene '{_currentSceneAlias}' neu...");
        GetTree().ReloadCurrentScene();
    }

    public void QuitGame()
    {
        GD.Print("Spiel wird beendet...");
        GetTree().Quit();
    }

    public int GetSceneCount() => Scenes.Count;
    public string GetCurrentSceneAlias() => _currentSceneAlias;
}
