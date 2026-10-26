using Godot;
using System;

public partial class WorldManager : Node
{
    public static WorldManager Instance { get; private set; }

    public string worldPath;

    public WorldMetadata worldData;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
            SetProcess(false);
        }
        else
        {
            QueueFree();
        }
    }
}
