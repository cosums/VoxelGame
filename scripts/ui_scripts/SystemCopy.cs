using Godot;
using System;
using System.Text;

public partial class SystemCopy : Button
{
    public override void _Ready()
    {
        Pressed += CopySystemInfoToClipBoard;
    }

    private void CopySystemInfoToClipBoard()
    {
        var info = new StringBuilder();

        info.AppendLine($"OS: {OS.GetName()} {OS.GetVersion()}");
        info.AppendLine($"CPU: {OS.GetProcessorName()}");
        info.AppendLine($"Cores: {OS.GetProcessorCount()}");
        
        var memInfo = OS.GetMemoryInfo();
        if (memInfo.TryGetValue("physical", out var physicalMem))
        {
            info.AppendLine($"RAM: {Math.Round((double)physicalMem / (1024 * 1024 * 1024), 2)} GB");
        }
        else
        {
            info.AppendLine("ERROR ACCESSING RAM");
        }

        info.AppendLine($"Renderer: {RenderingServer.GetCurrentRenderingMethod().ToUpperInvariant()}");
        info.AppendLine($"GPU: {RenderingServer.GetVideoAdapterName()}");

        int screenId = DisplayServer.GetPrimaryScreen();
        int refreshRate = (int)Math.Round(DisplayServer.ScreenGetRefreshRate(screenId));
        info.AppendLine($"MRR: {refreshRate} Hz");

        DisplayServer.ClipboardSet(info.ToString());

        GD.Print("Copied system info to clipboard.");
    }
}
