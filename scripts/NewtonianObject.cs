using Godot;
using System;
using System.Collections.Generic;

public partial class NewtonianObject : RigidBody3D
{
    private List<IOrbitable> _overlappingBodies = new();
    public IOrbitable InfluencingBody {get; private set; }

    public override void _EnterTree()
    {
        GravityScale = 0;

        if (InfluencingBody == null && StarController.Instance != null)
        {
            _overlappingBodies.Add(StarController.Instance);
            InfluencingBody = StarController.Instance;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyNewtonianGravity();
    }

    public void SetInitialForces(IOrbitable body)
    {
        GD.Print("Setting initial?");
        LinearVelocity = body.LocalVelocity;
        GD.Print(LinearVelocity);
        InfluencingBody = body;
        _overlappingBodies.Add(body);
    }

    private void ApplyNewtonianGravity()
    {
        Vector3 direction = InfluencingBody.GlobalPosition - GlobalPosition;
        float distanceSquared = direction.LengthSquared();

        // backout for no /0 things
        // also not throwing things wildly out of scale
        if (distanceSquared < 0.01f) return;

        float G = PhysicalConstants.GRAVITATIONAL_CONSTANT;
        float forceMagnitude = G * Mass * InfluencingBody.Mass / distanceSquared;
        Vector3 forceVector = direction.Normalized() * forceMagnitude;

        ApplyForce(forceVector);
    }

    public void AddPotentialInfluence(IOrbitable body)
    {
        GD.Print("Adding!");
        if (!_overlappingBodies.Contains(body))
        {
            _overlappingBodies.Add(body);
            UpdatePrimaryInfluence();
        }
    }

    public void RemovePotentialInfluence(IOrbitable body)
    {
        GD.Print("Removing!");
        if (_overlappingBodies.Contains(body))
        {
            _overlappingBodies.Remove(body);
            UpdatePrimaryInfluence();
        }
    }

    private void UpdatePrimaryInfluence()
    {
        IOrbitable bestBody = null;
        int maxPriority = int.MinValue;

        foreach (IOrbitable body in _overlappingBodies)
        {
            if (body.SphereOfInfluencePriority > maxPriority)
            {
                bestBody = body;
                maxPriority = body.SphereOfInfluencePriority;
                GD.Print(maxPriority);
                GD.Print((body as Node).Name);
            }
        }

        if (bestBody != null && bestBody != InfluencingBody)
        {
            SwitchReferenceFrame(bestBody);
        }
    }

    private void SwitchReferenceFrame(IOrbitable newBody)
    {
       Node targetParent = newBody as Node;
       InfluencingBody = newBody;

        GD.Print(Name + " is parenting to: " + targetParent.Name);
    }
}
