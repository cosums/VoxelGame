using Godot;
using System;
using System.Collections.Generic;

public partial class UserSettings : Node, ISaveable
{
    public static UserSettings Instance { get; private set; }

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
            SetProcess(false);
        }
        else
        {
            QueueFree();
        }
    }
    
    public void ManualLoad()
    {
        SaveManager.Instance.RegisterSaveable(this);
        
        var data = SaveManager.Instance.GetDataFor(SaveID);
        if (data != null)
        {
            Load(data);
        }
    }

    public string SaveID => "UserSettings";

    public Dictionary<string, Variant> Save()
    {
        return new Dictionary<string, Variant>
        {
            { "mouse_sensitivity", mouseSensitivity },
            { "view_distance", viewDistance },
            { "ring_count", ringCount },
            { "ring_distance", ringDistance },
            { "thread_count", threadCount },
            { "full_screen", fullScreen },
            { "seed", seed },
            { "has_seen_terminal", hasSeenTerminal },
            { "version", version },
            { "renderer", renderer }
        };
    }

    public void Load(Dictionary<string, Variant> data)
    {
        if (data.TryGetValue("mouse_sensitivity", out var ms)) mouseSensitivity = (int)ms;
        if (data.TryGetValue("view_distance", out var vd)) viewDistance = (int)vd;
        if (data.TryGetValue("ring_count", out var rc)) ringCount = (int)rc;
        if (data.TryGetValue("ring_distance", out var rd)) ringDistance = (int)rd;
        if (data.TryGetValue("thread_count", out var tc)) threadCount = (int)tc;
        if (data.TryGetValue("full_screen", out var fs)) fullScreen = (bool)fs;
        if (data.TryGetValue("seed", out var sd)) seed = (int)sd;
        if (data.TryGetValue("has_seen_terminal", out var st)) hasSeenTerminal = (bool)st;
        if (data.TryGetValue("version", out var v)) version = (string)v;
        if (data.TryGetValue("renderer", out var r)) renderer = (int)r;
    }

    // preferences
    public int mouseSensitivity = 50;

    // performance
    public int viewDistance = 10;
    public int ringCount = 0;
    public int ringDistance = 2;
    public int threadCount = 4;
    
    // toggles
    public bool surfaceNets = true;
    public bool fullScreen = true;

    public int renderer = 0;

    // world
    public int seed = 3564;

    public bool hasSeenTerminal = false;

    public string version = "";

    public bool dirty { get; private set; } = false;

    public void MarkDirty()
    {
        dirty = true;
    }

    public void ScrubbyScrubby()
    {
        dirty = false;
    }
}
