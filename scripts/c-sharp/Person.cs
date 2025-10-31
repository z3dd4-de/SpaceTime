using Godot;

public partial class Person : Node
{
    // öffentliche Properties (für Serialisierung / Zugriff)
    public string PersonName { get; set; } = "";
    public float Age { get; set; } = 20.0f;
    public Genome Genome { get; set; } = null;
    public Globals.Sex Sex { get; set; } = Globals.Sex.CIS_FEMALE;
    public string Birthdate { get; set; } = "2480-03-01";

    public Person() { Genome = new Genome(); }

    public Person(string name, Globals.Sex sex, float age = 20.0f, string birthdate = "2480-03-01")
    {
        PersonName = name;
        Sex = sex;
        Age = age;
        Birthdate = birthdate;
        Genome = new Genome();
    }

    public override string ToString()
    {
        return $"{PersonName} ({Sex}), Age={Age}, Birthdate={Birthdate}";
    }
}
