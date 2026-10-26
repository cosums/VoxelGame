using Godot;
using System;

public partial class Backdrop : Panel
{
    [Export] public int gridWidth = 10;
    [Export] float lineThickness;
    [Export] Color gridColor;
    [Export] float driftSpeed;

    private Vector2 gridOrigin = Vector2.Zero;
    
    public override void _Draw()
    {
        for (float x = -gridOrigin.X; x <= Size.X; x += gridWidth)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, Size.Y), gridColor, lineThickness);
        }

        for (float y = -gridOrigin.Y; y <= Size.Y; y += gridWidth)
        {
            DrawLine(new Vector2(0, y), new Vector2(Size.X, y), gridColor, lineThickness);
        }

        for (float x = -gridOrigin.X - gridWidth; x <= Size.X; x += gridWidth)
        {
            for (float y = -gridOrigin.Y - gridWidth; y <= Size.Y; y += gridWidth)
            {
                Vector2 topLeft = new Vector2(x, y);
                Vector2 bottomRight = topLeft + new Vector2(gridWidth, gridWidth);

                DrawLine(topLeft, bottomRight, gridColor, lineThickness);
            }
        }
    }


    public override void _Process(double delta)
    {
        gridOrigin -= Vector2.One * (float)delta * driftSpeed;
        gridOrigin = gridOrigin % gridWidth;

        QueueRedraw();
    }
}
