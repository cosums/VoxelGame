using Godot;
using System;

public partial class VersionLabel : Label
{
	private string version;
	
	public override void _Ready()
	{
		version = "0xDEC " + (string)ProjectSettings.GetSetting("application/config/version");
	}

	public override void _Process(double delta)
	{
		ulong bytes = OS.GetStaticMemoryUsage();
		float mb = bytes / (1024f * 1024f);

		Text = $"{version}\nMem: {mb:F2} MB\nFPS: " + Engine.GetFramesPerSecond();
    }

}
