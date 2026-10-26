using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public partial class FlatTerrainManager : Node
{
    private Node3D playerNode;
    private ChunkLoader chunkLoader;

    private Vector2I lastPlayerChunk;
    private bool hasLastChunk = false;

    private int[] lodThresholds;
    private int maxChunkThreads;

    public readonly Dictionary<(Vector2I, int), ChunkManager> activeChunks = new();
    private readonly HashSet<(Vector2I, int)> chunksNeeded = new();
    private readonly List<(Vector2I, int)> chunksToRemove = new();

    public FlatTerrainManager(Node3D playerNode, int[] lodThresholds, ChunkLoader chunkLoader)
    {
        this.playerNode = playerNode;
        this.lodThresholds = lodThresholds;
        this.chunkLoader = chunkLoader;
    }

    public override void _Process(double delta)
    {
        Vector3 playerPosition = playerNode.GlobalPosition;
        Vector2I currentPlayerChunk = WorldGenUtility.WorldToLod(playerPosition, 1);

        if (!hasLastChunk || currentPlayerChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentPlayerChunk;
            hasLastChunk = true;

            GameManager.Instance.chunkInfo = currentPlayerChunk.ToString();
            UpdateChunks(playerPosition);
        }

        chunkLoader.TryProcessNext();
    }
    
    /// <summary>
    /// Allows FlatTerrainManager to reset all loaded chunks.
    /// </summary>
    /// <param name="lodThresholds"></param>
    public void Reset(int[] lodThresholds, int newThreadCount, int newSeed)
    {
        this.lodThresholds = lodThresholds;
        foreach (var kvp in activeChunks)
        {
            ChunkManager chunk = kvp.Value;
            if (chunk != null)
            {
                chunk.ReleaseToPool();
            }
        }

        activeChunks.Clear();
        chunkLoader.Clear(newThreadCount);
        chunkLoader.SetSeed(newSeed);
        NativeTerrain.Init(newSeed);
        hasLastChunk = false;

        GD.Print("Terrain reset complete.");
    }

    // For use with terrain deformation or other methods
    public bool TryGetChunk(Vector2I coord, out ChunkManager chunk, int resolution = 1)
    {
        return activeChunks.TryGetValue((coord, resolution), out chunk);
    }

    public void ApplyDeform(Vector3 point, int radius, float delta)
    {
        Vector2I baseChunk = WorldGenUtility.WorldToLod(point, 1);

        foreach (var offset in WorldGenUtility.chunkNeighbors)
        {
            Vector2I neighbor = baseChunk + offset;
            if (TryGetChunk(neighbor, out var chunk))
            {
                if (chunk != null)
                    chunk.DeformLocal(point, radius, delta);
            }
        }
    }

    private void UpdateChunks(Vector3 playerPosition)
    {
        chunksNeeded.Clear();
        int innerRadius = 0;

        for (int i = 0; i < lodThresholds.Length; i++)
        {
            int chunkScale = (int)Mathf.Pow(2, i);
            int outerRadius = innerRadius + lodThresholds[i];
            int outerRadiusAdjustment = 0;

            // We must account for if a ring is even or odd,
            // otherwise it will overlap and cause fun issues!
            if (i + 1 < lodThresholds.Length)
            {
                outerRadiusAdjustment = outerRadius % 2;
                outerRadius -= outerRadiusAdjustment;
            }

            Vector2I playerChunk = WorldGenUtility.WorldToLod(playerPosition, chunkScale);

            // Must keep chunks in a even x even square to properly scale with rings
            int xOffset = (playerChunk.X % 2 == 0) ? 1 : 0;
            int yOffset = (playerChunk.Y % 2 == 0) ? 1 : 0;

            int lowerX = -outerRadius - (1 - xOffset);
            int upperX = outerRadius + xOffset;
            int lowerY = -outerRadius - (1 - yOffset);
            int upperY = outerRadius + yOffset;

            for (int dx = lowerX; dx <= upperX; dx++)
            {
                for (int dy = lowerY; dy <= upperY; dy++)
                {
                    // Keeps from generating inside innerRad
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) < innerRadius) continue;

                    float distanceSqrd = dx * dx + dy * dy;
                    Vector2I chunkCoordinate = playerChunk + new Vector2I(dx, dy);
                    var key = (chunkCoordinate, chunkScale);

                    chunksNeeded.Add(key);

                    if (!activeChunks.ContainsKey(key))
                    {
                        EnqueueChunk(chunkScale, chunkCoordinate, distanceSqrd);
                    }
                }
            }

            // Calculate innerRad of next LOD
            innerRadius = ((outerRadius + outerRadiusAdjustment) / 2) + 1;
        }

        CleanupUnusedChunks(chunksNeeded);
    }

    // --- LOADING MANAGEMENT ---

    // Only adds a chunk to the load queue and list of actively tracked chunks
    private void EnqueueChunk(int chunkScale, Vector2I chunkCoord, float distance)
    {
        bool isLodChunk = chunkScale != 1;

        var request = new ChunkLoadRequest
        {
            Key = (chunkCoord, chunkScale),
            Priority = (int)(chunkScale * chunkScale * distance),
            IsLodChunk = isLodChunk
        };

        ChunkManager chunk = chunkLoader.Pool.Get();
        chunk.Position = new Vector3(request.Key.coord.X, 0, request.Key.coord.Y) * WorldGenUtility.chunkSize * request.Key.scale;
        activeChunks.Add((chunkCoord, chunkScale), chunk);

        request.AssignedChunk = chunk;
        chunkLoader.Enqueue(request);
    }
    
    private void CleanupUnusedChunks(HashSet<(Vector2I, int)> needed)
    {
        chunksToRemove.Clear();

        foreach (var kvp in activeChunks)
        {
            if (!needed.Contains(kvp.Key))
            {
                ChunkManager chunk = kvp.Value;
                if (chunk == null)
                {
                    continue;
                }
                if (chunk.State != ChunkState.Active)
                {
                    continue;
                }

                chunksToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in chunksToRemove)
        {
            if (!activeChunks.TryGetValue(key, out var chunk))
            {
                continue;
            }

            activeChunks.Remove(key);

            chunk.ReleaseToPool();
        }
    }
}
