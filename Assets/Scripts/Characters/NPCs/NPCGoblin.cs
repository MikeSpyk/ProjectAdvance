using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGoblin : NPC_Base
{
    [SerializeField] private float m_randomMoveDistance = 5f;
    [SerializeField] private float m_randomMoveIntervalMin = 3f;
    [SerializeField] private float m_randomMoveIntervalMax = 4f;
    [SerializeField] private float m_playerAggroRange = 5f;
    [SerializeField] private float m_startAttackRange = 1f;
    [SerializeField] private float m_normalSpeed = 3.5f;
    [SerializeField] private float m_combatSpeed = 5f;
    [SerializeField] private float m_jumpSpeed = 10f;
    [SerializeField] private float m_minPlayerDistance = 0.1f;
    [SerializeField] private float m_jumpDistance = 1f;
    [SerializeField] private float m_damage = 10f;
    [SerializeField] private float m_jumpRecoveringTime = 1f;
    [SerializeField] private Animator m_animator;
    [SerializeField] private BasicTools.Animations.AnimationEventReceiver m_animationEventReceiver;
    [SerializeField] private GameobjectListener m_weaponListener;

    private float m_lastTimeRandomMove = float.MinValue;
    private float m_jumpRecoveryStartTime = 0f;
    private PlayerController m_currentPlayer = null;
    private bool m_isTargetingEnemy = false;
    private bool m_isAttacking = false;
    private bool m_hurtEnemyThisAttack = false;
    private bool m_isRecoveringJump = false;

    protected override void Awake()
    {
        base.Awake();
        base.destroyed += onDeath;
    } 

    // Start is called before the first frame update
    void Start()
    {
        m_animationEventReceiver.AnimationEventFired += onAnimationEventFired;
        m_weaponListener.m_onTriggerStayAction = new System.Action<Collider>(OnTriggerStayWeapon);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if(navMeshAgent.velocity.magnitude > 0.001f)
        {
            m_animator.SetBool("IsMoving",true);
        }
        else
        {
            m_animator.SetBool("IsMoving",false);
        }

        if(m_currentPlayer == null)
        {
            m_currentPlayer = CustomGameManager.Singleton.getActivePlayerController();
        }

        if(m_isTargetingEnemy)
        {
            m_animator.SetBool("IsInCombat",true);

            if(m_isRecoveringJump)
            {
                if(Time.time > m_jumpRecoveryStartTime + m_jumpRecoveringTime)
                {
                    m_isRecoveringJump = false;
                    Debug.DrawRay(transform.position, Vector3.up, Color.red);
                }
            }
            else if(m_isAttacking) // is jumping towards player
            {

            }
            else // go towards player
            {
                navMeshAgent.speed = m_combatSpeed;

                if(m_currentPlayer != null)
                {
                    if(Vector3.Distance( m_currentPlayer.transform.position,transform.position) < m_startAttackRange)
                    {
                        m_hurtEnemyThisAttack = false;
                        m_animator.SetBool("Attack1",true);
                        m_isAttacking = true;
                        navMeshAgent.speed = m_jumpSpeed;
                        navMeshAgent.destination = transform.position + (m_currentPlayer.transform.position - transform.position).normalized * m_jumpDistance;
                        
                        if (BasicTools.Random.RandomValuesSeed.getRandomBoolProbability(Time.time * 0.31f, 50))
                        {
                            BasicTools.Audio.SoundManager.singleton.playSoundAt(25, transform.position);
                        }
                    }
                    else
                    {
                        Vector3 toPlayerVec = m_currentPlayer.transform.position - transform.position;
                        if(toPlayerVec == Vector3.zero)
                        {
                            toPlayerVec = transform.forward;
                        }

                        float toPlayerDistance = toPlayerVec.magnitude;
                        navMeshAgent.SetDestination(transform.position + toPlayerVec.normalized * (toPlayerDistance - m_minPlayerDistance));
                    }
                }
            }
        }
        else
        {
            m_animator.SetBool("IsInCombat",false);
            navMeshAgent.speed = m_normalSpeed;

            if(m_currentPlayer != null)
            {
                if(Vector3.Distance( m_currentPlayer.transform.position,transform.position) < m_playerAggroRange)
                {
                    m_isTargetingEnemy = true;
                    BasicTools.Audio.SoundManager.singleton.playSoundAt(24, transform.position);
                }
                else
                {
                    float nextMoveTime = m_lastTimeRandomMove + m_randomMoveIntervalMin + (m_randomMoveIntervalMax - m_randomMoveIntervalMin) * BasicTools.Random.RandomValuesSeed.getRandomValueSeed(getRandomSeed());

                    // random movement
                    if(Time.time > nextMoveTime)
                    {
                        navMeshAgent.SetDestination(transform.position + (BasicTools.Random.RandomValuesSeed.getRandomRotationYAxis(getRandomSeed()) * Vector3.forward) * m_randomMoveDistance);
                        m_lastTimeRandomMove = Time.time;
                    }
                }
            }
        }
    }

    private float getRandomSeed()
    {
        return Time.time + transform.position.x;
    }

    private void onAnimationEventFired(object sender, string message)
    {
        if (message.Equals("AnimationAttackEnd"))
        {
            m_isAttacking = false;
            m_animator.SetBool("Attack1",false);
            navMeshAgent.speed = m_combatSpeed;
            m_isRecoveringJump = true;
            m_jumpRecoveryStartTime = Time.time;
        }
        else if(message.Equals("AttackSwingStart"))
        {
            BasicTools.Audio.SoundManager.singleton.playSoundAt(10, transform.position);
        }
        else if (message.Equals("FootHitGround"))
        {
            int soundIndex =  BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.31f, 0.31f, 0, 9);
            BasicTools.Audio.SoundManager.singleton.playSoundAt(soundIndex, transform.position, BasicTools.Audio.Sound.SoundPlaystyle.Once);
        }
        else
        {
            Debug.Log("onAnimationEventFired:"+message);
        }
    }

    private void OnTriggerStayWeapon(Collider other)
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if(target != null)
        {
            if(m_hurtEnemyThisAttack == false && m_isAttacking)
            {
                Damageable player = target as Damageable;
                player.onDamage(m_weaponListener.GetCollider().ClosestPoint(other.transform.position),new DamageData{m_amount= m_damage,m_damageType= DamageTypes.Physical});
                m_hurtEnemyThisAttack = true;
                BasicTools.Audio.SoundManager.singleton.playSoundAt(16, transform.position);
            }
        }
    }

    protected void onDeath(object sender, Damageable sender2)
    {
        base.destroyed -= onDeath;
        GameObject drop = ItemsInterface.singleton.spawnItemDropWorld(3);
        drop.transform.position = transform.position;
    }

}
