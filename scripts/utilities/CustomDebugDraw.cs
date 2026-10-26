using Godot;
using System;

public static class CustomDebugDraw
{
    public static Node3D DrawSphere(float radius = 0.1f, Color? color = null)
    {
        var mesh = new SphereMesh();
        mesh.Radius = radius;
        mesh.Height = radius * 2f;

        var instance = new MeshInstance3D();
        instance.Mesh = mesh;
        instance.Scale = Vector3.One;

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = color ?? Colors.White;
        instance.SetSurfaceOverrideMaterial(0, mat);

        return instance;
    }

    public static Node3D DrawDebugLine(Vector3 posA, Vector3 posB, Color? color = null, Node3D parent = null)
    {
        var mesh = new ImmediateMesh();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        mesh.SurfaceSetColor(color ?? Colors.White);
        mesh.SurfaceAddVertex(posA);
        mesh.SurfaceAddVertex(posB);
        mesh.SurfaceEnd();

        var instance = new MeshInstance3D();
        instance.Mesh = mesh;

        return instance;
    }
    
    public static void DrawDebugVoxel(Vector3 origin, float halfExtent, Color? color = null, Node3D parent = null)
    {
        
    }
}
