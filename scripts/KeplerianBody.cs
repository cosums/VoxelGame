using Godot;
using System;

public partial class KeplerianBody : AnimatableBody3D, IOrbitable
{
    public Vector3 LocalVelocity { get; set; }
    public Vector3 GlobalVelocity
    {
        get
        {
            return (_primaryBody != null) ? _primaryBody.GlobalVelocity + LocalVelocity : LocalVelocity;
        }
    }

    // --- PRIMARY --- 
    [Export] public Node3D PrimaryBodyNode;
    private IOrbitable _primaryBody;
    
    // --- ORBITAL VALUES ---
    [Export] public float Mass { get; set; }
    [Export] public float SemiMajorAxis;
    [Export] public float Eccentricity; 
    [Export] public float PericenterTime;
    [Export] public float Inclination;
    [Export] public float LongitudeAscending;
    [Export] public float ArgumentPeriapsis; 
    [Export] public int SphereOfInfluencePriority { get; set; } = 0;

    // --- ANGULAR VALUES ---
    [Export] public float RotationalPeriod; // seconds

    private float _elapsedTime;
    private float _orbitalPeriod; // calculate from G going forward!
    public float SOIDistance { get; set; }
    private Area3D _soiArea;

    public override void _Ready()
    {
        _primaryBody = PrimaryBodyNode as IOrbitable;
        
        _orbitalPeriod = CalculateOrbitalPeriod();
        GD.Print(Name + "'s orbital period: " + _orbitalPeriod);
        
        GlobalPosition = UpdatePosition(0);
        SetupSphereOfInfluence();
    }

    public override void _PhysicsProcess(double delta)
    {
        _elapsedTime += (float)delta;
        float rateOfSweep = Mathf.Pi * 2f / _orbitalPeriod;
        float meanAnomaly = rateOfSweep * (_elapsedTime - PericenterTime);
        GlobalPosition = UpdatePosition(meanAnomaly);
    }

    private float CalculateOrbitalPeriod()
    {
        float numerator = 4f * Mathf.Pi * Mathf.Pi * SemiMajorAxis * SemiMajorAxis * SemiMajorAxis;
        float denominator = PhysicalConstants.GRAVITATIONAL_CONSTANT * (_primaryBody.Mass + Mass);
        return Mathf.Sqrt(numerator / denominator);
    }

    private Vector3 UpdatePosition(float meanAnomaly)
    {
        float e = Eccentricity;
        float e2 = e * e;
        float e3 = e2 * e;

        float eccentricAnomaly = (e > 0.8) ? Mathf.Pi : meanAnomaly;
        
        for (int i = 0; i < PhysicalConstants.ECCENTRIC_ITERATIONS; i++)
        {
            float delta = (eccentricAnomaly - e * Mathf.Sin(eccentricAnomaly) - meanAnomaly)
                / (1f - e * Mathf.Cos(eccentricAnomaly));
            eccentricAnomaly -= delta;

            if (Math.Abs(delta) < 1e-7) break;
        }

        float y_ta = Mathf.Sqrt(1f + e) * Mathf.Sin(eccentricAnomaly / 2f);
        float x_ta = Mathf.Sqrt(1f - e) * Mathf.Cos(eccentricAnomaly / 2f);
        float trueAnomaly = 2f * Mathf.Atan2(y_ta, x_ta);

        UpdateVelocity(trueAnomaly);

        float radius = SemiMajorAxis * (1f - e2) / (1 + e * Mathf.Cos(trueAnomaly));
        float mu = PhysicalConstants.GRAVITATIONAL_CONSTANT * (_primaryBody.Mass + Mass);
        
        //float scalarOrbitSpeed = Mathf.Sqrt(mu * ((2f / radius) - (1f / SemiMajorAxis))); // useful for debug purposes sometimes
     
        float x = radius * Mathf.Cos(trueAnomaly);
        float y = radius * Mathf.Sin(trueAnomaly);

        float arg = Mathf.DegToRad(ArgumentPeriapsis);
        float inc = Mathf.DegToRad(Inclination);
        float lan = Mathf.DegToRad(LongitudeAscending);

        float cosArg = Mathf.Cos(arg); float sinArg = Mathf.Sin(arg);
        float cosInc = Mathf.Cos(inc); float sinInc = Mathf.Sin(inc);
        float cosLan = Mathf.Cos(lan); float sinLan = Mathf.Sin(lan);

        float xFinal = (cosLan * cosArg - sinLan * sinArg * cosInc) * x + (-cosLan * sinArg - sinLan * cosArg * cosInc) * y;
        float zFinal = (sinLan * cosArg + cosLan * sinArg * cosInc) * x + (-sinLan * sinArg + cosLan * cosArg * cosInc) * y;
        float yFinal = (sinArg * sinInc) * x + (cosArg * sinInc) * y;

        return _primaryBody.GlobalPosition + new Vector3(xFinal, yFinal, zFinal);
    }

    public void UpdateVelocity(float trueAnomaly)
    {
        float gravitationalParameter = PhysicalConstants.GRAVITATIONAL_CONSTANT * (_primaryBody.Mass + Mass);
        float semiLatusRectum = SemiMajorAxis * (1f - (Eccentricity * Eccentricity));

        float x = -1f * Mathf.Sqrt(gravitationalParameter / semiLatusRectum) * Mathf.Sin(trueAnomaly);
        float y = Mathf.Sqrt(gravitationalParameter / semiLatusRectum) * (Eccentricity + Mathf.Cos(trueAnomaly));

        float arg = Mathf.DegToRad(ArgumentPeriapsis);
        float inc = Mathf.DegToRad(Inclination);
        float lan = Mathf.DegToRad(LongitudeAscending);

        float cosArg = Mathf.Cos(arg); float sinArg = Mathf.Sin(arg);
        float cosInc = Mathf.Cos(inc); float sinInc = Mathf.Sin(inc);
        float cosLan = Mathf.Cos(lan); float sinLan = Mathf.Sin(lan);

        float xFinal = (cosLan * cosArg - sinLan * sinArg * cosInc) * x + (-cosLan * sinArg - sinLan * cosArg * cosInc) * y;
        float zFinal = (sinLan * cosArg + cosLan * sinArg * cosInc) * x + (-sinLan * sinArg + cosLan * cosArg * cosInc) * y;
        float yFinal = (sinArg * sinInc) * x + (cosArg * sinInc) * y;

        LocalVelocity = new Vector3(xFinal, yFinal, zFinal);
    }

    private void SetupSphereOfInfluence()
    {
        SOIDistance = SemiMajorAxis * Mathf.Pow(Mass / _primaryBody.Mass, 0.4f);
    
        _soiArea = new Area3D();
        _soiArea.Name = Name + "_SOI_Area";
        AddChild(_soiArea);
        
        // --- DEBUG MESH STUFF ---
        MeshInstance3D debugMesh = new MeshInstance3D();
        SphereMesh sphereMesh = new SphereMesh();
        sphereMesh.Radius = SOIDistance;
        sphereMesh.Height = SOIDistance * 2f;
        
        StandardMaterial3D debugMat = new StandardMaterial3D();
        debugMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        debugMat.AlbedoColor = new Color(0.2f, 0.6f, 1.0f, 0.15f); 
        debugMat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        
        debugMesh.Mesh = sphereMesh;
        debugMesh.MaterialOverride = debugMat;
        _soiArea.AddChild(debugMesh);
        // ------------------------

        CollisionShape3D shapeNode = new CollisionShape3D();
        SphereShape3D sphere = new SphereShape3D();
        sphere.Radius = SOIDistance;
        shapeNode.Shape = sphere;
        
        _soiArea.AddChild(shapeNode);

        _soiArea.BodyEntered += OnSOIEntered;
        _soiArea.BodyExited += OnSOIExited;

        CheckInitialOverlapsAsync();
    }

    private async void CheckInitialOverlapsAsync()
    {
        // Wait a few frames
        for (int i = 0; i < 5; i++) {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
        
        CheckInitialOverlaps();
    }

    private void CheckInitialOverlaps()
    {
        foreach (Node3D body in _soiArea.GetOverlappingBodies())
        {
            GD.Print("Found body: " + body.Name);
            if (body is NewtonianObject obj)
            {
                obj.SetInitialForces(this);
            }
        }
    }

    private void OnSOIEntered(Node3D body)
    {
        if (body is NewtonianObject obj)
        {
            obj.AddPotentialInfluence(this);
        }
    }

    private void OnSOIExited(Node3D body)
    {
        if (body is NewtonianObject obj)
        {
            obj.RemovePotentialInfluence(this);
        }
    }
}
