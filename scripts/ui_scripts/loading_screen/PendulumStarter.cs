using Godot;
using System;

public partial class PendulumStarter : RigidBody2D
{
    [Export] Vector2 startingForce;
    
    public override void _Ready()
    {
        ApplyCentralImpulse(startingForce);
    }

}
