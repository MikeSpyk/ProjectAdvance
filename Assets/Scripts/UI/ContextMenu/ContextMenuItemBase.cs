using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuItemBase
{
    protected ContextMenuItemBase (string text)
    {
        m_text = text;
    }

    public string m_text;
    public int m_depth = -1;
    public UnityEngine.UI.Button m_associatedButton = null;
    public ContextMenuItemParent m_parent = null;
}
