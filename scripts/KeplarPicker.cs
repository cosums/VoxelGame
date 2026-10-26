// using Godot;
// using System;

// public partial class KeplarPicker : StaticBody3D
// {
//     [Export] KeplerianRail attachedBody;
//     [Export] Node3D debugCam;

//     public override void _InputEvent(Camera3D camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, int shapeIdx)
//     {
//         if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
//         {
//             if (mouseEvent.ButtonIndex == MouseButton.Left)
//             {
//                 GD.Print("attempted to load the settings???");
//                 attachedBody.AddChild(debugCam);
//             }
//         }
//     }

// }
