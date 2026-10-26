using Godot;
using System;

public partial class TerminateButton : Button
{
    //[Export] PackedScene mainMenuScene;
    //[Export] string mainMenuPath = "res://scenes/ui/main_menu.tscn";
    
    public override void _Ready()
    {
        Pressed += Quit;
    }

    private void Quit()
    {
        //GetTree().CallDeferred("change_scene_to_packed", mainMenuScene);
        //GetTree().ChangeSceneToFile(mainMenuPath);
        //GetTree().CallDeferred("change_scene_to_file", mainMenuPath);
        //GetTree().Root.FindChild("Settings").QueueFree();

        // yeah this is ugly but I don't care
        GetParent().GetParent().GetParent().GetParent().GetParent().QueueFree(); 
        // so beautiful i cant wait to come across this again someday
    }
}
