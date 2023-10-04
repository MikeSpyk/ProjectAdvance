using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocietyManager : MonoBehaviour
{
    [SerializeField] private PickUpWaterSource m_waterSource;
    [SerializeField] private SpawnerManager m_foodSpawner;
    [SerializeField] private GameObject m_enemyTownPosition; // only for bandits

    private List<NPCHumanoidBase> m_societyMembers = new List<NPCHumanoidBase>();
    private NPCHumanoidBase m_currentLeader = null;

    public void registerNewMember(NPCHumanoidBase member)
    {
        m_societyMembers.Add(member);
    }

    public void unregisterMember(NPCHumanoidBase member)
    {
        if(m_societyMembers.Contains(member))
        {
            m_societyMembers.Remove(member);
        }
    }

    public PickUpWaterSource getWaterSource()
    {
        return m_waterSource;
    }

    public GameObject[] getFoodSources()
    {
        return m_foodSpawner.getChildren();
    }

    public NPCHumanoidBase getLeader()
    {
        if(m_currentLeader == null)
        {
            if(m_societyMembers.Count > 0)
            {
                m_currentLeader = m_societyMembers[0];
            }
            else
            {
                Debug.LogWarning("Cannot decide new Leader. society has no members");
            }
        }
        
        return m_currentLeader;
    }

    public GameObject getEnemyTown()
    {
        return m_enemyTownPosition;
    }

    public List<NPCHuman> getMembers()
    {
        List<NPCHuman> result = new List<NPCHuman>();

        for(int i = 0; i < m_societyMembers.Count; i++)
        {
            result.Add(m_societyMembers[i] as NPCHuman);
        }

        return result;
    }
}
