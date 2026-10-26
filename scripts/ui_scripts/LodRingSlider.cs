using Godot;
using System;

public partial class LodRingSlider : HSlider
{
    public override void _Ready()
    {
        Value = UserSettings.Instance.ringCount;
        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        UserSettings.Instance.ringCount = (int)value;
        UserSettings.Instance.MarkDirty();
    }
}
