using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ChunkLoader
{
    private readonly PriorityQueue<ChunkLoadRequest, int> queue = new();
    private readonly HashSet<ChunkLoadRequest> activeRequests = new();
    public readonly ChunkPooler Pool;
    private TerrainField terrainField;
    private int maxConcurrency;
    private int activeThreads;

    public ChunkLoader(ChunkPooler Pool, TerrainField terrainField, int maxConcurrency)
    {
        this.Pool = Pool;
        this.terrainField = terrainField;
        this.maxConcurrency = maxConcurrency;
    }

    public void Enqueue(ChunkLoadRequest request)
    {
        queue.Enqueue(request, request.Priority);
    }

    public void TryProcessNext()
    {
        if (queue.Count == 0)
        {
            return;
        }
        else
        {
            while (activeThreads < maxConcurrency && queue.Count != 0)
            {
                ChunkLoadRequest request = queue.Dequeue();
                activeThreads++;

                ProcessRequest(request);
            }
        }
    }

    private void ProcessRequest(ChunkLoadRequest request)
    {
        request.AssignedChunk.Initialize(request.Key.scale, request.IsLodChunk);
        request.AssignedChunk.OnReady += ChunkLoaded;

        activeRequests.Add(request);
        request.AssignedChunk.BuildAsync(terrainField, request.Cts.Token);
    }

    private void ChunkLoaded(ChunkManager chunk)
    {
        chunk.OnReady -= ChunkLoaded;

        ChunkLoadRequest finishedRequest = null;
        foreach (var r in activeRequests)
        {
            if (r.AssignedChunk == chunk)
            {
                finishedRequest = r;
                break;
            }
        }

        if (finishedRequest != null) activeRequests.Remove(finishedRequest);
        
        activeThreads--;
        TryProcessNext();
    }
    
    public void Clear(int newThreadCount)
    {
        // Cancel queued
        while (queue.Count > 0)
        {
            if (queue.TryDequeue(out ChunkLoadRequest request, out _))
            {
                request.Cts.Cancel();
                request.Cts.Dispose();
            }
        }

        // Cancel in-progress
        foreach (var request in activeRequests.ToList())
        {
            request.Cts.Cancel();
            request.Cts.Dispose();
            activeRequests.Remove(request);
        }

        activeThreads = 0;
        maxConcurrency = newThreadCount;

        GD.Print("ChunkLoader: Cleared all pending chunk load requests.");

        Pool.Clear();
    }

    public void SetSeed(int newSeed)
    {
        terrainField = new TerrainField(newSeed);
    }
    
    public void Cancel((Vector2I coord, int scale) key)
    {
        // TODO
    }
}
