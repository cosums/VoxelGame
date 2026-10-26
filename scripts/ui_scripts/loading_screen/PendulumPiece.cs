using Godot;
using System;

public partial class PendulumPiece : RigidBody2D
{
    public override void _Draw()
    {
        DrawCircle(Position, 50, Colors.Red);
    }

}
