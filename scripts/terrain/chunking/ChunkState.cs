/// <summary>
/// Represents states a chunk can be in lifecycle.
/// </summary>
public enum ChunkState
{
    /// <summary>
    /// In chunk pool
    /// </summary>
    Pooled,
    /// <summary>
    /// Queued for loading
    /// </summary>
    Queued,
    /// <summary>
    /// Actively loading data and mesh
    /// </summary>
    Loading,
    /// <summary>
    /// Ready to spawn into the world
    /// </summary>
    Ready,
    /// <summary>
    /// Actively visible and in use
    /// </summary>
    Active,
    /// <summary>
    /// Being unloaded from world
    /// </summary>
    Unloading
}