using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCTroll : NPC_Base
{
    private enum ActState{Idle, Walk, Run, Stomp, Attack, StompRecovery, RunHome}

    [SerializeField] private float m_stompRecoveryTime = 2f;
    [SerializeField] private float m_randomStompMinTime = 5f;
    [SerializeField] private float m_randomStompMaxTime = 10f;
    [SerializeField] private float m_stompRunDistance = 10f;
    [SerializeField] private float m_walkSpeed = 1f;
    [SerializeField] private float m_runSpeed = 2f;
    [SerializeField] private float m_startStompRange = 1f;
    [SerializeField] private float m_startAttackRange = 2f;
    [SerializeField] private float m_attackRotateSpeed = 1f;
    [SerializeField] private float m_damage = 10f;
    [SerializeField] private Animator m_animator;
    [SerializeField] private BasicTools.Animations.AnimationEventReceiver m_animationEventReceiver;
    [SerializeField] private GameobjectListener m_weaponListener;
    [SerializeField] private GameobjectListener m_leftFootListener;
    [SerializeField] private GameobjectListener m_rightFootListener;
    [SerializeField] private BasicTools.Triggers.GenericTriggerEvents m_aggroTrigger;
    [SerializeField] private float m_randomSoundCooldown = 3f;
    [SerializeField] private float m_maxDistanceToHome = 500f;

    private PlayerController m_currentPlayer = null;
    private bool m_hurtEnemyThisAttack = false;
    private bool m_hurtPlayerLastFootStep = false;
    private ActState m_currentActState = ActState.Idle;
    private float m_startStompTime = 0f;
    private float m_nextTimeRandomStomp = 0f;
    private float m_stompRecoveryStartTime = 0f;
    private float m_lastTimeRandomSound = 0f;
    private Vector3 m_homePosition;
    private PlayerController m_playerInRange = null;

    protected override void Awake()
    {
        base.Awake();
        base.destroyed += onDeath;
        m_aggroTrigger.triggerEntered += onAggroTriggerEnter;
        m_aggroTrigger.triggerExit += onAggroTriggerExited;
    } 

    // Start is called before the first frame update
    void Start()
    {
        m_animationEventReceiver.AnimationEventFired += onAnimationEventFired;
        m_weaponListener.m_onTriggerStayAction = new System.Action<Collider>(OnTriggerStayWeapon);
        m_leftFootListener.m_onTriggerStayAction = new System.Action<Collider>(OnTriggerStayFoot);
        m_rightFootListener.m_onTriggerStayAction = new System.Action<Collider>(OnTriggerStayFoot);

        navMeshAgent.speed = m_walkSpeed;
        m_homePosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if(navMeshAgent.velocity.magnitude > 0.001f || m_currentActState == ActState.Stomp)
        {
            if(m_currentActState == ActState.Run || m_currentActState == ActState.Stomp || m_currentActState == ActState.RunHome)
            {
                m_animator.SetBool("IsWalking",false);
                m_animator.SetBool("IsRunning",true);
            }
            else
            {
                m_animator.SetBool("IsRunning",false);
                m_animator.SetBool("IsWalking",true);
            }
        }
        else
        {
            m_animator.SetBool("IsWalking",false);
            m_animator.SetBool("IsRunning",false);
        }

        if(m_currentPlayer == null)
        {
            m_currentActState = ActState.Idle;
        }
        else
        {
            if(m_currentActState == ActState.RunHome)
            {
                if(Vector3.Distance(transform.position, m_homePosition) < 10f)
                {
                    m_currentActState = ActState.Idle;
                    if(m_playerInRange == null)
                    {
                        m_currentPlayer = null;
                    }
                }
            }
            else if(m_currentActState == ActState.StompRecovery)
            {
                if(Time.time > m_stompRecoveryStartTime + m_stompRecoveryTime)
                {
                    m_animator.SetBool("IsTakingDamage",false);
                    m_currentActState = ActState.Idle;
                }
            }
            else if(m_currentActState == ActState.Attack)
            {
                Vector3 toPlayerDir = m_currentPlayer.transform.position - transform.position;
                Vector2 toPlayerDirXZ = new Vector2(toPlayerDir.x,toPlayerDir.z);
                float angleDif = Vector2.SignedAngle(toPlayerDirXZ, new Vector2(transform.forward.x,transform.forward.z));

                float roationAngle = m_attackRotateSpeed * Time.deltaTime * Mathf.Sign(angleDif);

                if(Mathf.Abs(angleDif) > Mathf.Abs(roationAngle))
                {
                    transform.Rotate(0,roationAngle,0);
                }
            }
            else if(m_currentActState == ActState.Stomp)
            {
                if(Vector3.Distance(transform.position, navMeshAgent.destination) < 2f)
                {
                    m_currentActState = ActState.StompRecovery;
                    m_stompRecoveryStartTime = Time.time;
                    m_animator.SetBool("IsTakingDamage",true);
                    navMeshAgent.speed = m_walkSpeed;
                    navMeshAgent.isStopped = true;
                    navMeshAgent.destination = transform.position;
                }
            }
            else
            {
                if(Vector3.Distance(transform.position, m_homePosition) > m_maxDistanceToHome)
                {
                    m_currentActState = ActState.RunHome;
                    navMeshAgent.speed = m_runSpeed;
                    navMeshAgent.destination = m_homePosition;

                    if(navMeshAgent.isStopped)
                    {
                        navMeshAgent.isStopped = false;
                    }
                }
                else if(Time.time > m_nextTimeRandomStomp)
                {
                    setStomping();
                }
                else
                {
                    float distanceToPLayer = Vector3.Distance(transform.position, m_currentPlayer.transform.position);

                    if(Vector3.Distance(transform.position, m_currentPlayer.transform.position) < m_startStompRange)
                    {
                        setStomping();
                    }
                    else if(Vector3.Distance(transform.position, m_currentPlayer.transform.position) < m_startAttackRange)
                    {
                        playRandomTrollSound();
                        m_currentActState = ActState.Attack;
                        m_hurtEnemyThisAttack = false;
                        navMeshAgent.isStopped = true;
                        m_animator.SetBool("IsAttacking1",true);
                    }
                    else
                    {
                        m_currentActState = ActState.Walk;

                        navMeshAgent.destination = m_currentPlayer.transform.position;

                        if(navMeshAgent.isStopped)
                        {
                            navMeshAgent.isStopped = false;
                        }
                    }
                }
            }
        }
    }

    private void setStomping()
    {
        playRandomTrollSound();

        m_currentActState = ActState.Stomp;
        m_startStompTime = Time.time;
        navMeshAgent.speed = m_runSpeed;

        if(navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = false;
        }

        Vector3 playerDirection = m_currentPlayer.transform.position - transform.position;
        playerDirection = new Vector3(playerDirection.x,0,playerDirection.z).normalized;

        navMeshAgent.destination = transform.position + playerDirection * m_stompRunDistance;

        m_nextTimeRandomStomp = Time.time + m_randomStompMinTime + m_randomStompMaxTime * BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time);
    }

    private void onAggroTriggerEnter(object sender, Collider other)
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if(target != null)
        {
            m_playerInRange = target;
            m_currentPlayer = target;
            BasicTools.Audio.SoundManager.singleton.playSoundAt(30, transform.position);
            m_lastTimeRandomSound = Time.time;
        }
    }

        private void onAggroTriggerExited(object sender, Collider other)
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if(target != null)
        {
            m_playerInRange = null;
        }
    }

    private void playRandomTrollSound()
    {
        if(Time.time > m_lastTimeRandomSound + m_randomSoundCooldown)
        {
            m_lastTimeRandomSound = Time.time;
            int soundIndex =  BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.31f, 0.31f, 26, 29);
            BasicTools.Audio.SoundManager.singleton.playSoundAt(soundIndex, transform.position, BasicTools.Audio.Sound.SoundPlaystyle.Once);
        }
    }

    private float getRandomSeed()
    {
        return Time.time + transform.position.x;
    }

    private void onAnimationEventFired(object sender, string message)
    {
        if (message.Equals("Attack1Ended"))
        {
            m_currentActState = ActState.Idle;
            m_animator.SetBool("IsAttacking1",false);
        }
        else if (message.Equals("FootHitGround"))
        {
            m_hurtPlayerLastFootStep = false;
            int soundIndex =  BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.31f, 0.31f, 31, 34);
            BasicTools.Audio.SoundManager.singleton.playSoundAt(soundIndex, transform.position, BasicTools.Audio.Sound.SoundPlaystyle.Once);
        }
        else if(message.Equals("Attack1SwingStart"))
        {
            BasicTools.Audio.SoundManager.singleton.playSoundAt(35, transform.position, BasicTools.Audio.Sound.SoundPlaystyle.Once);
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
            if(m_hurtEnemyThisAttack == false && m_currentActState == ActState.Attack)
            {
                Damageable player = target as Damageable;
                player.onDamage(m_weaponListener.GetCollider().ClosestPoint(other.transform.position),new DamageData{m_amount= m_damage,m_damageType= DamageTypes.Physical});
                m_hurtEnemyThisAttack = true;
            }
        }
    }

    private void OnTriggerStayFoot(Collider other)
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if(target != null)
        {
            if(m_hurtPlayerLastFootStep == false && (m_currentActState == ActState.Walk || m_currentActState == ActState.Run || m_currentActState == ActState.Stomp))
            {
                Damageable player = target as Damageable;
                player.onDamage(m_weaponListener.GetCollider().ClosestPoint(other.transform.position),new DamageData{m_amount= m_damage,m_damageType= DamageTypes.Physical});
                m_hurtPlayerLastFootStep = true;
            }
        }
    }

    protected void onDeath(object sender, Damageable sender2)
    {
        PlayerUIInterface.Singleton.showGameWonScreen();
        base.destroyed -= onDeath;
    }
}
