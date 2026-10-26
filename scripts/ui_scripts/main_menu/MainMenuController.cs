using Godot;
using System;

public partial class MainMenuController : Node
{
    public override void _Ready()
    {
        GameManager.Instance.ChangeState(GameState.Menus);
    }
}
