using Godot;
using System;

public partial class SaveController : VBoxContainer
{
    public SaveContainer selectedWorld;

    [Export] DeleteSaveButton deleteButton;
    [Export] LaunchSaveButton launchButton;


    public void SetSelectedWorld(SaveContainer world)
    {
        selectedWorld = world;
        deleteButton.SetSelectedWorld(world);
        launchButton.SetSelectedWorld(world);
    }
}
