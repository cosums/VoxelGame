using Godot;
using System;

public partial class MainMenuControllerv2 : Node3D
{
    public override void _Ready()
    {
        if (UserSettings.Instance.fullScreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
    }
}
