using Godot;
using System;
using System.Linq;

public partial class SolarSystem : Node3D
{
	public static SolarSystem Instance { get; private set; }
	private Vector2 mouse = Vector2.Zero;
    private const float DIST = 1000f;
	public Planet[] Planets { get; private set; }

    // GUI
    [Export] private Panel planetInfoPanel;
    [Export] private RichTextLabel titelLabel;
	[Export] private RichTextLabel nameLabel;
	[Export] private RichTextLabel inhabitLabel;
	[Export] private RichTextLabel diameterLabel;
	[Export] private RichTextLabel distanceToParentLabel;
	[Export] private RichTextLabel distanceLabel;
	[Export] private RichTextLabel orbitLabel;
	[Export] private RichTextLabel dayLabel;
	[Export] private RichTextLabel moonLabel;
	[Export] private RichTextLabel colonieLabel;
	[Export] private RichTextLabel miningLabel;

	[Export] private Button okButton;
	//[Export] private PlanetSelector planetSelector;

	// Dauer der Ein-/Ausblendung in Sekunden
	[Export] private float fadeDuration = 0.3f;
	// Wird für laufende Animationen genutzt
	private Tween currentTween;
	private HUD _hud;
	private Node3D _targetPlanet;

	public override void _Ready()
	{
		Instance = this;

		Planets = this.GetChildren().Where(x => x is Planet).Cast<Planet>().ToArray();

		var orderedPlanets = Planets.OrderBy(planet => planet.HowManyParents());
		foreach (var planet in orderedPlanets)
		{
			planet.Init();
		}

		// Button mit HidePanel verbinden
		okButton.Pressed += HidePanel;

		// Panel beim Start unsichtbar machen
		planetInfoPanel.Visible = false;
		planetInfoPanel.Modulate = new Color(1, 1, 1, 0); // komplett transparent
		_hud = GetTree().Root.GetNode<HUD>("SolarSystem/HUD");
	}
	
	public void ShowPlanetPanel(string planetName)
	{
		var info = PlanetData.GetPlanet(planetName);
		if (info == null) return;

		nameLabel.Text = info.Name;
		diameterLabel.Text = $"{info.DiameterKm:N0} km";
		distanceLabel.Text = $"{info.DistanceToParentKm / 1_000_000:N1} Mio. km";
		orbitLabel.Text = $"{info.OrbitPeriodDays:N0} days";
		dayLabel.Text = $"{info.DayLengthHours:N1} hours";
	}


	public void ShowPanel(string value)
	{
		nameLabel.Text = value;
		var info = PlanetData.GetPlanet(value);
        if (info == null) return;
        GD.Print($"Planet/Moon:  {info.IsPlanet}/{info.IsMoon}");
        if (info.IsMoon) titelLabel.Text = "Moon info:";
        else if (info.IsPlanet) titelLabel.Text = "Planet info:";
        else titelLabel.Text = "Planet info:";

		nameLabel.Text = info.Name;
		diameterLabel.Text = $"{info.DiameterKm:N0} km";
		distanceToParentLabel.Text = $"Distance to {info.ParentName}";
		distanceLabel.Text = $"{info.DistanceToParentKm / 1_000_000:N1} Mio. km";
		orbitLabel.Text = $"{info.OrbitPeriodDays:N0} days";
		dayLabel.Text = $"{info.DayLengthHours:N1} hours";
		moonLabel.Text = $"{info.Moons:N0} ";
		colonieLabel.Text = $"{info.Colonized} ";
		miningLabel.Text = $"{info.Mining} ";

        // Falls ein Tween noch läuft → abbrechen
        currentTween?.Kill();

        planetInfoPanel.Visible = true;

        // Neues Tween erstellen
        currentTween = CreateTween();
        currentTween.TweenProperty(
            planetInfoPanel,
            "modulate:a", // Alpha-Kanal
            1.0f,         // Zielwert
            fadeDuration  // Dauer
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
	}
	
	public void HidePanel()
    {
        currentTween?.Kill();

        // Neues Tween für Fade-Out
        currentTween = CreateTween();
        currentTween.TweenProperty(
            planetInfoPanel,
            "modulate:a",
            0.0f,
            fadeDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);

        // Wenn fertig → Panel unsichtbar machen
        currentTween.Finished += () =>
        {
            planetInfoPanel.Visible = false;
        };
    }

	public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            mouse = mouseMotion.Position;
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            if (!mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                GetMouseWorldPos(mouse);
            }
        }
    }

    private void GetMouseWorldPos(Vector2 pos)
    {
        var camera = GetViewport().GetCamera3D();
        if (camera == null)
        {
            GD.PrintErr("Keine aktive Kamera gefunden!");
            return;
        }

        var space = GetWorld3D().DirectSpaceState;

        var start = camera.ProjectRayOrigin(pos);
        var end = camera.ProjectPosition(pos, DIST);

        var query = PhysicsRayQueryParameters3D.Create(start, end);
        var result = space.IntersectRay(query);

        if (result.Count > 0)
        {
            var position = (Vector3)result["position"];
			var collider = (Node3D)result["collider"];
			_targetPlanet = collider;

            GD.Print($"Treffer bei: {position}");

            if (collider != null)
            {
				GD.Print($"Getroffener Node: {collider.Name}");
				_hud.SetTarget(_targetPlanet);
				if (collider.Name != "Sun" && collider.Name != "")
                {
					ShowPanel(collider.Name);
                }
				
                if (collider.GetParent() != null)
                    GD.Print($"Root-Node: {collider.GetParent().Name}");
            }
            else
            {
                GD.Print("Kein Collider-Node gefunden.");
            }
        }
        else
        {
            GD.Print("Kein Treffer.");
        }
    }
}
