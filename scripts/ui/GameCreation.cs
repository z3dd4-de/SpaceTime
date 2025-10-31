using Godot;
using System;

public partial class GameCreation : Node2D
{
    private string playerName = "";
    private Globals.Sex sex;
    private float class_fl;
    private Globals.PlayerClass class_id;

    private Node2D choosePortrait;
    private CanvasLayer canvasLayer;

    public override void _Ready()
    {
        choosePortrait = GetNode<Node2D>("CanvasLayer3/ChoosePortrait");
        canvasLayer = GetNode<CanvasLayer>("CanvasLayer");

        var okButton = GetNode<Button>("CanvasLayer/StartPanel/VBoxContainer/OkButton");
        okButton.Disabled = true;

        float tmp = GetRandomStart();
        float i = Math.Abs(tmp);

        var classLabel = GetNode<Label>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/ClassPropLabel");

        if (i > 0 && i < 10)
        {
            classLabel.Text = "Miner";
            class_id = Globals.PlayerClass.MINER;
        }
        else if (i >= 10 && i < 20)
        {
            classLabel.Text = "Worker";
            class_id = Globals.PlayerClass.WORKER;
        }
        else if (i >= 20 && i < 40)
        {
            classLabel.Text = "Scientist";
            class_id = Globals.PlayerClass.SCIENTIST;
        }
        else if (i >= 40)
        {
            classLabel.Text = "Corporate";
            class_id = Globals.PlayerClass.CORPORATE;
        }

        // Signale verbinden
        var textEdit = GetNode<TextEdit>("CanvasLayer/StartPanel/VBoxContainer/TextEdit");
        textEdit.TextChanged += OnTextEditTextChanged;

        okButton.Pressed += OnOkButtonPressed;

        var maleCheck = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/MaleCheckBox");
        var femaleCheck = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/FemaleCheckBox");
        var otherCheck = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/OtherCheckBox");

        maleCheck.Toggled += OnMaleCheckBoxToggled;
        femaleCheck.Toggled += OnFemaleCheckBoxToggled;
        otherCheck.Toggled += OnOtherCheckBoxToggled;
    }

    private float GetRandomStart()
    {
        // randfn in GDScript = Normalverteilte Zufallszahl
        // Godot 4.5 bietet RandomNumberGenerator.RandomfRange() als Ersatz
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        return (float)rng.Randfn(0, 10);
    }

    private void OnTextEditTextChanged()
    {
        var textEdit = GetNode<TextEdit>("CanvasLayer/StartPanel/VBoxContainer/TextEdit");
        var okButton = GetNode<Button>("CanvasLayer/StartPanel/VBoxContainer/OkButton");

        if (!string.IsNullOrEmpty(textEdit.Text))
        {
            playerName = textEdit.Text;
            okButton.Disabled = false;
        }
        else
        {
            okButton.Disabled = true;
        }
    }

    private void OnOkButtonPressed()
    {
        GD.Print($"Spieler '{playerName}' erstellt als {sex}, Klasse {class_id}");

        GetNode<Control>("CanvasLayer/StartPanel").Visible = false;

        // Spielerobjekt erstellen
        Globals.Player.CreatePlayer(playerName, sex, class_id);

        canvasLayer.Visible = false;
        var choosePortrait = GetNode<ChoosePortrait>("CanvasLayer3/ChoosePortrait");
        choosePortrait.Sex = sex;        // setzt enum direkt

        choosePortrait.Set("visible", true);
        choosePortrait.Call("load_files");
        choosePortrait.Call("init_image");

    }

    private void OnMaleCheckBoxToggled(bool toggledOn)
    {
        if (!toggledOn) return;

        var male = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/MaleCheckBox");
        var female = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/FemaleCheckBox");
        var other = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/OtherCheckBox");

        male.ButtonPressed = true;
        female.ButtonPressed = false;
        other.ButtonPressed = false;

        sex = Globals.Sex.CIS_MALE;
    }

    private void OnFemaleCheckBoxToggled(bool toggledOn)
    {
        if (!toggledOn) return;

        var male = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/MaleCheckBox");
        var female = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/FemaleCheckBox");
        var other = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/OtherCheckBox");

        male.ButtonPressed = false;
        female.ButtonPressed = true;
        other.ButtonPressed = false;

        sex = Globals.Sex.CIS_FEMALE;
    }

    private void OnOtherCheckBoxToggled(bool toggledOn)
    {
        if (!toggledOn) return;

        var male = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/MaleCheckBox");
        var female = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/FemaleCheckBox");
        var other = GetNode<CheckBox>("CanvasLayer/StartPanel/VBoxContainer/GridContainer/VBoxContainer/OtherCheckBox");

        male.ButtonPressed = false;
        female.ButtonPressed = false;
        other.ButtonPressed = true;

        sex = Globals.Sex.DIVERS;
    }
}

