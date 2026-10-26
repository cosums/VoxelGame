using Godot;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

public class ChunkData
{
    private TerrainField terrainField;
    public float[,,] samples;
    public byte[,,] materials;

    public bool parallel = false;

    string name;

    // --- FLAT TERRAIN ---
    public ChunkData(TerrainField terrainField, string name)
    {
        this.name = name;
        this.terrainField = terrainField;
    }

    public float[,,] GenerateData(Vector3 position, int chunkScale, bool isLodRing = false)
    {
        if (LoadData())
        {
            return samples;
        }
        
        int chunkWidth = WorldGenUtility.chunkSize;
        int chunkHeight = WorldGenUtility.chunkHeight;

        int padStart = isLodRing ? -1 : 0;
        //int padEnd = 1;

        int pointWidth = chunkWidth + (isLodRing ? 3 : 2);
        int pointHeight = chunkHeight + 1;

        samples = new float[pointWidth, pointHeight, pointWidth];
        materials = new byte[pointWidth, pointHeight, pointWidth];

        float[,] surfaceSamples = new float[pointWidth, pointWidth];

        for (int x = 0; x < pointWidth; x++)
        {
            float sx = position.X + (x + padStart) * chunkScale;
            for (int z = 0; z < pointWidth; z++)
            {
                float sz = position.Z + (z + padStart) * chunkScale;

                terrainField.SampleHeight(sx, sz, out float surfaceDensity);
                surfaceSamples[x, z] = surfaceDensity;

                for (int y = 0; y < pointHeight; y++)
                {
                    float sy = position.Y + y * chunkScale;
                    float density = surfaceSamples[x, z] - sy;
                    
                    if (chunkScale > 1 && sy < WorldGenUtility.lodCutoff) {
                        samples[x, y, z] = 1;
                        materials[x, y, z] = 4; // mantlestone
                        continue;
                    }
                    
                    terrainField.SampleCaves(sx, sy, sz, density, out float caveDensity, out byte material);

                    float finalDensity = Mathf.Clamp(caveDensity, -1, 1);

                    samples[x, y, z] = finalDensity;
                    materials[x, y, z] = material;
                }
            }
        }


        // for (int x = 0; x < pointWidth; x++)
        // {
        //     float sx = position.X + (x + padStart) * chunkScale;
        //     for (int y = 0; y < pointHeight; y++)
        //     {
        //         float sy = position.Y + y * chunkScale;
        //         for (int z = 0; z < pointWidth; z++)
        //         {
        //             if (chunkScale > 1 && sy < WorldGenUtility.lodCutoff) {
        //                 samples[x, y, z] = 1;
        //                 materials[x, y, z] = 4; // mantlestone
        //                 continue;
        //             }
                    
        //             float sz = position.Z + (z + padStart) * chunkScale;

        //             terrainField.SampleField(sx, sy, sz, out float density, out byte material);

        //             samples[x, y, z] = density;
        //             materials[x, y, z] = material;
        //         }
        //     }
        // }

        SaveData();

        return samples;
    }
    
    public int Deform(Vector3 localPoint, float radius, float delta = -50f)
    {
        int dirty = 0;
        
        int width = samples.GetLength(0);
        int height = samples.GetLength(1);
        int depth = samples.GetLength(2);

        int minX = Mathf.Max(Mathf.FloorToInt(localPoint.X - radius), 0);
        int maxX = Mathf.Min(Mathf.CeilToInt(localPoint.X + radius), width - 1);

        int minY = Mathf.Max(Mathf.FloorToInt(localPoint.Y - radius), 0);
        int maxY = Mathf.Min(Mathf.CeilToInt(localPoint.Y + radius), height - 1);

        int minZ = Mathf.Max(Mathf.FloorToInt(localPoint.Z - radius), 0);
        int maxZ = Mathf.Min(Mathf.CeilToInt(localPoint.Z + radius), depth - 1);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    float dx = x - localPoint.X;
                    float dy = y - localPoint.Y;
                    float dz = z - localPoint.Z;
                    float distSqr = dx*dx + dy*dy + dz*dz;

                    if (distSqr > radius * radius)
                        continue;

                    float distance = Mathf.Sqrt(distSqr);
                    float falloff = 1f - (distance / radius);
                    samples[x, y, z] += delta * falloff;
                    if (delta > 0)
                    {
                        materials[x, y, z] = 0;
                    }

                    dirty = 1;
                }
            }
        }

        SaveData();

        return dirty;
    }

    public int SaveData()
    {
        string dirPath = ProjectSettings.GlobalizePath(
            WorldManager.Instance.worldPath + "surface/"
        );

        Directory.CreateDirectory(dirPath);

        string filePath = Path.Combine(dirPath, $"{name}.chunk");

        ChunkSaver.Save(filePath, samples, materials);

        return 0;
    }

    public bool LoadData()
    {
        string filePath = ProjectSettings.GlobalizePath(
            Path.Combine(WorldManager.Instance.worldPath + "surface/", $"{name}.chunk")
        );

        if (!File.Exists(filePath))
            return false;

        ChunkSaver.Load(filePath, out samples, out materials);
        return true;
    }
}
