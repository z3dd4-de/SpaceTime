using Godot;
using System;
using System.Collections.Generic;

public partial class StoryStart : Node2D
{
    [Export] public float TypingSpeed { get; set; } = 0.05f; // Sekunden pro Buchstabe
    [Export] public int SoundInterval { get; set; } = 5;      // alle X Zeichen Ton abspielen
    [Export] public string SoundDirectory { get; set; } = "res://audio/keyboard_sounds";
    [Export] public float PitchVariation { get; set; } = 0.05f; // ± Variation der Tonhöhe
    [Export] public float VolumeOffsetDb { get; set; } = -1.0f; // dB-Abweichung (negativ = leiser)

    private PanelContainer _panel;
    private RichTextLabel _label;
    private AudioStreamPlayer _audio;
    private Button _okButton;
    private Tween _tween;

    private float _charTimer = 0f;
    private int _charIndex = 0;
    private int _totalCharacters = 0;
    private bool _finished = false;
    private bool _soundPlaying = false;

    private List<AudioStream> _streams = new();
    private RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _panel = GetNode<PanelContainer>("PanelContainer");
        _label = GetNode<RichTextLabel>("PanelContainer/RichTextLabel");
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _okButton = GetNode<Button>("PanelContainer/OkButton");

        _label.VisibleRatio = 0f;
        _totalCharacters = Math.Max(1, _label.GetTotalCharacterCount()); // vermeide Div0

        // Panel weich einblenden
        _panel.Modulate = new Color(1, 1, 1, 0);
        _tween = CreateTween();
        _tween.TweenProperty(_panel, "modulate:a", 1.0f, 0.8f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        _okButton.Disabled = true;
        _okButton.Pressed += OnOkButtonPressed;

        _rng.Randomize();
        LoadKeyboardSounds();

        // Event: Wenn Sound endet → wieder freigeben
        _audio.Finished += () => _soundPlaying = false;
    }

    private void LoadKeyboardSounds()
    {
        _streams.Clear();

        using var dir = DirAccess.Open(SoundDirectory);
        if (dir == null)
        {
            GD.PrintErr($"[StoryStart] Konnte Sound-Verzeichnis nicht öffnen: {SoundDirectory}");
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        int loaded = 0;

        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && (fileName.EndsWith(".wav") || fileName.EndsWith(".ogg") || fileName.EndsWith(".mp3")))
            {
                string fullPath = $"{SoundDirectory}/{fileName}";
                var stream = GD.Load<AudioStream>(fullPath);
                if (stream != null)
                {
                    _streams.Add(stream);
                    loaded++;
                }
                else
                {
                    GD.PrintErr($"[StoryStart] Konnte Audio nicht laden: {fullPath}");
                }
            }
            fileName = dir.GetNext();
        }

        dir.ListDirEnd();

        if (loaded > 0)
        {
            GD.Print($"[StoryStart] {loaded} Tipp-Sounds geladen aus {SoundDirectory}.");
        }
        else
        {
            GD.PrintErr("[StoryStart] Keine Tipp-Sounds gefunden!");
        }
    }

    private void TryPlayRandomSound()
    {
        if (_soundPlaying || _streams.Count == 0)
            return;

        int idx = _rng.RandiRange(0, _streams.Count - 1);
        _audio.Stream = _streams[idx];

        // leichte Variation in Tonhöhe und Lautstärke
        float pitchDelta = (float)_rng.RandfRange(-PitchVariation, PitchVariation);
        _audio.PitchScale = 1.0f + pitchDelta;

        float volDb = VolumeOffsetDb + (float)_rng.RandfRange(-0.3f, 0.3f);
        _audio.VolumeDb = volDb;

        _soundPlaying = true;
        _audio.Play();
    }

    public override void _Process(double delta)
    {
        if (_finished)
            return;

        _charTimer += (float)delta;
        if (_charTimer >= TypingSpeed)
        {
            _charTimer = 0f;
            _charIndex++;

            _label.VisibleRatio = (float)_charIndex / _totalCharacters;

            // nur neuen Sound anstoßen, wenn keiner läuft
            if (_charIndex % SoundInterval == 0 && !_soundPlaying)
            {
                TryPlayRandomSound();
            }

            if (_charIndex >= _totalCharacters)
            {
                _finished = true;
                _okButton.Disabled = false;
            }
        }
    }

    private void OnOkButtonPressed()
    {
        _okButton.Disabled = true;

        _tween = CreateTween();
        _tween.TweenProperty(_panel, "modulate:a", 0.0f, 0.6f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.In);
        _tween.Finished += OnFadeOutComplete;
    }

    private void OnFadeOutComplete()
    {
        QueueFree();
        SceneManager.Instance.SwitchScene("Game");
    }
}
