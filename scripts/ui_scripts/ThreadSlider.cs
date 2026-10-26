using Godot;
using System;

public partial class ThreadSlider : HSlider
{
    [Export] Label countLabel;
    int maxThreads;
    
    public override void _Ready()
    {
        maxThreads = System.Environment.ProcessorCount;
        MaxValue = maxThreads;
        Value = UserSettings.Instance.threadCount;
        countLabel.Modulate = CurrentZone(Value);

        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        UserSettings.Instance.threadCount = (int)value;
        UserSettings.Instance.MarkDirty();
        countLabel.Modulate = CurrentZone(value);
    }

    private Color CurrentZone(double value)
    {
        if (value > maxThreads * 0.75f)
        {
            return Colors.Red;
        } else if (value > maxThreads * 0.5f)
        {
            return Colors.Yellow;
        } else
        {
            return Colors.Green;
        }
    }
}
