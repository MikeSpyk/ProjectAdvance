using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIMindSystem;

public class NPCHumanBandit : NPCHuman
{
    [SerializeField] private bool m_isLeader = false;

    protected virtual void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected virtual void Update()
    {
        base.Update();
    }

    protected virtual void Awake()
    {
        base.Awake();
    }

    protected override void createAIMindGoalSystem()
    {
        m_GoalSystem = new AIMindSystem.Goal(gameObject.name, new AIMindSystem.Goal_Base[]
        {
            new ActionableGoal("FightEnemies", calculateFightEnemiesUrgency, new Action[]{Action.Attack}),
            new ActionableGoal("FollowLeader", calculateFollowLeaderUrgency, new Action[]{Action.FindLeader, Action.Follow}),
            new ActionableGoal("AttackEnemyTown", calculateAttackEnemyTownUrgency, new Action[]{Action.FindEnemyTown, Action.GoToPosition, Action.PatrolInCircle})
        });
    }

    protected override float calculateFightEnemiesUrgency(Goal_Base sender)
    {
        float urgency = 0f;

        List<Lifeform> enemyLifeforms = new List<Lifeform>();

        for(int i = 0; i < m_lifeformsPerceived.Count; i++)
        {
            if(m_lifeformsPerceived[i].GetType() != typeof(NPCHumanBandit) )
            {
                enemyLifeforms.Add(m_lifeformsPerceived[i]);
            }
        }

        if(enemyLifeforms.Count > 0)
        {
            urgency = 1f;
        }
        else
        {
            urgency = 0f;
        }

        return urgency;
    }

    protected float calculateFollowLeaderUrgency(Goal_Base sender)
    {
        return 0.5f;
    }

    protected float calculateAttackEnemyTownUrgency(Goal_Base sender)
    {
        if(m_isLeader)
        {
            return 0.6f;
        }
        else
        {
            return 0f;
        }
    }

    protected override bool executeMindAIGoalAction(Action currentAction)
    {
        switch(currentAction)
        {
            case Action.Attack:
            {
                Lifeform closestsEnemy = null;
                float closestsEnemyDistance = float.MaxValue;
                float tempDistance;

                for(int i = 0; i < m_lifeformsPerceived.Count; i++)
                {
                    if(m_lifeformsPerceived[i].GetType() != typeof(NPCHumanBandit))
                    {
                        tempDistance = Vector3.Distance(transform.position, m_lifeformsPerceived[i].transform.position);

                        if(tempDistance < closestsEnemyDistance)
                        {
                            closestsEnemyDistance = tempDistance;
                            closestsEnemy = m_lifeformsPerceived[i];
                        }
                    }
                }

                if(closestsEnemy != null)
                {
                    navMeshAgent.destination = closestsEnemy.transform.position;

                    if(Vector3.Distance(transform.position,closestsEnemy.transform.position) < m_startPunchRange)
                    {
                        if(m_activeWeaponScript != null)
                        {
                            m_activeWeaponScript.onStartAttack();
                        }

                        m_animator.SetBool("IsAttacking",true);
                    }
                }

                break;
            }
            case Action.FindLeader:
            {
                NPCHumanoidBase leader = societyManager.getLeader();

                if(leader == this)
                {
                    m_isLeader = true;
                }

                if(leader == null)
                {
                    return false;
                }
                else
                {
                    m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{leader.gameObject}));
                    return true;
                }
            }
            case Action.Follow:
            {
                if(m_currentActionQueueIndex < 1)
                {
                    Debug.LogWarning("index out of expected range");
                    return false;
                }

                GameObject toFollow = m_currentActionQueueResults[m_currentActionQueueIndex-1].Item2[0] as GameObject;

                if(toFollow == null)
                {
                    resetGoalActionQueueProgess();
                    return false;
                }

                if(navMeshAgent.destination != toFollow.transform.position)
                {
                    navMeshAgent.destination = toFollow.transform.position;
                }

                if(Vector3.Distance(transform.position, toFollow.transform.position) > 0.5f)
                {
                    if(navMeshAgent.destination != toFollow.transform.position)
                    {
                        navMeshAgent.destination = toFollow.transform.position;
                    }
                }

                break;
            }
            case Action.FindEnemyTown:
            {
                Vector3 target = societyManager.getEnemyTown().transform.position;

                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{target}));
                return true;
            }
            default:
            {
                return base.executeMindAIGoalAction(currentAction);
            }
        }

        return false;
    }

}
