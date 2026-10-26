using Godot;
using System;
using System.Collections.Generic;

public partial class ViewDistanceSlider : HSlider
{
    public override void _Ready()
    {
        Value = UserSettings.Instance.viewDistance;

        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        UserSettings.Instance.viewDistance = (int)value;
        UserSettings.Instance.MarkDirty();
    }
}
