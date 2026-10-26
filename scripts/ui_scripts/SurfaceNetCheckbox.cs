using Godot;
using System;

public partial class SurfaceNetCheckbox : CheckBox
{
    public override void _Ready()
    {
        ButtonPressed = UserSettings.Instance.surfaceNets;
        Toggled += ToggleSurfaceNets;    
    }

    public void ToggleSurfaceNets(bool toggle)
    {
        UserSettings.Instance.surfaceNets = toggle;
        UserSettings.Instance.MarkDirty();
    }
}
