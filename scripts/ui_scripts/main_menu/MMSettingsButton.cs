using Godot;
using System;

public partial class MMSettingsButton : MainMenuButton
{
    [Export] public string settingsPath = "res://scenes/ui/settings_v2.tscn";
    
    public override void _Ready()
    {
        base._Ready();
        Pressed += OnButtonPressed;
    }

    public void OnButtonPressed()
    {
        GetTree().Root.AddChild(GD.Load<PackedScene>(settingsPath).Instantiate());
    }
}
