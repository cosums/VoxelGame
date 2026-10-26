using Godot;
using System;

public partial class CppTest : Node3D
{
    public override void _Ready()
    {
        var t = ClassDB.Instantiate("Terrain").As<Node3D>();
        AddChild(t);
        GD.Print(t.Call("add", 2, 3));
    }
}
