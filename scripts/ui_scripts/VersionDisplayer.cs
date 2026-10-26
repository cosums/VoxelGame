using Godot;
using System;

public partial class VersionDisplayer : Label
{
    public override void _Ready()
	{
		Text = "v" + (string)ProjectSettings.GetSetting("application/config/version");
	}
}
