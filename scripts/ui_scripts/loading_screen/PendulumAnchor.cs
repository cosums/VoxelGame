using Godot;
using System;

public partial class PendulumAnchor : Node2D
{
    public override void _Ready()
    {
        Position = GetViewportRect().Size / 2;
    }

    
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 10, Colors.White);
    }
}
