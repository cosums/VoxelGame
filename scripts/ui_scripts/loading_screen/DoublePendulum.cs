using Godot;
using System;
using System.Collections.Generic;

public partial class DoublePendulum : Node2D
{
    [Export] public float rod1 = 200f;
    [Export] public float rod2 = 200f;
    [Export] public float mass1 = 20f;
    [Export] public float mass2 = 20f;
    [Export] public float angle1 = Mathf.Pi / 2;
    [Export] public float angle2 = Mathf.Pi / 2.1f;
    [Export] public Vector2 velocity1 = Vector2.Zero; 
    [Export] public Vector2 velocity2 = Vector2.Zero;
    [Export] public float ballRadius = 20f;
    [Export] public float rodRadius = 5f;

    private float angularVelocity1 = 0f;
    private float angularVelocity2 = 0f;
    private float gravity = 9.8f;
    private float timeScale = 10;

    private List<Vector2> trail = new List<Vector2>();
    [Export] int maxTrailLength = 20000;
    [Export] int trailWidth = 10;


    public override void _Ready()
    {
        GlobalPosition = GetViewportRect().Size / 2;

        var rng = new RandomNumberGenerator();

        angle1 = rng.Randf() * 2 * Mathf.Pi;
        angle2 = rng.Randf() * 2 * Mathf.Pi;
    }

    public override void _Process(double delta)
    {
        UpdatePendulum((float)delta * timeScale);

        Vector2 origin = Vector2.Zero;
        Vector2 pos1 = origin + new Vector2(rod1 * Mathf.Sin(angle1), rod1 * Mathf.Cos(angle1));
        Vector2 pos2 = pos1 + new Vector2(rod2 * Mathf.Sin(angle2), rod2 * Mathf.Cos(angle2));

        trail.Add(pos2);
        if (trail.Count > maxTrailLength)
            trail.RemoveAt(0);

        QueueRedraw();
    }

    private void UpdatePendulum(float delta)
    {
        float num1 = -gravity * (2 * mass1 + mass2) * Mathf.Sin(angle1);
        float num2 = -mass2 * gravity * Mathf.Sin(angle1 - 2 * angle2);
        float num3 = -2 * Mathf.Sin(angle1 - angle2) * mass2;
        float num4 = angularVelocity2 * angularVelocity2 * rod2 + angularVelocity1 * angularVelocity1 * rod1 * Mathf.Cos(angle1 - angle2);
        float den = rod1 * (2 * mass1 + mass2 - mass2 * Mathf.Cos(2 * angle1 - 2 * angle2));
        float accel1 = (num1 + num2 + num3 * num4) / den;

        num1 = 2 * Mathf.Sin(angle1 - angle2);
        num2 = angularVelocity1 * angularVelocity1 * rod1 * (mass1 + mass2);
        num3 = gravity * (mass1 + mass2) * Mathf.Cos(angle1);
        num4 = angularVelocity2 * angularVelocity2 * rod2 * mass2 * Mathf.Cos(angle1 - angle2);
        den = rod2 * (2 * mass1 + mass2 - mass2 * Mathf.Cos(2 * angle1 - 2 * angle2));
        float accel2 = (num1 * (num2 + num3 + num4)) / den;

        angularVelocity1 += accel1 * delta;
        angularVelocity2 += accel2 * delta;

        angle1 += angularVelocity1 * delta;
        angle2 += angularVelocity2 * delta;
    }

    public override void _Draw()
    {
        Vector2 origin = Vector2.Zero; 
        Vector2 pos1 = origin + new Vector2(rod1 * Mathf.Sin(angle1), rod1 * Mathf.Cos(angle1));
        Vector2 pos2 = pos1 + new Vector2(rod2 * Mathf.Sin(angle2), rod2 * Mathf.Cos(angle2));

        DrawLine(origin, pos1, Colors.White, rodRadius);
        DrawLine(pos1, pos2, Colors.White, rodRadius);

        DrawCircle(origin, ballRadius, Colors.White);
        DrawCircle(pos1, ballRadius, Colors.White);
        DrawCircle(pos2, ballRadius, Colors.White);

        for (int i = 1; i < trail.Count; i++)
        {
            float alpha = i / (float)trail.Count;
            DrawLine(trail[i - 1], trail[i], new Color(1, 1, 1, alpha), trailWidth);
        }
    }
}
