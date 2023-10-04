using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnDestroyCallback : MonoBehaviour
{
    public Action<GameObject> m_callbackAction = null;

    void OnDestroy()
    {
        if(m_callbackAction != null)
        {
            m_callbackAction(gameObject);
        }
        else
        {
            Debug.LogWarning("OnDestroyCallback-object without a callback-Action");
        }
    }
}
