using Godot;
using System;
using System.Threading;

public class ChunkLoadRequest
{
    public (Vector2I coord, int scale) Key;
    public int Priority;
    public CancellationTokenSource Cts = new();
    public bool IsLodChunk;
    public ChunkManager AssignedChunk;
}