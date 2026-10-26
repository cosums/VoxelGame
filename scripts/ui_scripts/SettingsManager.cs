using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class SettingsManager : Control
{
    [Export] FlatGenerator flatController;
    
    public override void _Ready()
    {
        Visible = false;
        CallDeferred(nameof(DeferredLoad));
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void DeferredLoad()
    {
        SaveManager.Instance.LoadData();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") && GameManager.Instance.State != GameState.Menus)
		{
			Toggle();
		}
    }

    private bool toggled = false;
    public void Toggle()
    {
        toggled = !toggled;
        //GD.Print("Toggled Settings: " + toggled);

        if (toggled)
        {
            Visible = true;
            GameManager.Instance.ChangeState(GameState.Paused);
        }
        else
        {
            Visible = false;
            GameManager.Instance.ChangeState(GameState.Playing);
            if (flatController != null && UserSettings.Instance.dirty)
            {
                flatController.UpdateSettings();
            }
        }
    }
}
