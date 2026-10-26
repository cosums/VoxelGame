using Godot;
using System;

public partial class MMTerminateButton : MainMenuButton
{

    public override void _Ready()
    {
        base._Ready();
        Pressed += OnTerminatePressed;
    }

    private void OnTerminatePressed()
    {
        GameManager.Instance.TerminateProcess();
    }
}
