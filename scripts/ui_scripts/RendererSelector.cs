using Godot;
using System;

public partial class RendererSelector : OptionButton
{
    [Export] Label warning;
    int currentRenderer = -1;
    
    public override void _Ready()
    {
        warning.Visible = false;
        Selected = UserSettings.Instance.renderer;
        currentRenderer = Selected;

        ItemSelected += OnItemSelected;
    }

    private void OnItemSelected(long index)
    {
        UserSettings.Instance.renderer = (int)index;
        warning.Visible = currentRenderer == index ? false : true;
    }
}
