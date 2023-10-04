using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AIMindSystem;

public class NPC_Base : Lifeform
{
    private NavMeshAgent m_navMeshAgent;
    protected Goal m_GoalSystem;

    protected NavMeshAgent navMeshAgent
    {
        get
        {
            return m_navMeshAgent;
        }
    }

    protected virtual void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
    }
}
