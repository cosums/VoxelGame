using Godot;
using System;
using System.Text.Json;

public partial class SavesReader : VBoxContainer
{
    [Export] string savePath = "user://saves/";
    [Export] PackedScene worldButton;

    [Export] SaveController saveController;

    public override void _Ready()
    {
        LoadSaves();
    }

    public void LoadSaves()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
        
        DirAccess.MakeDirRecursiveAbsolute(savePath);

        using DirAccess dir = DirAccess.Open(savePath);
        if (dir == null)
            return;

        dir.ListDirBegin();
        string folderName = dir.GetNext();

        while (folderName != "")
        {
            if (dir.CurrentIsDir() && folderName != "." && folderName != "..")
            {
                string worldFolder = savePath + folderName + "/";
                string metadataPath = worldFolder + "world.json";

                if (FileAccess.FileExists(metadataPath))
                {
                    try
                    {
                        var file = FileAccess.Open(metadataPath, FileAccess.ModeFlags.Read);
                        string json = file.GetAsText();
                        file.Close();

                        var metadata = JsonSerializer.Deserialize<WorldMetadata>(json);
                        CreateWorldButton(metadata);
                    }
                    catch (Exception e)
                    {
                        GD.PushError($"Failed to read world metadata: {e.Message}");
                    }
                }
            }
            folderName = dir.GetNext();
        }
    }

    private void CreateWorldButton(WorldMetadata data)
    {
        SaveContainer node = (SaveContainer)worldButton.Instantiate();

        node.SetData(data);
        node.saveController = saveController;

        AddChild(node);
    }
}