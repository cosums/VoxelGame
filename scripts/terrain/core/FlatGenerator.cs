using Godot;
using System;

public partial class FlatGenerator : Node
{
    [Export] public Node3D playerNode;
    
    [Export] public int viewDistance;
    [Export] public int lodRings;
    [Export] public int ringDistance;

    [Export] public int seed;
    [Export] public bool randomSeed = true;
    [Export] public int maxChunkThreads = 4;
    [Export] public bool useSurfaceNets = true;

    private WorldMetadata worldMetaData;
    private ChunkPooler chunkPool;
    private ChunkLoader chunkLoader;
    private TerrainField terrainField;
    private FlatTerrainManager terrainManager;

    public override void _Ready()
    {
        GD.Print("Loading world with path: " + WorldManager.Instance.worldPath);
        worldMetaData = WorldManager.Instance.worldData;
        seed = WorldManager.Instance.worldData.generation.seed;

        GrabUserSettings();

        chunkPool = new ChunkPooler(this);
        terrainField = new TerrainField(seed);

        NativeTerrain.Init(seed);

        chunkLoader = new ChunkLoader(chunkPool, terrainField, maxChunkThreads);

        terrainManager = new FlatTerrainManager(playerNode, ConstructLODs(), chunkLoader);
        terrainManager.Name = "Terrain Manager";
        AddChild(terrainManager);

        GD.Print("World Initialized.");
    }

    private void GrabUserSettings()
    {
        viewDistance = UserSettings.Instance.viewDistance;
        lodRings = UserSettings.Instance.ringCount;
        ringDistance = UserSettings.Instance.ringDistance;
        maxChunkThreads = UserSettings.Instance.threadCount;
        UserSettings.Instance.ScrubbyScrubby();
    }
    
    private int[] ConstructLODs()
    {
        int[] lods = new int[lodRings + 1];
        lods[0] = viewDistance;
        for (int i = 1; i <= lodRings; i++)
        {
            lods[i] = ringDistance;
        }

        return lods;
    }


    public void UpdateSettings()
    {
        GrabUserSettings();
        Reset();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_reload"))
        {
            Reset();
        }
    }

    public void Reset()
    {
        terrainManager.Reset(ConstructLODs(), maxChunkThreads, seed);
    }
}
