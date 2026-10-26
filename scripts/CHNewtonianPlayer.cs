using Godot;
using System;

public partial class CHNewtonianPlayer : CharacterBody3D
{
    [Export] public KeplerianBody PrimaryBody;
    [Export] public float Speed = 10f, Acceleration = 20f, Mass = 62, JumpVelocity = 5f;
    [Export] public float MouseSensitivity;
    [Export] public Node3D VisualBody; // child of the body to rotate in process for smooth camera
    [Export] public Node3D CameraAnchor; // child of the visual body

    private Vector2 _mouseDelta;
    private float _camX, _camY;

    public override void _Ready()
    {
        PlatformOnLeave = PlatformOnLeaveEnum.DoNothing;
        PlatformFloorLayers = 0;
    }


    public override void _Process(double delta)
    {
        Look();
    }

    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_mouseDelta = mouseMotion.Relative;
		}
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

    public override void _PhysicsProcess(double delta)
    {
        // Outerwilds works such that if you are in air, you are applying a force.
        // This can let you go into orbit.
        // If you are on ground, you seem to have constant velocity.
        // It is probably a good idea to eventually have a similar setup.
        
        Vector3 displacement = GlobalPosition - PrimaryBody.GlobalPosition;
        float distanceSquared = displacement.LengthSquared();

        Vector3 upDir = displacement.Normalized();
        UpDirection = upDir;

        AlignWithUp(upDir);

        float verticalSpeed = Velocity.Dot(upDir);
        Vector3 verticalVelocity = upDir * verticalSpeed;
        Vector3 horizontalVelocity = Velocity - verticalVelocity;

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

        Vector3 lookForward = -VisualBody.GlobalTransform.Basis.Z;
        Vector3 lookRight = VisualBody.GlobalTransform.Basis.X;

        Vector3 moveDir = (lookForward * -inputDir.Y + lookRight * inputDir.X);
        moveDir = (moveDir - upDir * moveDir.Dot(upDir)).Normalized();

        Vector3 targetHorizontal = moveDir * Speed;
        horizontalVelocity = horizontalVelocity.Lerp(targetHorizontal, Acceleration * (float)delta);

        float G = PhysicalConstants.GRAVITATIONAL_CONSTANT;
        float gravityStrength = G * PrimaryBody.Mass / Position.LengthSquared();
        
        if (IsOnFloor()) {
            // if (Input.IsActionJustPressed("jump"))
            // {
            //     verticalVelocity = upDir * JumpVelocity;
            // }
            // else
            // {
                verticalVelocity = -upDir * 0.1f; 
            //}
        } 
        else {
            verticalVelocity -= upDir * gravityStrength * (float)delta;
        }

        if (Input.IsActionPressed("jump"))
        {
            verticalVelocity = upDir * JumpVelocity; // JETPACK
        }

        Velocity = horizontalVelocity + verticalVelocity;
        MoveAndSlide();
    }

    private void AlignWithUp(Vector3 targetUp)
    {
        Transform3D t = GlobalTransform;
        Vector3 currentForward = -t.Basis.Z;

        Vector3 newRight = targetUp.Cross(currentForward).Normalized();
        Vector3 newForward = newRight.Cross(targetUp).Normalized();

        t.Basis = new Basis(newRight, targetUp, -newForward);
        GlobalTransform = t.Orthonormalized();
    }
}