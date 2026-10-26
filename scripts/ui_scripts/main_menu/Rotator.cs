using Godot;
using System;

public partial class Rotator : Node3D
{
    [Export] public float rotationSpeed = 30f;
    public override void _Process(double delta)
    {
        //RotateY(Mathf.DegToRad(rotationSpeed * (float)delta));
        RotateObjectLocal(Vector3.Up, Mathf.DegToRad(rotationSpeed * (float)delta));
    }
}
