using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuItemParent : ContextMenuItemBase
{
    public ContextMenuItemParent(string text, ContextMenuItemBase[] children) : base(text)
    {
        m_children = children;

        if(m_children == null || m_children.Length < 1)
        {
            throw new System.NotSupportedException("ContextMenuItemParent: No children provided.");
        }
    }

    private readonly ContextMenuItemBase[] m_children;
    public ContextMenuItemBase[] children {get{ return m_children;}}
}
