using Godot;
using System;

public partial class RBNewtonianPlayer : RigidBody3D
{
    [Export] KeplerianBody PrimaryBody;

    [Export] public float MouseSensitivity;
    [Export] public Node3D VisualBody; // child of the body to rotate in process for smooth camera
    [Export] public Node3D CameraAnchor; // child of the visual body
    [Export] public float Speed = 10f;

    private Vector2 _mouseDelta;
    private float _camX, _camY;

    public override void _Ready()
    {
        GravityScale = 0;
    }

    public override void _Process(double delta)
    {
        Look();
    }

    public override void _PhysicsProcess(double delta)
    {
        LinearVelocity = PrimaryBody.GlobalVelocity;
    }


    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_mouseDelta = mouseMotion.Relative;
		}
	}

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        // UpdateDown(state);
        // ApplyGravity(state);

        // Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        // Vector3 moveVec = GetMovementDirection(inputDir);

        // state.ApplyCentralForce(moveVec * Speed);
        // GD.Print(moveVec);
    }

    private void Look()
    {
        MouseSensitivity = UserSettings.Instance.mouseSensitivity / 200f;

        _camX -= _mouseDelta.Y * MouseSensitivity;
		_camX = Mathf.Clamp(_camX, -90f, 90f);
		_camY = -_mouseDelta.X * MouseSensitivity;

        CameraAnchor.RotationDegrees = new Vector3(_camX, 0, 0);
        VisualBody.RotateObjectLocal(Vector3.Up, Mathf.DegToRad(_camY));

        _mouseDelta = Vector2.Zero;
    }

    private void UpdateDown(PhysicsDirectBodyState3D state)
    {
        Vector3 globalPosition = state.Transform.Origin;
        Vector3 planetPosition = PrimaryBody.GlobalTransform.Origin;
        Vector3 displacement = planetPosition - globalPosition;
        Vector3 direction = displacement.Normalized();

        Vector3 targetUp = -direction;

        Vector3 currentForward = -state.Transform.Basis.Z;

        Vector3 newRight = currentForward.Cross(targetUp).Normalized();
        Vector3 newForward = targetUp.Cross(newRight).Normalized();

        Basis newBasis = new Basis(newRight, targetUp, -newForward);

        Transform3D newTransform = state.Transform;
        newTransform.Basis = newBasis;
        state.Transform = newTransform;
        state.AngularVelocity = Vector3.Zero;
    }

    private void ApplyGravity(PhysicsDirectBodyState3D state)
    {
        Vector3 globalPosition = state.Transform.Origin;
        Vector3 planetPosition = PrimaryBody.GlobalTransform.Origin;
        Vector3 displacement = planetPosition - globalPosition;
        Vector3 direction = displacement.Normalized();
        
        float distanceSquared = displacement.LengthSquared();
        if (distanceSquared < 0.01f) return;
        float G = PhysicalConstants.GRAVITATIONAL_CONSTANT;
        float forceMagnitude = G * Mass * PrimaryBody.Mass / distanceSquared;
        Vector3 forceVector = forceMagnitude * direction;
        state.ApplyCentralForce(forceVector);
    }

    private Vector3 GetMovementDirection(Vector2 inputDir)
    {
        Vector3 forward = VisualBody.GlobalTransform.Basis.Z;
        Vector3 right = VisualBody.GlobalTransform.Basis.X;

        return (forward * inputDir.Y + right * inputDir.X).Normalized();
    }
}