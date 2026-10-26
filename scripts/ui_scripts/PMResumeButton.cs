using Godot;
using System;

public partial class PMResumeButton : Button
{
    [Export] SettingsManager settingsManager;
    
    public override void _Ready()
    {
        Pressed += OnButtonPressed;
    }

    public void OnButtonPressed()
    {
        settingsManager.Toggle();
    }
}
