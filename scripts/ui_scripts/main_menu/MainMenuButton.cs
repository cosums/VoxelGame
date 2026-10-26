using Godot;
using System;

public partial class MainMenuButton : Button
{
    [Export] public string normalText = " debug", hoveredText = ">debug";
    
    public override void _Ready()
    {
        Text = normalText;
        MouseEntered += OnFocusEntered;
        MouseExited += OnFocusExited;
    }

    private void OnFocusEntered()
    {
        Text = hoveredText;
    }

    private void OnFocusExited()
    {
        Text = normalText;
    }
}
