using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public int SIZE = 8;
	public bool hasSeenTerminal = false;

	public string chunkInfo = "";
	public string worldSeed = "";

	public bool canPause = true;

	public event Action<GameState, GameState> GameStateChanged;

	public enum WorldModel
	{
		Flat,
		Smooth
	}

	public GameState State { get; private set; }

	public override void _EnterTree()
	{
		if (Instance == null)
		{
			Instance = this;
			SetProcess(false);
			ChangeState(GameState.Playing); //for debug purps
		}
		else
		{
			QueueFree();
		}

	}
	
    public override void _Ready()
    {
		ProcessMode = ProcessModeEnum.Always;
    }

	public void ChangeState(GameState newState)
	{
		if (State == newState) return;

		GameState oldState = State;
		
		ExitState(State);
		State = newState;
		EnterState(State);

		GameStateChanged?.Invoke(oldState, newState);
	}

	private void EnterState(GameState state)
	{
		switch (state)
		{
			case GameState.Menus:
				GetTree().Paused = false;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				break;
			case GameState.Playing:
				GetTree().Paused = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;
				break;
			case GameState.Paused:
				GetTree().Paused = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				break;
			case GameState.Inventory:
				GetTree().Paused = false;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				break;
			case GameState.Cutscene:
				GetTree().Paused = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;
				break;
		}
	}

	private void ExitState(GameState state)
	{
		switch (state)
		{
			case GameState.Menus:
				break;
			case GameState.Playing:
				break;
			case GameState.Paused:
				break;
			case GameState.Inventory:
				break;
			case GameState.Cutscene:
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("debug_draw"))
		{
			ToggleDebugDraw();
		}
	}

	private bool isDebugDrawing = false;
	private void ToggleDebugDraw()
	{
		isDebugDrawing = !isDebugDrawing;

		if (!isDebugDrawing)
		{
			GetViewport().DebugDraw = Viewport.DebugDrawEnum.Disabled;
		}
		else
		{
			GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
		}
	}

	public void TerminateProcess()
	{
		SaveManager.Instance.SaveData();
		GetTree().Quit();
    }

	public void StartSinglePlayer()
	{
		LevelManager levelManager = GetTree().CurrentScene as LevelManager;
		GD.Print("Running?");
		if (levelManager != null)
		{
			levelManager.AddSinglePlayer();
		}
	}

    public override void _Notification(int what)
    {
		if (what == NotificationWMCloseRequest)
		{
			GD.Print("Process terminating...");
			SaveManager.Instance.SaveData();
		}
    }


	public string username;
}
