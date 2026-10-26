using Godot;
using System;
using System.Text.Json;

public partial class CreateButton : Button
{
    [Export] string savePath = "user://saves/";
    [Export] SavesReader savesReader;

    public override void _Ready()
    {
        Pressed += CreateInstance;
    }


    private void CreateInstance()
    {
        DirAccess.MakeDirRecursiveAbsolute(savePath);

        string worldName = $"World {Guid.NewGuid().ToString().Substring(0, 8)}";
        string worldFolder = savePath + worldName + "/";

        DirAccess.MakeDirRecursiveAbsolute(worldFolder);

        var metaData = new
        {
            world = new
            {
                id = Guid.NewGuid().ToString(),
                name = worldName,
                layer = "surface",
                version = "v" + (string)ProjectSettings.GetSetting("application/config/version"),
            },
            generation = new
            {
                seed = (int)GD.Randi(),
                type = 0
            },
            timestamps = new
            {
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                lastAccessed = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }
        };

        string json = JsonSerializer.Serialize(metaData, new JsonSerializerOptions{ WriteIndented = true});

        FileAccess file = FileAccess.Open(worldFolder + "world.json", FileAccess.ModeFlags.Write);
        file.StoreString(json);
        file.Close();

        GD.Print($"Created world: {worldName}");

        if (savesReader != null)
        {
            savesReader.LoadSaves();
        }
    }
}
