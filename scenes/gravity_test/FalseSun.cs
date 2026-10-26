using Godot;
using System;

public partial class FalseSun : DirectionalLight3D
{
    [Export] public Node3D Camera;
    [Export] public Node3D Star;

    public override void _Process(double delta)
    {
        if (Camera == null || Star == null) return;

        GlobalPosition = Camera.GlobalPosition;
        
        Vector3 target = Star.GlobalPosition;
        Vector3 direction = (target - GlobalPosition).Normalized();

        if (Mathf.Abs(direction.Dot(Vector3.Up)) > 0.99f)
        {
            LookAt(target, Vector3.Forward);
        }
        else
        {
            LookAt(target, Vector3.Up);
        }

        RotateObjectLocal(Vector3.Up, Mathf.Pi);
    }

}
