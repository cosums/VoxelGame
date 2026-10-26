using Godot;
using System;

public partial class RingSlider : HSlider
{
    public override void _Ready()
    {
        Value = UserSettings.Instance.ringDistance;

        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        UserSettings.Instance.ringDistance = (int)value;
        UserSettings.Instance.MarkDirty();
    }
}
