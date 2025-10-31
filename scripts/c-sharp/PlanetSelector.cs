using Godot;
using System;
using System.Linq;

[Tool] // Damit es im Editor läuft
public partial class PlanetSelector : Node
{
    [Export(PropertyHint.Enum, "")]
    private string planetName = "";

    public string PlanetName => planetName;

    public override void _Ready()
    {
        // Wenn das Spiel läuft, prüfe, ob der Planet gültig ist
        if (Engine.IsEditorHint()) return; // Nur im Editor aktiv
        if (string.IsNullOrEmpty(planetName)) return;

        var planet = PlanetData.GetPlanet(planetName);
        if (planet != null)
            GD.Print($"[PlanetSelector] Aktueller Planet: {planet.Name}");
    }

    // Wird im Editor ausgeführt, um das Dropdown dynamisch zu aktualisieren
    public override void _Notification(int what)
    {
        if (what == NotificationEditorPreSave)
        {
            UpdatePlanetEnum();
        }
    }

    private void UpdatePlanetEnum()
    {
        if (!Engine.IsEditorHint())
            return;

        // Lade alle Planeten aus dem Singleton
        var allNames = PlanetData.GetAllPlanets().Select(p => p.Name).ToArray();
        if (allNames.Length == 0)
            return;

        // Erstelle die Liste für das Dropdown (durch Kommas getrennt)
        string enumString = string.Join(",", allNames);

        // Wende PropertyHintEnum dynamisch an
        var exportInfo = GetPropertyList().FirstOrDefault(p => (string)p["name"] == "planetName");
        if (exportInfo.Count > 0)
        {
            // Das funktioniert, wenn PlanetData bereits geladen ist (Autoload aktiv)
            SetMeta("hint_string", enumString);
        }
    }
}

