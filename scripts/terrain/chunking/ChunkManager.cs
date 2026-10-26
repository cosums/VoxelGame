using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Godot.Collections;

public static class NativeTerrain
{
    static GodotObject gen;
    private static bool _setup = false;

    public static void Init(int seed)
    {
        gen = ClassDB.Instantiate("ChunkGenerator").AsGodotObject();
        gen.Call("set_seed", seed);
        _setup = true;
    }

    public static Godot.Collections.Dictionary GenerateChunk(Vector3 pos, int scale, bool lod) {
        if (!_setup)
        {
            GD.PushError("[NativeTerrain]: Not setup!");
            return null;
        }
        return gen.Call("generate_chunk", pos, scale, lod).AsGodotDictionary();
    }
}

public partial class ChunkManager : Node3D
{
    public int chunkScale { get; private set; }
    public bool isLodChunk { get; private set; }
    
    public event Action<ChunkManager> OnReady;
    public event Action<ChunkManager> OnActivated;
    public event Action<ChunkManager> OnUnloaded;
    
    private ChunkData chunkData;
    private ChunkMesher chunkMesher;

    public ChunkState State;


    // OLD
    public ChunkManager ParentChunk;
    public List<ChunkManager> ChildChunks = new();

    private string dataPath;


    public void Initialize(int chunkScale, bool isLodChunk)
    {
        this.chunkScale = chunkScale;
        this.isLodChunk = isLodChunk;
        Visible = false;
        State = ChunkState.Queued;
        Name = $"LOD_{chunkScale}_{Position / WorldGenUtility.chunkSize}";

        dataPath = $"{chunkScale}_{Position / WorldGenUtility.chunkSize}";
    }

    public async void BuildAsync(TerrainField terrainField, CancellationToken ct = default)
    {
        if (State == ChunkState.Loading) return;
        State = ChunkState.Loading;

        CreateWaterMesh();

        Vector3 positionSnapshot = Position;

        Dictionary meshDict;

        try
        {
            meshDict = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return NativeTerrain.GenerateChunk(positionSnapshot, chunkScale, isLodChunk);
            }, ct);
        }
        catch (OperationCanceledException)
        {
            GD.Print($"Chunk {Name} build cancelled.");
            return;
        }

        if (ct.IsCancellationRequested || State == ChunkState.Unloading)
        {
            meshDict?.Clear();
            return;
        }

        CallDeferred(nameof(ApplyMesh), meshDict);  // do cancellation before push
    }
    
    private void ApplyMesh(Dictionary meshDict)
    {
        // Reuse children if possible
        MeshInstance3D chunkMesh = GetNodeOrNull<MeshInstance3D>("Chunk Mesh");
        if (chunkMesh == null)
        {
            chunkMesh = new MeshInstance3D();
            chunkMesh.Name = "Chunk Mesh";
            AddChild(chunkMesh);
        }

        Vector3[] verts = meshDict["vertices"].AsVector3Array();
        Vector3[] normals = meshDict["normals"].AsVector3Array();

        var mats = meshDict["material_indices"].AsGodotDictionary();

        ArrayMesh arrayMesh = new ArrayMesh();

        foreach (var kvp in mats)
        {
            byte materialId = (byte)kvp.Key.AsInt32();
            int[] matIndices = kvp.Value.AsInt32Array();

            if (verts.Length == 0 || matIndices.Length == 0)
                continue;

            Godot.Collections.Array surfaceArray = [];
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts;
            surfaceArray[(int)Mesh.ArrayType.Index] = matIndices;

            if (normals.Length == verts.Length)
            {
                surfaceArray[(int)Mesh.ArrayType.Normal] = normals;
            }

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

            int surfaceIndex = arrayMesh.GetSurfaceCount() - 1;

            if (MaterialLibrary.Materials.TryGetValue(materialId, out var material))
            {
                arrayMesh.SurfaceSetMaterial(surfaceIndex, material);
            }
        }

        chunkMesh.Mesh = arrayMesh;

        if (!isLodChunk)
        {
            // StaticBody3D body = GetNodeOrNull<StaticBody3D>("Chunk Collider");
            // if (body == null)
            // {
            //     body = new StaticBody3D();
            //     body.Name = "Chunk Collider";
            //     AddChild(body);
            // }

            // CollisionShape3D collider = body.GetNodeOrNull<CollisionShape3D>("Collision Shape");
            // if (collider == null)
            // {
            //     collider = new CollisionShape3D();
            //     collider.Name = "Collision Shape";
            //     body.AddChild(collider);
            // }

            // ConcavePolygonShape3D colliderMesh = new ConcavePolygonShape3D();
            // colliderMesh.SetFaces(meshData.collisionVerts.ToArray());
            // collider.Shape = colliderMesh;
        }

        // Clear for GC
        meshDict.Clear();
        chunkMesher = null;

        State = ChunkState.Ready;
        Activate(); // let chunks do this for now
        OnReady?.Invoke(this);
    }

    public void Activate()
    {
        if (State != ChunkState.Ready) return;
        State = ChunkState.Active;
        Visible = true;
        OnActivated?.Invoke(this);
    }

    public void ReleaseToPool()
    {
        State = ChunkState.Unloading;
        Unload();
    }

    public void DeformGlobal(Vector3 globalPoint, int radius, float delta)
    {
        FlatGenerator controller = GetParent<FlatGenerator>();
        FlatTerrainManager manager = controller.GetNodeOrNull<FlatTerrainManager>("Terrain Manager");
        manager.ApplyDeform(globalPoint, radius, delta);
    }

    public void DeformLocal(Vector3 globalPoint, int radius, float delta)
    {
        Vector3 localPoint = ToLocal(globalPoint);
        
        int dirty = chunkData.Deform(localPoint, radius, delta);
        if (dirty == 1)
        {
            RebuildMesh();
        }
    }

    private async void RebuildMesh()
    {
        if (chunkMesher == null)
        {
            chunkMesher = new ChunkMesher(chunkScale, true);
        }

        //ArrayMeshData meshData = await Task.Run(() => chunkMesher.GenerateMesh(chunkData, isLodChunk));
        //ApplyMesh(meshData);
        State = ChunkState.Active;
    }

    private void CreateWaterMesh()
    {
        WaterGenerator water = new WaterGenerator(chunkScale);
        ArrayMesh waterArrMesh = water.GenerateMesh();
        MeshInstance3D waterMesh = new MeshInstance3D();
        waterMesh.Mesh = waterArrMesh;
        StandardMaterial3D waterMat = ResourceLoader.Load<StandardMaterial3D>("res://materials/other/water.tres");
        waterMesh.MaterialOverride = waterMat;
        waterMesh.Name = "Water";
        AddChild(waterMesh);
    }
    
    private void Unload()
    {
        chunkData = null;
        chunkMesher = null;
        
        var meshCollider = GetNodeOrNull<Node>("Chunk Collider");
        if (meshCollider != null)
        {
            meshCollider.QueueFree();
        }

        var water = GetNodeOrNull<Node>("Water");
        if (water != null)
        {
            water.QueueFree();
        }

        State = ChunkState.Pooled;
        OnUnloaded?.Invoke(this);
    }
}
