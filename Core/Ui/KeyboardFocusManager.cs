﻿using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public class KeyboardFocusManager
{
    /// <summary>
    /// The widget that is the current keyboard focus.
    /// </summary>
    public WidgetBase? KeyboardFocus { get; private set; }

    private readonly IReadOnlyList<WidgetBase> _topLevelWidgets;

    public KeyboardFocusManager(IReadOnlyList<WidgetBase> topLevelWidgets)
    {
        _topLevelWidgets = topLevelWidgets;
    }

    public void MoveFocusByKeyboard(bool backwards)
    {
        static bool CanContainFocus(WidgetBase widget) => widget is {Visible: true, Disabled: false};

        WidgetBase? candidate;
        if (_topLevelWidgets.Count == 0)
        {
            KeyboardFocus = null;
            return;
        }

        if (!backwards)
        {
            // The first focusable element when navigating forward is simply the first
            // Otherwise just start with the first one following the current keyboard focus
            candidate = KeyboardFocus == null
                ? _topLevelWidgets[0]
                : KeyboardFocus.Following(predicate: CanContainFocus);
        }
        else
        {
            // 
            candidate = KeyboardFocus == null
                ? _topLevelWidgets[^1].LastInclusiveDescendant()
                : KeyboardFocus.Preceding();
        }

        while (candidate != null)
        {
            var skipChildren = false;

            // Skip invisible and disabled elements and their children
            if (!CanContainFocus(candidate))
            {
                skipChildren = true;
            }
            else if (candidate.FocusMode == FocusMode.User)
            {
                KeyboardFocus = candidate;
                return;
            }

            candidate = !backwards ? candidate.Following(null, skipChildren) : candidate.Preceding();
        }

        // We've reached the end of the focus list
        KeyboardFocus = null;
    }
}