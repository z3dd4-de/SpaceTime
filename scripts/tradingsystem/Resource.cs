// Resource.cs - Ressourcen-Definition
using Godot;
using System;

public partial class Resource : GodotObject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int BasePrice { get; set; } = 100;
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    
    public Resource() { }
    
    public Resource(string id, string name, int basePrice, string category = "", string description = "")
    {
        Id = id;
        Name = name;
        BasePrice = basePrice;
        Category = category;
        Description = description;
    }
}
