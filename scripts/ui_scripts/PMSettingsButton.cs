using Godot;
using System;

public partial class PMSettingsButton : Button
{
    [Export] public string settingsPath = "res://scenes/ui/settings_v2.tscn";
    
    public override void _Ready()
    {
        Pressed += OnButtonPressed;
    }

    public void OnButtonPressed()
    {
        GetTree().Root.AddChild(GD.Load<PackedScene>(settingsPath).Instantiate());
    }
}
