using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifeform : Damageable
{
    protected float m_averageSpeed = float.NaN;
    public float averageSpeed
    {
        get
        {
            #if UNITY_EDITOR
                if(m_averageSpeed == float.NaN)
                {
                    Debug.LogError("Uninitialized property in Lifeform: m_averageSpeed. assign Value in derived class \""+this.GetType().Name+"\"! (Gameobject: \""+gameObject.name + "\")");
                }
            #endif

            return m_averageSpeed;
        }   
    }
    protected float m_averageDamage = 0f;
    public float averageDamage
    {
        get
        {
            #if UNITY_EDITOR
                if(m_averageDamage == float.NaN)
                {
                    Debug.LogError("Uninitialized property in Lifeform: m_averageDamage. assign Value in derived class \""+this.GetType().Name+"\"! (Gameobject: \""+gameObject.name + "\")");
                }
            #endif

            return m_averageDamage;
        }
    }
    protected float m_attackRange = 0f;
    public float attackRange
    {
        get
        {
            #if UNITY_EDITOR
                if(m_attackRange == float.NaN)
                {
                    Debug.LogError("Uninitialized property in Lifeform: m_attackRange. assign Value in derived class \""+this.GetType().Name+"\"! (Gameobject: \""+gameObject.name + "\")");
                }
            #endif

            return m_attackRange;
        }
    }
}
