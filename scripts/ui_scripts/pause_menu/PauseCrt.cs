using Godot;
using System;

public partial class PauseCrt : ColorRect
{
    [Export] SettingsManager settingsManager;

    public override void _Ready()
    {
        Visible = false;
        GameManager.Instance.GameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.Paused)
        {
            Visible = true;
        } else
        {
            Visible = false;
        }
    }

    public override void _ExitTree()
    {
        GameManager.Instance.GameStateChanged -= OnGameStateChanged;
    }

}
