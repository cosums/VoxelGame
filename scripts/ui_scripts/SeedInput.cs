using Godot;
using System;

public partial class SeedInput : LineEdit
{
    int seed;
    
    public override void _Ready()
    {
        if (WorldManager.Instance.worldData == null)
        {
            seed = 0;
        } else {
            seed = WorldManager.Instance.worldData.generation.seed;
        }
        
        PlaceholderText = seed.ToString();
        TextSubmitted += SubmitSeed;
    }

    private void SubmitSeed(string text)
    {
        if (int.TryParse(text, out int parsedValue))
        {
            seed = parsedValue;
        }
        else
        {
            seed = (int)text.Hash();
        }

        if (seed == 0)
        {
            seed = 3564;
        }

        //UserSettings.Instance.seed = seed;
        UserSettings.Instance.MarkDirty();

        Clear();
        ReleaseFocus();
        PlaceholderText = seed.ToString();
    }
}
