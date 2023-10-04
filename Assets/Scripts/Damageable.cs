using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Damageable : MonoBehaviour
{
    [SerializeField] private bool m_invulnerable = false;
    [SerializeField] public float m_health = 100f;
    [SerializeField] private string[] m_groups = null;

    public float health {get{return m_health;}}
    public event EventHandler<Damageable> destroyed;
    public event EventHandler<Damageable> damaged;

    
    private HashSet<string> m_groupsHashSet = null;

    private void fireDestroyed()
    {
        destroyed?.Invoke(this, this);
    }

    private void fireDamaged()
    {
        damaged?.Invoke(this, this);
    }

    protected void kill()
    {
        onDamage(transform.position, new DamageData(){ m_amount= float.MaxValue, m_damageType= DamageTypes.Physical} );
    }

    public virtual void onDamage(Vector3 position, params DamageData[] damageData)
    {
        float damage = 0f;

        for(int i = 0; i < damageData.Length; i++)
        {
            damage += damageData[i].m_amount;
        }

        WorldTextManager.Singleton.showText(damage.ToString("0"), position, transform, Color.red);

        if(!m_invulnerable)
        {
            m_health -= damage;

            if(m_health < 0f)
            {
                fireDestroyed();
                Destroy(this.gameObject);
            }

            fireDamaged();
        }
    }

    public bool hasSameGroup(Damageable source)
    {
        if(m_groups != null)
        {
            for(int i = 0; i < m_groups.Length; i++)
            {
                if(source.hasGroup(m_groups[i]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool hasGroup(string name)
    {
        if(m_groupsHashSet == null)
        {
            updateGroupsHashSet();
        }

        return m_groupsHashSet.Contains(name);
    }

    private void updateGroupsHashSet()
    {
        m_groupsHashSet = new HashSet<string>();

        if(m_groups != null)
        {
            for(int i = 0; i < m_groups.Length; i++)
            {
                m_groupsHashSet.Add(m_groups[i]);
            }
        }
    }

}
