using Godot;
using System;

public interface IOrbitable
{
    float Mass { get;}
    Vector3 GlobalPosition { get; }
    Vector3 LocalVelocity { get; }
    Vector3 GlobalVelocity { get;}
    float SOIDistance { get; set;}
    int SphereOfInfluencePriority { get; set; }
}
