using Godot;
using System;
using System.Collections.Generic;

public partial class SaveMenu : Control
{
    [Export] private VBoxContainer SlotList;
    [Export] private Button AddSlotButton;

    private List<string> availableSlots = new();

    public override void _Ready()
    {
        // Fallbacks (falls nicht im Editor gesetzt)
        SlotList ??= GetNode<VBoxContainer>("SlotList");
        AddSlotButton ??= GetNode<Button>("AddSlotButton");

        AddSlotButton.Pressed += OnAddSlotPressed;

        RefreshSlotList();
    }

    private void RefreshSlotList()
    {
        // Alte Buttons entfernen
        foreach (Node child in SlotList.GetChildren())
            child.QueueFree();

        availableSlots = SaveManager.GetAvailableSlots();

        if (availableSlots.Count == 0)
        {
            var noSavesLabel = new Label
            {
                Text = "Keine Speicherstände vorhanden.",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            SlotList.AddChild(noSavesLabel);
            return;
        }

        foreach (string slotName in availableSlots)
        {
            var data = SaveManager.LoadSlot(slotName);
            if (data == null)
                continue;

            string info = $"{slotName}  |  {data.PlayerName} ({data.PlayerClass})  " +
                          $"|  {data.Year:D4}-{data.Month:D2}  |  Credits: {data.Credits}  XP: {data.XP}";

            var slotButton = new Button
            {
                Text = info,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 40)
            };
            slotButton.AddThemeFontSizeOverride("font_size", 18);

            slotButton.Pressed += () => OnSlotSelected(slotName);
            SlotList.AddChild(slotButton);
        }
    }

    private void OnSlotSelected(string slotName)
    {
        GD.Print($"[SaveMenu] Slot '{slotName}' ausgewählt.");

        // Sauber über GameState laden
        bool success = GameState.Instance.LoadGame(slotName);
        if (!success)
        {
            GD.PrintErr($"[SaveMenu] Laden von Slot '{slotName}' fehlgeschlagen!");
            return;
        }

        var slotData = GameState.Instance.CurrentSave;

        GD.Print($"[SaveMenu] Geladen: {slotData.PlayerName}, Scene={slotData.CurrentScene}");

        // Fade-in/out über SceneManager, falls vorhanden
        if (SceneManager.Instance != null)
            SceneManager.Instance.SwitchScene(GetSceneAliasFromPath(slotData.CurrentScene));
        else
            GetTree().ChangeSceneToFile(slotData.CurrentScene);
    }

    private string GetSceneAliasFromPath(string scenePath)
    {
        foreach (var pair in SceneManager.Instance.Scenes)
        {
            if (pair.Value == scenePath)
                return pair.Key;
        }
        return "Game";
    }

    private void OnAddSlotPressed()
    {
        string newSlotName = GenerateNextSlotName();

        // Neues Spiel + Slot initialisieren
        GameState.Instance.NewGame($"Pilot {availableSlots.Count + 1}",
                                   Globals.Sex.CIS_MALE,
                                   Globals.PlayerClass.WORKER);

        // Aktuellen Spielstand speichern
        GameState.Instance.CurrentSlotName = newSlotName;
        GameState.Instance.SaveGame();

        GD.Print($"[SaveMenu] Neuer Slot '{newSlotName}' erstellt und gespeichert.");

        RefreshSlotList();
    }

    private string GenerateNextSlotName()
    {
        int i = 1;
        while (availableSlots.Contains($"Save{i:00}"))
            i++;
        return $"Save{i:00}";
    }
}
