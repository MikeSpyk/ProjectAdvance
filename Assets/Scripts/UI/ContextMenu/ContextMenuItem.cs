using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ContextMenuItem : ContextMenuItemBase
{
    public ContextMenuItem (string text, Action<ContextMenuItem> action) : base(text)
    {
        m_action = action;
    }

    private Action<ContextMenuItem> m_action;

    public void doAction()
    {
        m_action(this);
    }
}
