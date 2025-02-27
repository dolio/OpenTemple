using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Cursors;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.InGame;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui;

/// <summary>
/// Shows a view of the ingame world in the user interface. Handles rendering of the world and
/// interacting with object in the world.
/// </summary>
public class GameView : WidgetContainer, IGameViewport
{
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 2.0f;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly GameViewScrollingController _scrollingController;

    private readonly RenderingDevice _device;

    private readonly GameRenderer _gameRenderer;

    public bool IsMouseScrollingEnabled
    {
        get => _scrollingController.IsMouseScrolling;
        set => _scrollingController.IsMouseScrolling = value;
    }

    WorldCamera IGameViewport.Camera
    {
        get
        {
            // We need to ensure pending layout is performed before accessing the camera
            // otherwise the cameras viewport size may be out of date
            if (!HasValidLayout)
            {
                EnsureLayoutIsUpToDate();
            }

            return _camera;
        }
    }

    [Obsolete]
    public GameRenderer GameRenderer => _gameRenderer;

    private event Action? _onResize;

    private float _renderScale;

    private float _zoom = 1f;

    public float Zoom => _zoom;

    SizeF IGameViewport.Size => ContentArea.Size;

    public bool IsInteractive => true;

    event Action IGameViewport.OnResize
    {
        add => _onResize += value;
        remove => _onResize -= value;
    }

    private readonly IMainWindow _mainWindow;

    private bool _isUpscaleLinearFiltering;
    private readonly WorldCamera _camera = new();

    public GameView(IMainWindow mainWindow, RenderingDevice device, RenderingConfig config)
    {
        // Usually game views are fully sized to their parent
        Width = Dimension.Percent(100);
        Height = Dimension.Percent(100);

        _device = device;
        _gameRenderer = new GameRenderer(_device, this);
        HitTesting = HitTestingMode.Area; // The entire area represents the in-game UI

        _mainWindow = mainWindow;

        ReloadConfig(config);

        GameViews.Add(this);

        Width = Dimension.Percent(100);
        Height = Dimension.Percent(100);

        _scrollingController = new GameViewScrollingController(this, this);

        Globals.ConfigManager.OnConfigChanged += OnConfigChange;

        // The order of these does matter
        UiSystems.RadialMenu.AddEventListeners(this);
        UiSystems.TurnBased.AddEventListeners(this);
        UiSystems.InGameSelect.AddEventListeners(this);
        UiSystems.InGame.AddEventListeners(this);

        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        // Hotkeys from the Key-Manager are only relevant when the Radial Menu isn't open
        foreach (var hotkeyAction in UiSystems.KeyManager.EnumerateHotkeyActions())
        {
            AddActionHotkey(
                hotkeyAction.Hotkey,
                hotkeyAction.Callback,
                () => IsInteractive && !UiSystems.RadialMenu.IsOpen && hotkeyAction.Condition()
            );
        }

        // Hotkeys for turn-based combat also only work while the radial menu is closed,
        // and while turn-based combat is active
        foreach (var hotkeyAction in UiSystems.TurnBased.EnumerateHotkeyActions())
        {
            AddActionHotkey(
                hotkeyAction.Hotkey,
                hotkeyAction.Callback,
                () => IsInteractive && !UiSystems.RadialMenu.IsOpen && GameSystems.Combat.IsCombatActive() && hotkeyAction.Condition()
            );
        }

        // Hotkeys from the radial menu on the other hand are only available while it's open
        foreach (var hotkeyAction in UiSystems.RadialMenu.EnumerateHotkeyActions())
        {
            AddActionHotkey(
                hotkeyAction.Hotkey,
                hotkeyAction.Callback,
                () => IsInteractive && UiSystems.RadialMenu.IsOpen && hotkeyAction.Condition()
            );
        }

        foreach (var heldHotkey in UiSystems.RadialMenu.EnumerateHeldHotkeys())
        {
            // Wrap the condition to check that the radial menu is open
            AddHeldHotkey(
                heldHotkey.Hotkey,
                state =>
                {
                    heldHotkey.Held = state;
                    heldHotkey.Callback(state);
                },
                () => IsInteractive && UiSystems.RadialMenu.IsOpen && heldHotkey.Condition()
            );
        }
    }

    private void OnConfigChange()
    {
        ReloadConfig(Globals.Config.Rendering);
    }

    private void ReloadConfig(RenderingConfig config)
    {
        _isUpscaleLinearFiltering = config.IsUpscaleLinearFiltering;
        _renderScale = config.RenderScale;
        _gameRenderer.MultiSampleSettings = new MultiSampleSettings(
            config.IsAntiAliasing,
            config.MSAASamples,
            config.MSAAQuality
        );
        OnAfterLayout();
    }

    public void RenderScene()
    {
        if (!Visible || !IsInTree)
        {
            return;
        }

        if (!GameViews.IsDrawingEnabled)
        {
            return;
        }

        using var _ = _device.CreatePerfGroup("Updating GameView {0}", Name);

        try
        {
            _gameRenderer.Render();
        }
        catch (Exception e)
        {
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }
    }

    public override void Render(UiRenderContext context)
    {
        if (!Visible)
        {
            return;
        }

        var sceneTexture = _gameRenderer.SceneTexture;
        if (sceneTexture == null)
        {
            return;
        }

        var samplerType = SamplerType2d.Clamp;
        if (!_isUpscaleLinearFiltering)
        {
            samplerType = SamplerType2d.Point;
        }

        Tig.ShapeRenderer2d.DrawRectangle(
            GetViewportPaddingArea(),
            sceneTexture,
            PackedLinearColorA.White,
            samplerType
        );

        UiSystems.TurnBased.Render(this);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GameViews.Remove(this);
    }

    private void UpdateSceneSize()
    {
        var renderSize = new Size(
            (int) (ContentArea.Width * _mainWindow.UiScale * _renderScale),
            (int) (ContentArea.Height * _mainWindow.UiScale * _renderScale)
        );

        if (renderSize != _gameRenderer.RenderSize)
        {
            // We're trying to render at native resolutions by default, hence we apply
            // the UI scale to determine the render target size here.
            _gameRenderer.RenderSize = renderSize;
            Logger.Debug("Rendering @ {0}x{1} ({2}%), MSAA: {3}",
                _gameRenderer.RenderSize.Width,
                _gameRenderer.RenderSize.Height,
                (int) (_renderScale * 100),
                _gameRenderer.MultiSampleSettings
            );
        }

        UpdateCamera();
    }

    protected override void OnAfterLayout()
    {
        base.OnAfterLayout();
        UpdateSceneSize();
    }

    public void TakeScreenshot(string path, Size size = default)
    {
        _gameRenderer.TakeScreenshot(path, size.Width, size.Height);
    }

    public MapObjectRenderer GetMapObjectRenderer()
    {
        return _gameRenderer.GetMapObjectRenderer();
    }

    protected override void HandleMouseMove(MouseEvent e)
    {
        _scrollingController.MouseMoved(e.GetLocalPos(this));
    }

    protected override void HandleMouseDown(MouseEvent e)
    {
        if (e.Button == MouseButton.Middle)
        {
            if (_scrollingController.MiddleMouseDown(e.GetLocalPos(this)))
            {
                e.StopImmediatePropagation();
            }
        }
    }

    protected override void HandleMouseUp(MouseEvent e)
    {
        if (e.Button == MouseButton.Middle)
        {
            _scrollingController.MiddleMouseUp();
        }
    }

    protected override void HandleMouseWheel(WheelEvent e)
    {
        _zoom = Math.Clamp(_zoom + Math.Sign(e.DeltaY) * 0.1f, MinZoom, MaxZoom);
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        var size = new SizeF(
            ContentArea.Width / _zoom,
            ContentArea.Height / _zoom
        );

        if (size != _camera.ViewportSize)
        {
            // Using IGameViewport.CenteredOn will be wrong here if the zoom just changed
            var previousCenter = _camera.CenteredOn;

            _camera.ViewportSize = size;

            // See also @ 0x10028db0
            _camera.CenterOn(previousCenter);

            // Ensure the primary translation is updated, otherwise the next scroll event will undo what we just did
            if (GameViews.Primary == this)
            {
                var dt = _camera.Get2dTranslation();
                GameSystems.Location.LocationTranslationX = (int) dt.X;
                GameSystems.Location.LocationTranslationY = (int) dt.Y;
                // This clamps the translation to the scroll limit
                GameSystems.Scroll.ScrollBy(this, 0, 0);
            }

            _onResize?.Invoke();
        }
    }

    public override void OnUpdateTime(TimePoint now)
    {
        base.OnUpdateTime(now);
        _scrollingController.UpdateTime(now);
    }

    private static readonly Dictionary<ActionCursor, string> CursorPaths = new()
    {
        {ActionCursor.AttackOfOpportunity, CursorIds.AttackOfOpportunity},
        {ActionCursor.AttackOfOpportunityGrey, CursorIds.AttackOfOpportunityGrey},
        {ActionCursor.Sword, CursorIds.Sword},
        {ActionCursor.Arrow, CursorIds.Arrow},
        {ActionCursor.FeetGreen, CursorIds.MoveGreen},
        {ActionCursor.FeetYellow, CursorIds.MoveYellow},
        {ActionCursor.SlidePortraits, CursorIds.SlideHorizontal},
        {ActionCursor.Locked, CursorIds.Locked},
        {ActionCursor.HaveKey, CursorIds.UseKey},
        {ActionCursor.UseSkill, CursorIds.UseSkill},
        {ActionCursor.UsePotion, CursorIds.UsePotion},
        {ActionCursor.UseSpell, CursorIds.UseSpell},
        {ActionCursor.UseTeleportIcon, CursorIds.OpenHand},
        {ActionCursor.HotKeySelection, CursorIds.AssignHotkey},
        {ActionCursor.Talk, CursorIds.Talk},
        {ActionCursor.IdentifyCursor, CursorIds.Identify},
        {ActionCursor.IdentifyCursor2, CursorIds.Identify},
        {ActionCursor.ArrowInvalid, CursorIds.ArrowInvalid},
        {ActionCursor.SwordInvalid, CursorIds.SwordInvalid},
        {ActionCursor.ArrowInvalid2, CursorIds.ArrowInvalid},
        {ActionCursor.FeetRed, CursorIds.MoveRed},
        {ActionCursor.FeetRed2, CursorIds.MoveRed},
        {ActionCursor.InvalidSelection, CursorIds.InvalidSelection},
        {ActionCursor.Locked2, CursorIds.Locked},
        {ActionCursor.HaveKey2, CursorIds.UseKey},
        {ActionCursor.UseSkillInvalid, CursorIds.UseSkillInvalid},
        {ActionCursor.UsePotionInvalid, CursorIds.UsePotionInvalid},
        {ActionCursor.UseSpellInvalid, CursorIds.UseSpellInvalid},
        {ActionCursor.PlaceFlag, CursorIds.PlaceFlag},
        {ActionCursor.HotKeySelectionInvalid, CursorIds.AssignHotkeyInvalid},
        {ActionCursor.InvalidSelection2, CursorIds.InvalidSelection},
        {ActionCursor.InvalidSelection3, CursorIds.InvalidSelection},
        {ActionCursor.InvalidSelection4, CursorIds.InvalidSelection},
    };

    protected override void HandleGetCursor(GetCursorEvent e)
    {
        var localPos = e.GetLocalPos(this);

        // Mouse-Scrolling
        if (IsInteractive && _scrollingController.TryGetMouseScrollingDirection(localPos, out var direction))
        {
            e.Cursor = direction switch
            {
                ScrollDirection.UP => CursorIds.ScrollUp,
                ScrollDirection.UP_RIGHT => CursorIds.ScrollUpRight,
                ScrollDirection.RIGHT => CursorIds.ScrollRight,
                ScrollDirection.DOWN_RIGHT => CursorIds.ScrollDownRight,
                ScrollDirection.DOWN => CursorIds.ScrollDown,
                ScrollDirection.DOWN_LEFT => CursorIds.ScrollDownLeft,
                ScrollDirection.LEFT => CursorIds.ScrollLeft,
                ScrollDirection.UP_LEFT => CursorIds.ScrollUpLeft,
                _ => throw new ArgumentOutOfRangeException()
            };
            return;
        }

        var actionCursor = GameSystems.D20.Actions.CurrentCursor;
        if (actionCursor != ActionCursor.Undefined)
        {
            if (CursorPaths.TryGetValue(actionCursor, out var cursorId))
            {
                e.Cursor = cursorId;
            }
            else
            {
                Logger.Error("Unknown D20 action cursor: {0}", actionCursor);
            }
        }
    }
}