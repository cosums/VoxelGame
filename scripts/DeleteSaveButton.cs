using Godot;
using System;

public partial class DeleteSaveButton : Button
{
    public SaveContainer world = null;
    [Export] SavesReader savesReader;
    [Export] string savePath = "user://saves/";

    public override void _Ready()
    {
        Pressed += DeleteWorld;
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

    private void DeleteWorld()
    {
        if (world == null)
            return;

        string worldFolder = savePath + world.metaData.world.name + "/";

        DeleteDirectoryRecursive(worldFolder);

        GD.Print($"Deleted world: {world.metaData.world.name}");

        RemoveSelectedWorld();
        savesReader.LoadSaves();
    }

    private void DeleteDirectoryRecursive(string folder)
    {
        using DirAccess dir = DirAccess.Open(folder);

        if (dir == null)
            return;

        dir.ListDirBegin();

        string item;
        while ((item = dir.GetNext()) != "")
        {
            string fullPath = folder + item;

            if (dir.CurrentIsDir())
            {
                DeleteDirectoryRecursive(fullPath + "/");
            }
            else
            {
                DirAccess.RemoveAbsolute(fullPath);
            }
        }

        dir.ListDirEnd();
        DirAccess.RemoveAbsolute(folder);
    }
}
