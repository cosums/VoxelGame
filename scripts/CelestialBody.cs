// using Godot;
// using System;

// public partial class CelestialBody : Node3D
// {
//     [Export] public KeplerianRail bodyRail;
//     [Export] CelestialBody parentBody;
//     [Export] public float mass;
//     [Export] bool dominant = false;
//     [Export] int priority;

//     float SOI;

//     public override void _EnterTree()
//     {
//         if (!dominant)
//         {
//             SOI = CalculateSphereOfInfluence();
//         }
//     }

//     public override void _PhysicsProcess(double delta)
//     {
//         // patched conics logic
//         if (NewtonianObject.newtonianObjects == null) return;

//         foreach (var obj in NewtonianObject.newtonianObjects)
//         {
//             Vector3 direction = GlobalPosition - obj.GlobalPosition;
//             float distance = direction.Length();
//             if (dominant && obj.influencingBody == null)
//             {
//                 ApplyForceVector(obj);
//             }
//             else if (obj.influencingBody == this)
//             {
//                 if (distance > SOI) obj.SetInfluencingBody(null);
//                 else ApplyForceVector(obj);
//             }
//             else if (distance <= SOI)
//             {
//                 if (obj.influencingBody == null)
//                 {
//                     ApplyForceVector(obj);
//                     obj.SetInfluencingBody(this);
//                 }
//                 else if (obj.influencingBody.priority < priority)
//                 {
//                     ApplyForceVector(obj);
//                     obj.SetInfluencingBody(this);
//                 }
//             }
//         }
//     }

//     private void ApplyForceVector(NewtonianObject obj)
//     {
//         Vector3 bodyPosition = obj.GlobalPosition;
//         Vector3 myPosition = GlobalPosition;

//         Vector3 direction = myPosition - bodyPosition;
//         float distance = direction.Length();

//         // backout for no /0 things
//         // also not throwing things wildly out of scale
//         if (distance < 0.1f) return;

//         float G = PhysicalConstants.GRAVITATIONAL_CONSTANT;
//         float force = mass * obj.bodyMass / (distance * distance) * G;

//         Vector3 forceVector = direction.Normalized() * force;
//         obj.ApplyForce(forceVector);
//     }

//     private float CalculateSphereOfInfluence()
//     {
//         //float semiMajorAxis = bodyRail._semiMajorAxis;
//         //float soi = semiMajorAxis * Mathf.Pow(mass / parentBody.mass, 0.4f);
//         //GD.Print(Name + "'s SOI: " + soi);
//         //return soi;
//     }
// }
