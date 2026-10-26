using Godot;
using System;

public partial class SaveContainer : Button
{
    public WorldMetadata metaData;

    public SaveController saveController;

    [Export] Label name, version, accessed, created;

    public override void _Ready()
    {
        Pressed += SelectSave;
    }

    public void SetData(WorldMetadata data)
    {
        metaData = data;
        
        name.Text = data.world.name;
        version.Text = data.world.version;
        accessed.Text = data.timestamps.lastAccessed.ToString();
        created.Text = data.timestamps.created.ToString();
    }

    private void SelectSave()
    {
        GD.Print("Save was selected.");
        saveController.SetSelectedWorld(this);
    }

}
