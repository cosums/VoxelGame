using Godot;
using System;

public partial class StarController : MeshInstance3D, IOrbitable
{
    [Export] public float Mass { get; set; }

    public Vector3 LocalVelocity { get; set; } = Vector3.Zero;
    public Vector3 GlobalVelocity { get; set;} = Vector3.Zero;
    public float SOIDistance { get; set;} = 999999999999;

    public int SphereOfInfluencePriority { get; set; } = -1;

    public static StarController Instance {get; private set;}
    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PushWarning("Two star controllers in scene.");
            QueueFree();
            return;
        }

        Instance = this;
    }

}
