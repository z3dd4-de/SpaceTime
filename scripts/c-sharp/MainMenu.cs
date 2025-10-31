using Godot;
using System;

public partial class MainMenu : Node2D
{
    [Export] private Parallax2D parallax;
    [Export] private TextureRect background;
    [Export] private TextureRect foreground;
    [Export] private RichTextLabel titleLabel;
    //[Export] private Panel mainMenuBackground;
    [Export] private VBoxContainer mainMenuPanel;       //VBoxContainer
    [Export] private CanvasLayer canvasLayer;
    [Export] private Control settingsPanel;
    [Export] private Control audioPanel;
    [Export] private Control videoPanel;

    private Tween introTween;
    private Tween pulseTween;
    public SceneManager SceneManager;

    [Export] private float backgroundDriftSpeed = 10f;   //5f
    [Export] private float foregroundDriftSpeed = 20f;  //10f

    private Vector2 backgroundOffset = Vector2.Zero;
        
    private string[] resolutions = [
        "1920x1080",
        "1600x900",
        "1280x720",
        "1024x768",
        "800x600"
    ];
    

    //Bus indexes + Audio menu sliders
    private int master_index, music_index, sfx_index;
    [Export] private HSlider master_hslider;
    [Export] private HSlider music_hslider;
    [Export] private HSlider sfx_hslider;
    [Export] private CheckButton fullscreenButton;
    [Export] private CheckButton borderlessButton;
    [Export] private CheckButton vsyncButton;
    [Export] private OptionButton resolutionOptionButton;
    [Export] private Button audioButton;
    [Export] private Button videoButton;

    // Game Settings for Audio and Video
    private JsonData data;


    public override void _Ready()
    {
        SceneManager = GetNode<SceneManager>("/root/SceneManager");
        
        // Node references
        if (parallax == null)
            parallax = GetNode<Parallax2D>("Parallax2D");
        if (background == null)
            background = GetNode<TextureRect>("Parallax2D/Background");
        if (foreground == null && HasNode("Parallax2D/Foreground"))
            foreground = GetNode<TextureRect>("Parallax2D/Foreground");
        if (titleLabel == null)
            titleLabel = GetNode<RichTextLabel>("Parallax2D/Background/TitleLabel");//anpassen
        if (mainMenuPanel == null)
            mainMenuPanel = GetNode<VBoxContainer>("CanvasLayer/MainMenuPanel");
        if (settingsPanel == null)
            settingsPanel = GetNode<Control>("CanvasLayer/SettingsPanel");
        if (audioPanel == null)
            audioPanel = GetNode<Control>("CanvasLayer/AudioPanel");
        if (audioButton == null)
            audioButton = GetNode<Button>("CanvasLayer/SettingsPanel/SettingsVBoxContainer/AudioButton");
        if (videoPanel == null)
            videoPanel = GetNode<Control>("CanvasLayer/VideoPanel");
        if (videoButton == null)
            videoButton = GetNode<Button>("CanvasLayer/SettingsPanel/SettingsVBoxContainer/VideoButton");
        if (master_hslider == null)
            master_hslider = GetNode<HSlider>("CanvasLayer/AudioPanel/VBoxContainer/GridContainer/MainVolumeHSlider");
        if (music_hslider == null)
            music_hslider = GetNode<HSlider>("CanvasLayer/AudioPanel/VBoxContainer/GridContainer/MusicVolumeHSlider");
        if (sfx_hslider == null)
            sfx_hslider = GetNode<HSlider>("CanvasLayer/AudioPanel/VBoxContainer/GridContainer/SfxVolumeHSlider");
        if (fullscreenButton == null)
            fullscreenButton = videoPanel.GetNode<CheckButton>("VBoxContainer/GridContainer/FullscreenCheckButton");
        if (borderlessButton == null)
            borderlessButton = videoPanel.GetNode<CheckButton>("VBoxContainer/GridContainer/BorderlessCheckButton");
        if (vsyncButton == null)
            vsyncButton = videoPanel.GetNode<CheckButton>("VBoxContainer/GridContainer/VsyncCheckButton");
        if (resolutionOptionButton == null)
            resolutionOptionButton = videoPanel.GetNode<OptionButton>("VBoxContainer/GridContainer/ResolutionOptionButton");

        // Add Resolutions
        foreach (string res in resolutions)
            resolutionOptionButton.AddItem(res);

        titleLabel.Modulate = new Color(1, 1, 1, 0);
        foreach (Button b in mainMenuPanel.GetChildren())
            b.Modulate = new Color(1, 1, 1, 0);

        canvasLayer.Visible = false;
        settingsPanel.Visible = false;
        audioPanel.Visible = false;
        videoPanel.Visible = false;

        // Assign bus indexes
        master_index = AudioServer.GetBusIndex("Master");
        music_index = AudioServer.GetBusIndex("Music");
        sfx_index = AudioServer.GetBusIndex("SFX");

        data = GameSettings.Load();
        // first time loading
        master_hslider.Value = data.DefaultMasterVolume;
        music_hslider.Value = data.DefaultMusicVolume;
        sfx_hslider.Value = data.DefaultSfxVolume;
        fullscreenButton.ButtonPressed = data.FullScreen;
        borderlessButton.ButtonPressed = data.Borderless;
        vsyncButton.ButtonPressed = data.Vsync;
        resolutionOptionButton.Text = data.Resolution;
        LoadSavedSettings();

        parallax.ScrollOffset = Vector2.Zero;

        PlayIntroAnimation();
        ConnectButtonSignals();
    }

    private void LoadSavedSettings()
    {
        SetResolution(data.Resolution);
        SetVsync(data.Vsync);
        SetFullscreenMode(data.FullScreen);

        AudioServer.SetBusVolumeDb(master_index, Mathf.LinearToDb((float)data.MasterVolume));
        AudioServer.SetBusVolumeDb(music_index, Mathf.LinearToDb((float)data.MusicVolume));
        AudioServer.SetBusVolumeDb(sfx_index, Mathf.LinearToDb((float)data.SfxVolume));
    }


    public override void _Process(double delta)
    {
        float time = (float)Time.GetTicksMsec() / 1000f;

        // sanfter Drift (elliptische Bewegung)
        float dx = Mathf.Sin(time / 3f) * backgroundDriftSpeed;
        float dy = Mathf.Cos(time / 4f) * (backgroundDriftSpeed / 2f);

        // Beweg die Parallax-Ebene
        parallax.ScrollOffset = new Vector2(dx, dy);
    }

    private void PlayIntroAnimation()
    {
        introTween?.Kill();
        introTween = CreateTween();

        // Hintergrund leicht einzoomen
        background.Scale = new Vector2(1.0f, 1.0f);
        introTween.TweenProperty(background, "scale", new Vector2(1.15f, 1.15f), 6.0f)
                  .SetTrans(Tween.TransitionType.Sine)
                  .SetEase(Tween.EaseType.InOut);

        // Titel einblenden
        introTween.TweenInterval(0.5);
        introTween.TweenProperty(titleLabel, "modulate:a", 1.0f, 2.0f)
                  .SetTrans(Tween.TransitionType.Sine)
                  .SetEase(Tween.EaseType.Out);

        // Buttons nacheinander
        canvasLayer.Visible = true;
        double delay = 0.3;
        foreach (Button b in mainMenuPanel.GetChildren())
        {
            introTween.TweenInterval(delay);
            introTween.TweenProperty(b, "modulate:a", 1.0f, 1.0f)
                      .SetTrans(Tween.TransitionType.Sine)
                      .SetEase(Tween.EaseType.Out);
        }

        introTween.Finished += StartTitlePulse;
    }

    private void StartTitlePulse()
    {
        pulseTween?.Kill();
        pulseTween = CreateTween();
        pulseTween.SetLoops();

        pulseTween.TweenProperty(titleLabel, "modulate:a", 0.85f, 2.0f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        pulseTween.TweenProperty(titleLabel, "modulate:a", 1.0f, 2.0f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void ConnectButtonSignals()
    {
        var startButton = mainMenuPanel.GetNode<Button>("StartButton");
        var settingsButton = mainMenuPanel.GetNode<Button>("SettingsButton");
        var exitButton = mainMenuPanel.GetNode<Button>("ExitButton");
        var audioButton = settingsPanel.GetNode<Button>("SettingsVBoxContainer/AudioButton");
        var masterVolumeHslider = audioPanel.GetNode<HSlider>("VBoxContainer/GridContainer/MainVolumeHSlider");
        var musicVolumeHslider = audioPanel.GetNode<HSlider>("VBoxContainer/GridContainer/MusicVolumeHSlider");
        var sfxVolumeHslider = audioPanel.GetNode<HSlider>("VBoxContainer/GridContainer/SfxVolumeHSlider");
        var audioBackButton = audioPanel.GetNode<Button>("VBoxContainer/AudioBackButton");
        var settingsBackButton = settingsPanel.GetNode<Button>("SettingsVBoxContainer/SettingsBackButton");
        var fullscreenButton = videoPanel.GetNode<CheckButton>("VBoxContainer/GridContainer/FullscreenCheckButton");
        var borderlessButton = videoPanel.GetNode<CheckButton>("VBoxContainer/GridContainer/BorderlessCheckButton");
        var vsyncButton = videoPanel.GetNode<CheckButton>("VBoxContainer/GridContainer/VsyncCheckButton");
        var resolutionOptionButton = videoPanel.GetNode<OptionButton>("VBoxContainer/GridContainer/ResolutionOptionButton");
        var videoBackButton = videoPanel.GetNode<Button>("VBoxContainer/VideoBackButton");

        startButton.Pressed += OnStartPressed;
        settingsButton.Pressed += OnSettingsPressed;
        exitButton.Pressed += OnExitPressed;
        //audioButton.Pressed += OnAudioPressed;

        masterVolumeHslider.ValueChanged += OnMainVolumeSliderValueChanged;
        musicVolumeHslider.ValueChanged += OnMusicVolumeSliderValueChanged;
        sfxVolumeHslider.ValueChanged += OnSfxVolumeSliderValueChanged;
        audioButton.Pressed += OnAudioButtonPressed;
        audioBackButton.Pressed += OnAudioBackButtonPressed;
        settingsBackButton.Pressed += OnSettingsBackButtonPressed;
        fullscreenButton.Pressed += OnFullscreenPressed;
        borderlessButton.Pressed += OnBorderlessPressed;
        vsyncButton.Pressed += OnVsyncPressed;
        resolutionOptionButton.ItemSelected += OnResolutionChanged;
        videoButton.Pressed += OnVideoButtonPressed;
        videoBackButton.Pressed += OnVideoBackButtonPressed;
    }

    private void OnResolutionChanged(long index)
    {
        GD.Print("Resolution changed to index " + index.ToString());
        string res = resolutions[index];
        GD.Print("New Resolution: " + res);
        SetResolution(res);
    }

    private void SetResolution(string value)
    {
        Vector2I size = GetResolutionFromString(value);
        DisplayServer.WindowSetSize(size);
        data.Resolution = value;
        GameSettings.Save();
    }


    private void OnVideoButtonPressed()
    {
        settingsPanel.Visible = false;
        videoPanel.Visible = true;
    }


    private void OnAudioButtonPressed()
    {
        settingsPanel.Visible = false;
        audioPanel.Visible = true;
    }


    private void OnSfxVolumeSliderValueChanged(double value)
    {
        sfx_hslider.Value = value;
        data.SfxVolume = value;
        AudioServer.SetBusVolumeDb(sfx_index, Mathf.LinearToDb((float)value));
        GameSettings.Save();
    }


    private void OnMusicVolumeSliderValueChanged(double value)
    {
        music_hslider.Value = value;
        data.MusicVolume = value;
        AudioServer.SetBusVolumeDb(music_index, Mathf.LinearToDb((float)value));
        GameSettings.Save();
    }


    private void OnMainVolumeSliderValueChanged(double value)
    {
        master_hslider.Value = value;
        data.MasterVolume = value;
        AudioServer.SetBusVolumeDb(master_index, Mathf.LinearToDb((float)value));
        GameSettings.Save();
    }


    private void OnVideoBackButtonPressed()
    {
        settingsPanel.Visible = true;
        videoPanel.Visible = false;
    }


    private void OnVsyncPressed()
    {
        SetVsync(vsyncButton.ButtonPressed);
    }

    private void SetVsync(bool value)
    {
        if (value)
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
        else
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
    }


    private void OnBorderlessPressed()
    {
        SetBorderlessMode(borderlessButton.ButtonPressed);
    }

    private void SetBorderlessMode(bool value)
    {
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, value);
    }


    private void OnFullscreenPressed()
    {
        SetFullscreenMode(fullscreenButton.ButtonPressed);
    }

    private void SetFullscreenMode(bool value)
    {
        if (value)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }


    private void OnSettingsBackButtonPressed()
    {
        settingsPanel.Visible = false;
        mainMenuPanel.Visible = true;
    }


    /*private void OnAudioPressed()
    {
        audioPanel.Visible = true;
        settingsPanel.Visible = false;
    }*/


    private void OnStartPressed()
    {
        GD.Print("New Game started...");
        SceneManager.Instance.SwitchScene("GameCreation");
    }

    private void OnSettingsPressed()
    {
        mainMenuPanel.Visible = false;
        settingsPanel.Visible = true;
    }

    private void OnExitPressed()
    {
        GD.Print("Exit Game...");
        SceneManager.Instance.QuitGame();
    }

    private void OnAudioBackButtonPressed()
    {
        audioPanel.Visible = false;
        settingsPanel.Visible = true;
    }

    private Vector2I GetResolutionFromString(string value)
    {
        string[] strings = value.Split("x");
        int x = strings[0].ToInt();
        int y = strings[1].ToInt();
        return new Vector2I(x, y);
    }
}
