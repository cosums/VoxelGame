using Godot;
using System;
using System.Linq;

public partial class Main : Node
{
	[Export] public PackedScene serverScene;
	[Export] public PackedScene[] playerScenes;

	public override void _Ready()
	{
		if (OS.HasFeature("dedicated_server"))
		{
			StartServer();
		}
		else
		{
			StartPlayer();
		}
	}

	private void StartServer()
	{
		GD.Print("Launching as server...\n");
		NetworkManager.instance.isServer = true;
		NetworkManager.instance.StartServer();
		GetTree().CallDeferred("change_scene_to_packed", serverScene);
	}

	private void StartPlayer()
	{
		GD.Print("Launching as client...\n");
		
		SaveManager.Instance.LoadData();
		UserSettings.Instance.ManualLoad();

		int r = CheckRenderer();
		if (r == -1) return; // prevents fullscreen thing

		string savedVersion = UserSettings.Instance.version;
		string[] parts = savedVersion.Split('.');
		string majorMinor = parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : "";

		string currentVersion = (string)ProjectSettings.GetSetting("application/config/version");
		parts = currentVersion.Split('.');
		string curMajorMinor = parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : "";
		
		if (majorMinor == curMajorMinor)
		{
			// Run a terminal dialouge describing the last changes
			GetTree().CallDeferred("change_scene_to_packed", playerScenes[1]);
		} 
		else
		{
            UserSettings.Instance.version = (string)ProjectSettings.GetSetting("application/config/version");
			GetTree().CallDeferred("change_scene_to_packed", playerScenes[0]);
        }
	}

	private int CheckRenderer()
    {
        string rendererString = RenderingServer.GetCurrentRenderingMethod();
		int renderer = rendererString == "forward_plus" ? 0 : 1;

		if (renderer != UserSettings.Instance.renderer)
        {
            string exePath = OS.GetExecutablePath();
			string[] args = new string[] { "--rendering-driver", "opengl3"};
			OS.CreateProcess(exePath, args);
			GetTree().Quit();
			return -1;
        }

		return 0;
    }
}
