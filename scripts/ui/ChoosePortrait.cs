using Godot;
using System;
using System.Collections.Generic;

public partial class ChoosePortrait : Node2D
{
    [Export] private Sprite2D personImageSprite;
    [Export] private TextureButton leftArrowButton;
    [Export] private RichTextLabel descriptionLabel;
    [Export] private TextureButton rightArrowButton;

    private string rootDir = "res://gfx/portraits";
    private List<string> files = new();
    private string filter = "";
    public Globals.Sex Sex = Globals.Sex.CIS_MALE;

    private int count = 0;
    private int currentImage = 0;

    public override void _Ready()
    {
        // Referenzen sichern (falls nicht über den Editor gesetzt)
        if (personImageSprite == null)
            personImageSprite = GetNode<Sprite2D>("PanelContainer/PersonImageSprite");
        if (leftArrowButton == null)
            leftArrowButton = GetNode<TextureButton>("PanelContainer/HBoxContainer/LeftArrowButton");
        if (descriptionLabel == null)
            descriptionLabel = GetNode<RichTextLabel>("PanelContainer/HBoxContainer/DescriptionLabel");
        if (rightArrowButton == null)
            rightArrowButton = GetNode<TextureButton>("PanelContainer/HBoxContainer/RightArrowButton");

        leftArrowButton.Disabled = true;
        rightArrowButton.Disabled = true;

        // Event-Verbindungen
        leftArrowButton.Pressed += OnLeftArrowButtonPressed;
        rightArrowButton.Pressed += OnRightArrowButtonPressed;

        LoadFiles();
        if (count > 0)
            InitImage();
    }

    private void InitImage()
    {
        LoadFileToPortrait(currentImage);
        CheckButtons();
    }

    private void LoadFileToPortrait(int index)
    {
        if (count > 0 && index >= 0 && index < count)
        {
            string path = $"{rootDir}/{files[index]}";
            using var image = Image.LoadFromFile(path);
            if (image == null)
            {
                GD.PrintErr($"Bild konnte nicht geladen werden: {path}");
                return;
            }

            var texture = ImageTexture.CreateFromImage(image);
            string desc = files[index].Replace("small.png", "").Replace("_", " ");
            descriptionLabel.Text = desc;
            personImageSprite.Texture = texture;
        }
    }

    private void CheckButtons()
    {
        if (count >= 1)
        {
            leftArrowButton.Disabled = (currentImage == 0);
            rightArrowButton.Disabled = (currentImage == count - 1);
        }
        else
        {
            leftArrowButton.Disabled = true;
            rightArrowButton.Disabled = true;
        }
    }

    private void SetFilter()
    {
        GD.Print($"Filter Sex: {Sex}");
        if (Sex == Globals.Sex.CIS_MALE || Sex == Globals.Sex.MALE_GAY || Sex == Globals.Sex.TRANS_MALE)
            filter = "male";
        else if (Sex == Globals.Sex.CIS_FEMALE || Sex == Globals.Sex.FEMALE_GAY || Sex == Globals.Sex.TRANS_FEMALE)
            filter = "female";
        else
            filter = "";

        GD.Print($"Filter: {filter}");
    }

    public void LoadFiles()
    {
        files.Clear();
        SetFilter();

        using var dir = DirAccess.Open(rootDir);
        if (dir == null)
        {
            GD.PrintErr("Fehler beim Öffnen des Portrait-Verzeichnisses.");
            count = 0;
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir())
            {
                if (fileName.Contains("_small") && !fileName.Contains(".import"))
                {
                    if (filter == "male")
                    {
                        if (!fileName.Contains("female") && !fileName.Contains("andro"))
                            files.Add(fileName);
                    }
                    else if (filter == "female")
                    {
                        if (fileName.Contains("female"))
                            files.Add(fileName);
                    }
                    else
                    {
                        files.Add(fileName);
                    }
                }
            }
            fileName = dir.GetNext();
        }

        dir.ListDirEnd();

        count = files.Count;
        if (count > 0)
            currentImage = 0;
    }

    private void OnLeftArrowButtonPressed()
    {
        currentImage = Mathf.Max(0, currentImage - 1);
        CheckButtons();
        LoadFileToPortrait(currentImage);
    }

    private void OnRightArrowButtonPressed()
    {
        currentImage = Mathf.Min(count - 1, currentImage + 1);
        CheckButtons();
        LoadFileToPortrait(currentImage);
    }

    public void OnOkButtonPressed()
    {
        if (Globals.Player == null)
        {
            GD.PrintErr("Kein Player-Objekt vorhanden!");
            return;
        }

        // Portrait speichern (z. B. Pfad oder Texture2D)
        Globals.Player.PortraitPath = $"{rootDir}/{files[currentImage]}";

        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.SwitchScene("GameCreation2");
        }
        else
        {
            GD.PrintErr("SceneManager-Instanz nicht gefunden!");
        }
    }

}

