// using Godot;
// using System;

// public partial class OrbitalConfigurer : VBoxContainer
// {
//     [Export] Slider e, a, T, i, la, per, arg;
//     KeplerianRail currentBody;

//     public override void _Ready()
//     {
//         Visible = false;
        
//         // lambds fucking suck dude
//         e.ValueChanged   += (val) => { currentBody.eccentricity =       (float)val;   UpdateBody(); };
//         a.ValueChanged   += (val) => { currentBody.semiMajorAxis =      (float)val;  UpdateBody(); };
//         T.ValueChanged   += (val) => { currentBody.orbitalPeriod =      (float)val;  UpdateBody(); };
//         i.ValueChanged   += (val) => { currentBody.inclination =        (float)val;   UpdateBody(); };
//         la.ValueChanged  += (val) => { currentBody.longitudeAscending = (float)val; UpdateBody(); };
//         per.ValueChanged += (val) => { currentBody.pericenterTime =        (float)val; UpdateBody(); };
//         arg.ValueChanged += (val) => { currentBody.argumentPeriapsis = (float)val; UpdateBody();};
//     }

//     private void UpdateBody()
//     {
//         Visible = true;
        
//         if (currentBody == null) return;
//         currentBody.Refresh(); 
//     }

//     public void LoadSettings(KeplerianRail body)
//     {
//         currentBody = body;
//         e.Value = body.eccentricity;
//         a.Value = body.semiMajorAxis;
//         T.Value = body.orbitalPeriod;
//         i.Value = body.inclination;
//         la.Value = body.longitudeAscending;
//         per.Value = body.pericenterTime;
//         arg.Value = body.argumentPeriapsis;
//     }
// }
