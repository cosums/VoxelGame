using Godot;
using System;

public partial class Terminal : Node
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
