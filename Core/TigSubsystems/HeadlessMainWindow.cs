using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.TigSubsystems;

public class HeadlessMainWindow : IMainWindow
{
    public void SetWindowMsgFilter(SDLEventFilter filter)
    {
    }

    public event Action<Size>? Resized;
    
    public event Action? Closed;
    
    public IUiRoot? UiRoot { get; set; }

    public Size OffScreenSize { get; set; } = new(1024, 768);

    public WindowConfig WindowConfig { get; set; }

    public void InvokeClosed()
    {
        Closed?.Invoke();
    }

    public void InvokeResized(Size size)
    {
        Resized?.Invoke(size);
    }

    public SizeF UiCanvasSize => new(OffScreenSize.Width, OffScreenSize.Height);

    public event Action UiCanvasSizeChanged;

    public float UiScale { get; set; } = 1.0f;

    public void SetCursor(int hotspotX, int hotspotY, string imagePath)
    {
    }

    public bool IsCursorVisible { get; set; }
    
    public void ProcessEvents()
    {
    }

    public void Dispose()
    {
    }
}