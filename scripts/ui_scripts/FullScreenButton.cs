using Godot;
using System;
using System.Collections.Generic;

public partial class FullScreenButton : CheckBox
{
    public override void _Ready()
    {
        ButtonPressed = UserSettings.Instance.fullScreen;
        Toggled += ToggleFullscreen;

        if (UserSettings.Instance.fullScreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}    
    }

    public void ToggleFullscreen(bool toggle)
    {
        UserSettings.Instance.fullScreen = toggle;
        
        if (toggle)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }
}
