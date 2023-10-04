using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPCSlime : NPC_Base
{
    [SerializeField] private float m_damage = 2f;
    [SerializeField] private float m_randomMoveDistance = 5f;
    [SerializeField] private float m_randomMoveIntervalMin = 3f;
    [SerializeField] private float m_randomMoveIntervalMax = 4f;
    [SerializeField] private float m_deaggroRange = 5f;
    [SerializeField] private float m_startAttackRange = 1f;
    [SerializeField] private float m_attackRotateSpeed = 1f;
    [SerializeField] private Animator m_animator;
    [SerializeField] private BasicTools.Triggers.GenericTriggerEvents m_aggroTrigger;
    [SerializeField] private BasicTools.Triggers.GenericTriggerEvents m_attackTrigger;
    [SerializeField] private BasicTools.Animations.AnimationEventReceiver m_animationEventReceiver;

    private float m_lastTimeRandomMove = float.MinValue;
    private Damageable m_currentAttackTarget = null;
    private bool m_isTargetingEnemy = false;
    private Collider m_collider;
    private bool m_hurtEnemyThisAttack = false;
    private List<Damageable> m_aggroedBy = new List<Damageable>();
    private List<Damageable> m_enemiesInDamageRange = new List<Damageable>();

    protected override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<Collider>();

        m_averageSpeed = navMeshAgent.speed;
        m_averageDamage = m_damage;
        m_attackRange = m_startAttackRange;

        m_aggroTrigger.triggerEntered += onAggroTriggerEnter;
        m_attackTrigger.triggerEntered += onAttackTriggerEnter;
        m_attackTrigger.triggerExit += onAttackTriggerExited;
        base.destroyed += onDeath;

        m_animationEventReceiver.AnimationEventFired += onAnimationEventFired;
    }

    private float getRandomSeed()
    {
        return Time.time + transform.position.x;
    }

    void FixedUpdate()
    {
        if(m_currentAttackTarget != null)
        {
            if(Vector3.Distance(transform.position,m_currentAttackTarget.transform.position) > m_deaggroRange)
            {
                m_currentAttackTarget = null;
            }
        }

        if(m_currentAttackTarget == null)
        {
            for(int i = m_aggroedBy.Count-1; i > -1; i--)
            {
                if(m_aggroedBy[i] == null || Vector3.Distance(m_aggroedBy[i].transform.position,transform.position) > m_deaggroRange)
                {
                    m_aggroedBy.RemoveAt(i);
                }
                else
                {
                    m_currentAttackTarget = m_aggroedBy[i];
                    break;
                }
            }
        }

        if(m_currentAttackTarget == null)
        {
            m_isTargetingEnemy = false;
        }
        else
        {
            m_isTargetingEnemy = true;
        }

        if(m_isTargetingEnemy)
        {
            navMeshAgent.SetDestination(m_currentAttackTarget.transform.position);

            if(Vector3.Distance( m_currentAttackTarget.transform.position,transform.position) < m_startAttackRange)
            {
                StopAgent();
                m_animator.SetTrigger("Attack");
            }
            else
            {
                resumeAgent();
            }

            {
                float angleDif = Quaternion.LookRotation(m_currentAttackTarget.transform.position-transform.position).normalized.eulerAngles.y 
                                - Quaternion.LookRotation(transform.forward).eulerAngles.y;
                
                float rotation = m_attackRotateSpeed * Mathf.Sign(angleDif) * Time.deltaTime;

                if(Mathf.Abs(rotation) > angleDif)
                {
                    rotation = angleDif;
                }

                transform.Rotate(new Vector3(0,rotation,0));
            }
        }
        else
        {
            resumeAgent();

            float nextMoveTime = m_lastTimeRandomMove + m_randomMoveIntervalMin + (m_randomMoveIntervalMax - m_randomMoveIntervalMin) * BasicTools.Random.RandomValuesSeed.getRandomValueSeed(getRandomSeed());

            // random movement
            if(Time.time > nextMoveTime)
            {
                navMeshAgent.SetDestination(transform.position + (BasicTools.Random.RandomValuesSeed.getRandomRotationYAxis(getRandomSeed()) * Vector3.forward) * m_randomMoveDistance);
                m_lastTimeRandomMove = Time.time;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("NavmeshAgent \""+ gameObject.name +"\" is not on nav mesh. Destroying..");
            kill();
        }

        if(navMeshAgent.remainingDistance > 0.01f)
        {
            m_animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
            //m_animator.SetTrigger("Jump");
        }
        else
        {
            m_animator.SetFloat("Speed", 0);
        }

        if(!m_hurtEnemyThisAttack && m_enemiesInDamageRange.Count > 0)
        {
            m_enemiesInDamageRange[0].onDamage(m_collider.ClosestPoint(transform.position),new DamageData{m_amount= m_damage,m_damageType= DamageTypes.Physical});
            m_hurtEnemyThisAttack = true;
        }
    }

    private void StopAgent()
    {
        navMeshAgent.isStopped = true;
        m_animator.SetFloat("Speed", 0);
        navMeshAgent.updateRotation = false;
    }

    private void resumeAgent()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = true;
    }

    private void onAnimationEventFired(object sender, string eventName)
    {
        if(eventName == "JumpStart" || eventName == "AttackStart")
        {
            BasicTools.Audio.SoundManager.singleton.playSoundAt(17, transform.position);
        }
    }

    // Animation Event
    public void AlertObservers(string message)
    {
        if (message.Equals("AnimationDamageEnded"))
        {

        }

        if (message.Equals("AnimationAttackEnded"))
        {
            m_hurtEnemyThisAttack = false;
        }

        if (message.Equals("AnimationJumpEnded"))
        {

        }
    }

    protected void onDeath(object sender, Damageable sender2)
    {
        base.destroyed -= onDeath;
        GameObject drop = ItemsInterface.singleton.spawnItemDropWorld(2);
        drop.transform.position = transform.position;
    }

    private void onAggroTriggerEnter(object sender, Collider other)
    {
        Damageable target = other.GetComponent<Damageable>();

        if(target != null)
        {
            NPCSlime otherSlimeScript = target.GetComponent<NPCSlime>();

            if(otherSlimeScript == null) // other slimes do not qualifie as enemies
            {
                if(!m_aggroedBy.Contains(target))
                {
                    m_aggroedBy.Add(target);
                }
            }
        }
    }

    private void onAttackTriggerEnter(object sender, Collider other)
    {
        Damageable target = other.GetComponent<Damageable>();

        if(target != null)
        {
            if(target.GetType() != typeof(NPCSlime))
            {
                m_enemiesInDamageRange.Add(target);
                target.destroyed += onEnemyInDamageRangeDestroyed;
            }
        }
    }

    private void onAttackTriggerExited(object sender, Collider other)
    {
        Damageable target = other.GetComponent<Damageable>();

        if(m_enemiesInDamageRange.Contains(target))
        {
            m_enemiesInDamageRange.Remove(target);
        }
    }

    private void onEnemyInDamageRangeDestroyed (object sender, Damageable sender2)
    {
        sender2.destroyed -= onEnemyInDamageRangeDestroyed;
        if(m_enemiesInDamageRange.Contains(sender2))
        {
            m_enemiesInDamageRange.Remove(sender2);
        }
    }
}

