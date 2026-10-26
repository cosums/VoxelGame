using Godot;
using System;

public partial class MMLaunchButton : MainMenuButton
{
    //[Export] PackedScene gameScene;
    [Export] string scenePath = "res://scenes/final_generators/freecam_flat.tscn";
    
    public override void _Ready()
    {
      base._Ready();
      Pressed += OnLaunchPressed;
    }

    private void OnLaunchPressed()
    {
		  //GetTree().CallDeferred("change_scene_to_packed", gameScene);
      //GetTree().ChangeSceneToFile(scenePath);
      GetTree().CallDeferred("change_scene_to_file", scenePath);
    }
}
