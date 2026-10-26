using Godot;
using System;
using System.IO;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

public static class ChunkSaver
{
    const int SaveVersion = 2; // updated version for half-float

    public static void Save(string path, float[,,] samples, byte[,,] materials)
    {
        int w = samples.GetLength(0);
        int h = samples.GetLength(1);
        int d = samples.GetLength(2);

        using var fs = new FileStream(path, FileMode.Create, System.IO.FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
        using var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionLevel.Fastest);
        using var bw = new BinaryWriter(gz);

        // Header
        bw.Write(SaveVersion);
        bw.Write(w);
        bw.Write(h);
        bw.Write(d);

        // --- Flatten samples to half-floats for bulk write ---
        byte[] sampleBytes = new byte[w * h * d * 2]; // 2 bytes per half-float
        int index = 0;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                for (int z = 0; z < d; z++)
                {
                    ushort hf = FloatToHalf(samples[x, y, z]);
                    sampleBytes[index++] = (byte)(hf & 0xFF);
                    sampleBytes[index++] = (byte)(hf >> 8);
                }

        bw.Write(sampleBytes);

        // --- Flatten materials ---
        byte[] flatMaterials = new byte[w * h * d];
        index = 0;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                for (int z = 0; z < d; z++)
                    flatMaterials[index++] = materials[x, y, z];

        bw.Write(flatMaterials);
    }

    public static void Load(string path, out float[,,] samples, out byte[,,] materials)
    {
        samples = null;
        materials = null;

        if (!File.Exists(path))
            return;

        using var fs = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress);
        using var br = new BinaryReader(gz);

        int version = br.ReadInt32();
        if (version != SaveVersion)
            throw new Exception($"Unsupported chunk version: {version}");

        int w = br.ReadInt32();
        int h = br.ReadInt32();
        int d = br.ReadInt32();
        int length = w * h * d;

        // --- Read samples as half-floats ---
        byte[] sampleBytes = br.ReadBytes(length * 2);
        samples = new float[w, h, d];
        int index = 0;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                for (int z = 0; z < d; z++)
                {
                    ushort hf = (ushort)(sampleBytes[index++] | (sampleBytes[index++] << 8));
                    samples[x, y, z] = HalfToFloat(hf);
                }

        // --- Read materials ---
        byte[] flatMaterials = br.ReadBytes(length);
        materials = new byte[w, h, d];
        index = 0;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                for (int z = 0; z < d; z++)
                    materials[x, y, z] = flatMaterials[index++];
    }

    // Convert 32-bit float to 16-bit half-float
    private static ushort FloatToHalf(float f)
    {
        uint bits = BitConverter.SingleToUInt32Bits(f);
        uint sign = (bits >> 16) & 0x8000;
        uint exponent = ((bits >> 23) & 0xFF) - 127 + 15;
        uint mantissa = (bits >> 13) & 0x3FF;

        if (exponent <= 0) return (ushort)sign;        // underflow
        if (exponent >= 31) return (ushort)(sign | 0x7C00); // overflow
        return (ushort)(sign | (exponent << 10) | mantissa);
    }

    // Convert 16-bit half-float to 32-bit float
    private static float HalfToFloat(ushort h)
    {
        uint sign = (uint)(h & 0x8000) << 16;
        uint exponent = (uint)((h >> 10) & 0x1F);
        uint mantissa = (uint)(h & 0x3FF);

        if (exponent == 0) return BitConverter.UInt32BitsToSingle(sign); // zero or subnormal
        if (exponent == 31) return BitConverter.UInt32BitsToSingle(sign | 0x7F800000 | (mantissa << 13)); // inf/nan

        exponent = exponent - 15 + 127;
        uint bits = sign | (exponent << 23) | (mantissa << 13);
        return BitConverter.UInt32BitsToSingle(bits);
    }
}
