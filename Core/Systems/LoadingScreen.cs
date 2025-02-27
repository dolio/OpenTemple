using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Systems;

public class LoadingScreen : IDisposable, ILoadingProgress
{
    private const int BarBorderSize = 1;
    private const int BarHeight = 18;
    private const int BarWidth = 512;
    private readonly UiRectangle _barBorder;
    private readonly UiRectangle _barFilled;
    private readonly UiRectangle _barUnfilled;
    private readonly ComputedStyles _styles;

    private readonly IMainWindow _mainWindow;
    private readonly RenderingDevice _device;
    private WidgetImage _imageFile;
    public string Message { get; set; }
    private Dictionary<int, string> _messages;

    private bool _messagesLoaded;
    private float _progress;

    public float Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            UpdateFilledBarWidth();
        }
    }

    public LoadingScreen(IMainWindow mainWindow, RenderingDevice device, ShapeRenderer2d shapeRenderer2d)
    {
        var styleResolver = new StyleResolver(Globals.Config.DefaultUiFont, 10, PackedLinearColorA.White);
        _styles = styleResolver.Resolve(new[] {
            new StyleDefinition()
            {
                WordWrap = WordWrap.NoWrap
            }
        });

        _mainWindow = mainWindow;
        _device = device;
        _barBorder = new UiRectangle(shapeRenderer2d);
        _barUnfilled = new UiRectangle(shapeRenderer2d);
        _barFilled = new UiRectangle(shapeRenderer2d);

        SetImage("art/splash/legal0322.img");

        _barBorder.SetColor(0xFF808080);
        _barUnfilled.SetColor(0xFF1C324E);
        _barFilled.SetColor(0xFF1AC3FF);

        _mainWindow.UiCanvasSizeChanged += Layout;
        Layout();
    }

    public void Dispose()
    {
        _imageFile?.Dispose();
        _imageFile = null;
        _mainWindow.UiCanvasSizeChanged -= Layout;
    }

    public void SetMessageId(int messageId)
    {
        if (!_messagesLoaded)
        {
            _messages = Tig.FS.ReadMesFile("mes/loadscreen.mes");
            _messagesLoaded = true;
        }

        if (_messages.TryGetValue(messageId, out var message))
        {
            Message = message;
        }
        else
        {
            Message = $"Unknown Message ID: {messageId}";
        }
    }

    public void SetImage(string imagePath)
    {
        _imageFile?.Dispose();
        _imageFile = new WidgetImage(imagePath);
        Layout();
    }

    public void Update()
    {
        if (_imageFile == null)
        {
            return;
        }

        _device.BeginFrame();
        _device.BeginDraw();
        _imageFile.Render(PointF.Empty);
        _barBorder.Render();
        _barUnfilled.Render();
        _barFilled.Render();

        if (Message.Length > 0)
        {
            var extents = new RectangleF {X = _barBorder.GetX(), Y = _barBorder.GetY() + BarHeight + 5};

            _device.TextEngine.RenderText(
                extents,
                _styles,
                Message
            );
        }

        _device.EndDraw();
        _device.Present();

        Tig.EventLoop.Tick();
    }

    private void Layout()
    {
        var screenSize = _mainWindow.UiCanvasSize;
        var centerX = screenSize.Width / 2.0f;
        var centerY = screenSize.Height / 2.0f;

        var imageSize = _imageFile.GetPreferredSize();
        var imgX = (int) (centerX - imageSize.Width / 2.0f);
        var imgY = (int) (centerY - imageSize.Height / 2.0f);
        _imageFile.SetBounds(new RectangleF(new PointF(imgX, imgY), imageSize));

        var barY = imgY + 20 + imageSize.Height;
        var barX = (int) (centerX - BarWidth / 2.0f);

        // Set up the border
        _barBorder.SetX(barX);
        _barBorder.SetY(barY);
        _barBorder.SetWidth(BarWidth);
        _barBorder.SetHeight(BarHeight);

        // Set up the background
        _barUnfilled.SetX(barX + BarBorderSize);
        _barUnfilled.SetY(barY + BarBorderSize);
        _barUnfilled.SetWidth(BarWidth - BarBorderSize * 2);
        _barUnfilled.SetHeight(BarHeight - BarBorderSize * 2);

        // Set up the filling (width remains unset)
        _barFilled.SetX(barX + BarBorderSize);
        _barFilled.SetY(barY + BarBorderSize);
        _barFilled.SetHeight(BarHeight - BarBorderSize * 2);
        UpdateFilledBarWidth();
    }

    private void UpdateFilledBarWidth()
    {
        var fullWidth = BarWidth - BarBorderSize * 2;
        _barFilled.SetWidth((int) (fullWidth * Math.Min(1.0f, _progress)));
    }
}

/// <summary>
/// Draws a rectangle on screen.
/// </summary>
internal class UiRectangle
{
    private readonly PackedLinearColorA[] _colors = new PackedLinearColorA[4];
    private readonly ShapeRenderer2d _shapeRenderer2d;

    private Render2dArgs _args;

    public UiRectangle(ShapeRenderer2d shapeRenderer2d)
    {
        _shapeRenderer2d = shapeRenderer2d;
        _args.vertexColors = _colors;
        _args.flags = Render2dFlag.VERTEXCOLORS;
    }

    public void SetX(float x)
    {
        _args.srcRect.X = x;
        _args.destRect.X = x;
    }

    public void SetY(float y)
    {
        _args.srcRect.Y = y;
        _args.destRect.Y = y;
    }

    public float GetX()
    {
        return _args.srcRect.X;
    }

    public float GetY()
    {
        return _args.srcRect.Y;
    }

    public void SetWidth(int width)
    {
        _args.srcRect.Width = width;
        _args.destRect.Width = width;
    }

    public void SetHeight(int height)
    {
        _args.srcRect.Height = height;
        _args.destRect.Height = height;
    }

    public void SetColor(uint color)
    {
        var packedColor = new PackedLinearColorA(color);
        Array.Fill(_colors, packedColor);

        if (packedColor.A != 255)
        {
            _args.flags |= Render2dFlag.VERTEXALPHA;
        }
        else
        {
            _args.flags &= ~Render2dFlag.VERTEXALPHA;
        }
    }

    public void Render()
    {
        _shapeRenderer2d.DrawRectangle(ref _args);
    }
}