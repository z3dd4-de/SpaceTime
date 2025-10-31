using Godot;
using System;
using System.Linq;

# if TOOLS
[Tool]
public partial class PlanetSelectorPlugin : EditorPlugin
{
    private EditorInspectorPlugin inspectorPlugin;

    public override void _EnterTree()
    {
        inspectorPlugin = new PlanetInspectorPlugin();
        AddInspectorPlugin(inspectorPlugin);
        GD.Print("[PlanetSelectorPlugin] Editor-Plugin aktiviert.");
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(inspectorPlugin);
        GD.Print("[PlanetSelectorPlugin] Editor-Plugin deaktiviert.");
    }
}

[Tool]
public partial class PlanetInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj)
    {
        // Wir unterstützen PlanetSelector Nodes
        return obj is PlanetSelector;
    }

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (obj is PlanetSelector selector && name == "planetName")
        {
            AddPropertyEditor(name, CreatePlanetDropdown(selector));
            return true; // Diese Property wurde behandelt
        }

        return false; // Alle anderen standardmäßig anzeigen
    }

    private Control CreatePlanetDropdown(PlanetSelector selector)
    {
        var box = new HBoxContainer();
        var label = new Label { Text = "Planet Name:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var dropdown = new OptionButton();

        // Lade aktuelle Planetennamen aus dem Singleton
        var planets = PlanetData.GetAllPlanets().Select(p => p.Name).ToList();
        foreach (var name in planets)
        {
            dropdown.AddItem(name);
        }

        // Setze aktuellen Wert, falls vorhanden
        var currentIndex = planets.IndexOf(selector.PlanetName);
        if (currentIndex >= 0)
            dropdown.Select(currentIndex);

        // Event: Auswahl im Dropdown geändert
        dropdown.ItemSelected += (index) =>
        {
            var newName = planets[(int)index];
            selector.Set("planetName", newName);
            selector.NotifyPropertyListChanged();
        };

        box.AddChild(label);
        box.AddChild(dropdown);

        return box;
    }
}
# endif

