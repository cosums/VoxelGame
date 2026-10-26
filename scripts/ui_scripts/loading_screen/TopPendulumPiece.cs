using Godot;
using System;

public partial class TopPendulumPiece : Node2D
{
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 10, Colors.White);
    }
}
