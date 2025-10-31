using Godot;
using System;

/*
Zu verwenden, um Speicherstände in einer Szene zu verwenden.
GameState ist im Autoload definiert.
*/
public partial class GameTemplate : Node
{
    private SaveSlot currentData;

    public override void _Ready()
    {
        string slotName = GameState.Instance.CurrentSlotName;

        if (string.IsNullOrEmpty(slotName))
        {
            GD.PrintErr("Kein Speicherstand gesetzt! Lade Default-Daten...");
            currentData = new SaveSlot();
        }
        else
        {
            currentData = SaveManager.LoadSlot(slotName);
            GD.Print($"Spiel geladen von Slot '{slotName}': XP={currentData.XP}, Credits={currentData.Credits}");
        }

        // Beispiel: Änderungen speichern
        currentData.Credits += 50;
        SaveManager.SaveSlot(slotName, currentData);
    }

    public override void _ExitTree()
    {
        GameState.AutoSave();
        base._ExitTree();
    }

}
