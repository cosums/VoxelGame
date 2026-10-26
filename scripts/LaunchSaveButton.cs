using Godot;
using System;

public partial class LaunchSaveButton : Button
{
    public SaveContainer world;
    [Export] string savePath = "user://saves/";
    [Export] PackedScene levelScene;

    public override void _Ready()
    {
        Pressed += LaunchWorld;
    }

    public void SetSelectedWorld(SaveContainer world)
    {
        this.world = world;
        Disabled = false;
    }

    public void RemoveSelectedWorld()
    {
        world = null;
        Disabled = true;
    }

    private void LaunchWorld()
    {
        string worldFolder = savePath + world.metaData.world.name + "/";
        WorldManager.Instance.worldPath = worldFolder;
        WorldManager.Instance.worldData = world.metaData;

        GetTree().ChangeSceneToPacked(levelScene);
    }
}
