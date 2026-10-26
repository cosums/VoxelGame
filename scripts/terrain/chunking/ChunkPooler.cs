using Godot;
using System;
using System.Collections.Generic;

public class ChunkPooler
{
    private readonly Node parent;
    private readonly Stack<ChunkManager> pool = new();

    public ChunkPooler(Node parent)
    {
        this.parent = parent;
    }

    /// <summary>
    /// Pulls a chunk from the pool or generates a new one if none are in the pool.
    /// </summary>
    /// <returns>
    /// A <see cref="ChunkManager"/> instance either pulled or created. 
    /// </returns>
    public ChunkManager Get()
    {
        ChunkManager chunk;
        if (pool.Count > 0)
        {
            chunk = pool.Pop();
        }
        else
        {
            chunk = new ChunkManager();
            parent.AddChild(chunk);
        }

        chunk.OnUnloaded += Release;
        return chunk;
    }

    /// <summary>
    /// Releases a chunk back into the pool for future reuse.
    /// </summary>
    /// <param name="chunk"></param>
    public void Release(ChunkManager chunk)
    {
        chunk.OnUnloaded -= Release;

        chunk.Name = "Pooled Chunk";
        chunk.Visible = false;

        pool.Push(chunk);
    }
    
    /// <summary>
    /// Fully clears the pool.
    /// </summary>
    public void Clear()
    {
        foreach (var c in pool)
        {
            c.QueueFree();
        }
        pool.Clear();

        GD.Print("ChunkPool: Cleared all pooled chunks.");
    }
}
