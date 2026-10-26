using Godot;
using System;

public partial class LevelController : Node
{
    public override void _Ready()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
        //GameManager.Instance.SetPause(false);
    }
}
