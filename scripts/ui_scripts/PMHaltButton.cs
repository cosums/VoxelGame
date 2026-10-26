using Godot;
using System;

public partial class PMHaltButton : Button
{
    [Export] string mainMenuPath = "res://scenes/ui/main_menu.tscn";

    public override void _Ready()
    {
        Pressed += OnHaltPressed;
    }

    private void OnHaltPressed()
    {
        GetTree().CallDeferred("change_scene_to_file", mainMenuPath);
    }
}
