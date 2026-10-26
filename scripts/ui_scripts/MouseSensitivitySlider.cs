using Godot;
using System;

public partial class MouseSensitivitySlider : HSlider
{
    public override void _Ready()
    {
        Value = UserSettings.Instance.mouseSensitivity;

        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        UserSettings.Instance.mouseSensitivity = (int)value;
    }
}
