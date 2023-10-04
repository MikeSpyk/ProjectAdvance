using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameobjectListener : MonoBehaviour
{
    private void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    public Action<Collider> m_onTriggerStayAction = null;
    private Collider m_collider;
    
    public Collider GetCollider()
    {
        return m_collider;
    }

    private void OnTriggerStay(Collider other)
    {
        if(m_onTriggerStayAction != null)
        {
            m_onTriggerStayAction(other);
        }
    }
}
