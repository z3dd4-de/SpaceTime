using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    [Export] private Panel pauseMenuPanel;
    [Export] private Button ResumeButton;
    [Export] private Button SaveButton;
    [Export] private Button MainMenuButton;

    [Export] private float fadeDuration = 0.3f;
    private Tween currentTween;
    private bool isPaused = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; // 👈 Buttons reagieren im Pausenmodus

        // Stelle sicher, dass das Panel ebenfalls "WhenPaused" hat (falls nötig)
        if (pauseMenuPanel != null)
            pauseMenuPanel.ProcessMode = ProcessModeEnum.WhenPaused;

        if (ResumeButton == null)
            ResumeButton = GetNode<Button>("PanelContainer/VBoxContainer/ResumeButton");
        if (SaveButton == null)
            SaveButton = GetNode<Button>("PanelContainer/VBoxContainer/SaveButton");
        if (MainMenuButton == null)
            MainMenuButton = GetNode<Button>("PanelContainer/VBoxContainer/MainMenuButton");

        ResumeButton.Pressed += OnResumePressed;
        SaveButton.Pressed += OnSavePressed;
        MainMenuButton.Pressed += OnMainMenuPressed;

        pauseMenuPanel.Visible = false;
        pauseMenuPanel.Modulate = new Color(1, 1, 1, 0);

        Hide();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    private void PauseGame()
    {
        // Panel sichtbar machen (Startzustand vorher auf alpha=0 setzen)
        pauseMenuPanel.Visible = true;

        // Tween an das Panel binden (so läuft er auch wenn der Tree paused ist,
        // vorausgesetzt das Panel hat ProcessMode = WhenPaused)
        currentTween?.Kill();
        currentTween = pauseMenuPanel.CreateTween();
        currentTween.TweenProperty(
            pauseMenuPanel,
            "modulate:a", // Alpha-Kanal
            1.0f,
            fadeDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

        // Dann pausieren (Input für Nodes mit WhenPaused bleibt aktiv)
        GetTree().Paused = true;
        isPaused = true;
        Show();
        GD.Print("Spiel pausiert.");
    }

    private void ResumeGame()
    {
        GD.Print("Resume clicked.");

        currentTween?.Kill();
        currentTween = pauseMenuPanel.CreateTween();
        currentTween.TweenProperty(
            pauseMenuPanel,
            "modulate:a",
            0.0f,
            fadeDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        currentTween.Finished += () =>
        {
            pauseMenuPanel.Visible = false;
        };

        // Aufheben der Pause (Tween, der an pauseMenuPanel gebunden ist, läuft trotzdem bis zum Ende,
        // weil das Panel ProcessMode = WhenPaused hat — aber es ist sicher, die Pause hier aufzuheben)
        GetTree().Paused = false;
        isPaused = false;
        Hide();
        GD.Print("Spiel fortgesetzt.");
    }


    private void OnResumePressed() => ResumeGame();

    private void OnSavePressed()
    {
        string slot = GameState.Instance.CurrentSlotName;
        if (string.IsNullOrEmpty(slot))
        {
            GD.PrintErr("Kein aktiver Speicher-Slot!");
            return;
        }

        var data = SaveManager.LoadSlot(slot);
        data.Credits += 10;
        data.CurrentScene = GetTree().CurrentScene.SceneFilePath;
        SaveManager.SaveSlot(slot, data);

        GD.Print($"Spiel gespeichert in Slot '{slot}' (Szene: {data.CurrentScene}).");
    }

    private void OnMainMenuPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/SaveMenu.tscn");
        GD.Print("Zurück zum Hauptmenü...");
    }
}
