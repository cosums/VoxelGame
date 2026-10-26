using Godot;
using System;

public partial class StarField : GpuParticles3D
{
    [Export] Node3D _camera;

    public override void _Process(double delta)
    {
        GlobalPosition = _camera.GlobalPosition;
    }

}
