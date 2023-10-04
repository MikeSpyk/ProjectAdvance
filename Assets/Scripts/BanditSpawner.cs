using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditSpawner : MonoBehaviour
{
    [SerializeField] private SocietyManager m_society;
    [SerializeField] private GameObject m_banditPrefab;
    [SerializeField] private int m_banditCount = 5;
    [SerializeField] private float m_timeBetweenAttacks = 100f;

    private float m_lastTimeAttack = 0f;

    void FixedUpdate()
    {
        if(Time.time > m_lastTimeAttack + m_timeBetweenAttacks)
        {
            spawnBanditGroup();
            m_lastTimeAttack = Time.time;
        }
    }

    private void spawnBanditGroup()
    {
        for(int i = 0; i < m_banditCount; i++)
        {
            GameObject banditObj = Instantiate(m_banditPrefab, transform.position, Quaternion.identity);
            NPCHumanBandit bandit = banditObj.GetComponent<NPCHumanBandit>();

            bandit.setSociety(m_society);
        }
    }
}
