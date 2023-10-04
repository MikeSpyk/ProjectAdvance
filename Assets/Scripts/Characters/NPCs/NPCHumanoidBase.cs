using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIMindSystem;
using BasicTools.UI;
using BasicTools.UI.Inventory;

public class NPCHumanoidBase : NPC_Base, SkillTeacher, Reproducer
{
    [SerializeField] public float m_maxHealth = 100f;
    [SerializeField] public float m_food = 100f;
    [SerializeField] public float m_water = 100f;
    [SerializeField] public float m_baseHunger = 0.01f;
    [SerializeField] public float m_baseThirst = 0.01f;
    [SerializeField] public float m_maxWater = 100f;
    [SerializeField] public float m_maxFood = 100f;
    [SerializeField] private SocietyManager m_societyManager;
    [SerializeField] public Animator m_animator;
    [SerializeField] public BasicTools.Animations.AnimationEventReceiver m_animationEventReceiver;
    [SerializeField] private BasicTools.Triggers.GenericTriggerEvents m_perceptionTrigger;
    [SerializeField] public BasicTools.Triggers.GenericTriggerEvents m_punchTrigger;
    [SerializeField] public Skills m_skills;
    [SerializeField] public GameObject m_weaponMountPoint;
    [SerializeField] public GameObject m_childMalePrefab;
    [SerializeField] public GameObject m_childFemalePrefab;
    [SerializeField] protected float m_startPunchRange = 1f;
    [SerializeField] private float m_punchDamage = 10f;
    [SerializeField] private float m_teachDistance = 5f;
    [SerializeField] private float m_lernSpeedFromTeacher = 1f;
    [SerializeField] private float m_height = 1f;
    [SerializeField] public Gender m_gender;
    [SerializeField] private float m_pregnancyDuration = 5f;
    [SerializeField] private int m_ageRate = 10000;
    [SerializeField] public int m_maxAge = 70;
    [SerializeField] private int[] m_startItemsIndex;
    [SerializeField] private float m_painSoundMinTimeDistance = 2f;
    [Header("AI Settings")]
    [SerializeField] private float m_eyesHeight = 1f;
    [SerializeField] private bool m_DEBUG_AIMindSystemMessage = false;
    [SerializeField] private string m_DEBUG_CurrentGoalAction = "";
    [SerializeField,Range(0f,1f)] private float m_aggressiveness = 0.1f;

    private Vector3 m_homePosition;
    private ActionableGoal m_currentGoal;
    protected List<System.Tuple<Action, object[]>> m_currentActionQueueResults = new List<System.Tuple<Action, object[]>>();
    protected int m_currentActionQueueIndex = 0;
    protected List<Lifeform> m_lifeformsPerceived = new List<Lifeform>();
    private List<Damageable> m_targetsInPunchZone = new List<Damageable>();
    private bool m_isPlayerCursorOver = false;
    private float m_getTaughtUrgency = 0f;
    private SkillTeacher m_externalTeacher = null;
    private bool m_isGettingTaught = false;
    private string m_gettingTaughtSkill = null;
    private bool m_isPregnant = false;
    private float m_reproduceUrgency = 0f;
    private float m_pregnancyStartTime = 0f;
    private Reproducer m_lastReproducer = null;
    private System.TimeSpan m_age = new System.TimeSpan();
    public int m_ageYears = 0;
    private GameObject m_activeWeapon = null;
    protected BaseWeapon m_activeWeaponScript = null;
    private string m_lastAnimatorHoldingParameterName = null;
    private float m_lastTimeDamagedSound = 0f;
    private int m_lastFramePlayedImpactSound = 0;

    public SocietyManager societyManager
    {
        get{return m_societyManager;}
        set{setSociety(value);}
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position + Vector3.up * m_eyesHeight, transform.position + Vector3.up * m_eyesHeight + transform.forward);
    }

    protected virtual void Awake()
    {
        damaged += onDamage;

        CursorRayCastTarget cursorTarget = GetComponent<CursorRayCastTarget>();
        cursorTarget.cursorEntered += onPlayerCursorEntered;
        cursorTarget.cursorExited += onPlayerCursorExited;

        m_health = m_maxHealth;

        base.Awake();
        m_perceptionTrigger.triggerEntered += onPerceptionTriggerEntered;
        m_perceptionTrigger.triggerExit += onPerceptionTriggerExited;

        m_punchTrigger.triggerEntered += onPunchTriggerEntered;
        m_punchTrigger.triggerExit += onPunchTriggerExited;

        m_skills.skillLeveledUp += onSkillLevelUp;

        createAIMindGoalSystem();

        if(m_societyManager != null)
        {
            setSociety(m_societyManager);
        }
    }

    private void onDamage(object sender, Damageable sender2)
    {
        if(Time.time > m_lastTimeDamagedSound + m_painSoundMinTimeDistance)
        {
            int soundIndex;

            if(m_gender == Gender.Male)
            {
                soundIndex = 12;
            }
            else
            {
                soundIndex = 11;
            }

            BasicTools.Audio.SoundManager.singleton.playSoundAt(soundIndex, transform.position);

            m_lastTimeDamagedSound = Time.time;
        }
    }

    protected void OnDestroy()
    {
        m_punchTrigger.triggerEntered -= onPunchTriggerEntered;
        m_punchTrigger.triggerExit -= onPunchTriggerExited;
        m_skills.skillLeveledUp -= onSkillLevelUp;
        m_societyManager.unregisterMember(this);
        damaged -= onDamage;
        m_animationEventReceiver.AnimationEventFired -= onAnimationEventReceived;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public Skills getSkills()
    {
        return m_skills;
    }

    public List<System.Tuple<string,string>> getRespawnInfoData()
    {
        List<System.Tuple<string,string>> result = new List<System.Tuple<string, string>>();

        result.Add(new System.Tuple<string, string>("Gender: ", m_gender.ToString()));
        result.Add(new System.Tuple<string, string>("Age: ", m_ageYears.ToString()));

        var skillsData = m_skills.getTextNameLevel();

        for(int i = 0; i< skillsData.Length; i++)
        {
            result.Add(skillsData[i]);
        }

        return result;
    }

    void onSkillLevelUp(object sender, SkillLevelUpEventArgs args)
    {
        WorldTextManager.Singleton.showText(string.Format("{0} {1}", args.name, args.level), transform.position + Vector3.up * m_height, transform, Color.black);
    }

    public void setSociety(SocietyManager newSociety)
    {
        m_societyManager = newSociety;
        m_societyManager.registerNewMember(this);
        destroyed += (object sender, Damageable sender2)=>{m_societyManager.unregisterMember(this);};
    }

    private void onPlayerCursorEntered(object sender, object args)
    {
        m_isPlayerCursorOver = true;
    }

    private void onPlayerCursorExited(object sender, object args)
    {
        m_isPlayerCursorOver = false;
    }

    private void onPerceptionTriggerEntered(object sender, Collider other)
    {
        Lifeform lifeform = other.GetComponent<Lifeform>();

        if(lifeform != null)
        {
            m_lifeformsPerceived.Add(lifeform);
            lifeform.destroyed += onPerceivedEnemyDied;
        }
    }

    private void onPerceptionTriggerExited(object sender, Collider other)
    {
        Lifeform lifeform = other.GetComponent<Lifeform>();

        if(lifeform != null)
        {
            if(m_lifeformsPerceived.Contains(lifeform))
            {
                m_lifeformsPerceived.Remove(lifeform);
                lifeform.destroyed -= onPerceivedEnemyDied;
            }
            else
            {
                Debug.LogWarning("Lifeform exited perception range but never entered");
            }
        }
    }

    private void onPerceivedEnemyDied(object sender, Damageable sender2)
    {
        sender2.destroyed -= onPerceivedEnemyDied;
        m_lifeformsPerceived.Remove((Lifeform)sender2);

        if(m_targetsInPunchZone.Contains(sender2))
        {
            m_targetsInPunchZone.Remove(sender2);
        }
    }

    private void onPunchTriggerEntered(object sender, Collider other)
    {
        Damageable damageable = other.GetComponent<Damageable>();

        if(damageable != null)
        {
            m_targetsInPunchZone.Add(damageable);
        }
    }

    private void onPunchTriggerExited(object sender, Collider other)
    {
        Damageable damageable = other.GetComponent<Damageable>();

        if(damageable != null && m_targetsInPunchZone.Contains(damageable))
        {
            m_targetsInPunchZone.Remove(damageable);
        }
    }

    protected virtual void Start()
    {
        m_homePosition = transform.position;
        m_animationEventReceiver.AnimationEventFired += onAnimationEventReceived;
        spawnStartItems();

        UIItemData weapon = transform.GetComponentInChildren<UIItemData>();

        if(weapon != null)
        {
            m_activeWeapon = equipNewItem(weapon);
            m_activeWeaponScript = m_activeWeapon.GetComponent<BaseWeapon>();
            m_activeWeaponScript.setCarrierSkills(m_skills);
            m_activeWeaponScript.setCarrierDamageable(this);
            setHoldingAnimation(m_activeWeaponScript.holdingPose);
        }
    }

    protected virtual void FixedUpdate()
    {
        ageUpdate();
        aiMindGoalsUpdate();
        teachingUpdate();
        pregnancyUpdate();
    }

    private void spawnStartItems()
    {
        if(m_startItemsIndex != null && m_startItemsIndex.Length > 0)
        {
            for(int i = 0; i < m_startItemsIndex.Length; i++)
            {
                GameObject itemOjb = new GameObject("Item " + i);
                itemOjb.transform.SetParent(this.transform);
                UIItemData itemData = itemOjb.AddComponent<UIItemData>();
                itemData.updateItemData(m_startItemsIndex[i], ItemsInterface.singleton.rollRandomAttributes(m_startItemsIndex[i],Time.time + i * 0.33f, Time.deltaTime).ToArray());
            }
        }
    }

    private void ageUpdate()
    {
        m_age = m_age.Add(new System.TimeSpan(0,0,0, m_ageRate));
        int lastYears = m_ageYears;
        m_ageYears = (int)(m_age.TotalDays / 365);

        if(m_ageYears > lastYears)
        {
            WorldTextManager.Singleton.showText(m_ageYears + " years old", transform.position + Vector3.up * m_height, transform, Color.black);

            if(m_ageYears > m_maxAge)
            {
                onDamage(transform.position, new DamageData(){m_amount= float.MaxValue, m_damageType = DamageTypes.Pure});
            }
        }
    }

    private GameObject equipNewItem(UIItemData itemData)
    {
        GameObject newItem = ItemsInterface.singleton.spawnEquipableItem(itemData.itemIndex);

        if(newItem != null)
        {
            newItem.transform.SetParent(m_weaponMountPoint.transform);
            newItem.transform.localPosition = Vector3.zero;
            newItem.transform.localRotation = Quaternion.identity;

            BaseItem item = newItem.GetComponent<BaseItem>();

            if(item == null)
            {
                Debug.LogError("Item without BaseItem-script");
            }
            else
            {
                item.applyOffset();
                item.applyItemData(itemData);
            }
        }

        return newItem;
    }

    private void setHoldingAnimation(HoldingPose holdingPose)
    {
        string animationParameter = getAnimatorParameterName(holdingPose);

        if(m_lastAnimatorHoldingParameterName != null)
        {
            m_animator.SetBool(m_lastAnimatorHoldingParameterName, false);
        }

        if(animationParameter != null)
        {
            m_animator.SetBool(animationParameter, true);
        }

        m_lastAnimatorHoldingParameterName = animationParameter;
    }

    private string getAnimatorParameterName(HoldingPose holdingPose)
    {
        switch (holdingPose)
        {
            case HoldingPose.Spear:
                return "IsSpear";

            default:
                return null;
        }
    }

    public void setPregnant(Reproducer parent)
    {
        m_isPregnant = true;
        m_pregnancyStartTime = Time.time;
        resetReproduceUrgency();
        WorldTextManager.Singleton.showText("Is pregnant", transform.position + Vector3.up * m_height, transform, Color.black);
    }

    private void pregnancyUpdate()
    {
        if(m_isPregnant)
        {
            if(Time.time > m_pregnancyStartTime + m_pregnancyDuration)
            {
                giveBirth();
            }
        }
    }

    private void giveBirth()
    {
        NPCHumanoidBase child;

        if(BasicTools.Random.RandomValuesSeed.getRandomBool(Time.time))
        {
            child = Instantiate(m_childMalePrefab, transform.position, Quaternion.identity).GetComponent<NPCHumanoidBase>();
        }
        else
        {
            child = Instantiate(m_childFemalePrefab, transform.position, Quaternion.identity).GetComponent<NPCHumanoidBase>();
        }

        child.setSociety(m_societyManager);

        m_isPregnant = false;
    }

    private void teachingUpdate()
    {
        if(m_isGettingTaught)
        {
            Skills teacherSkills = m_externalTeacher.getSkills();

            if(teacherSkills.getSkillLevel(m_gettingTaughtSkill) > m_skills.getSkillLevel(m_gettingTaughtSkill))
            {
                m_skills.addExperienceSkill(m_gettingTaughtSkill, m_lernSpeedFromTeacher);
            }
            else
            {
                WorldTextManager.Singleton.showText("Nothing left to learn", transform.position + Vector3.up * m_height, transform, Color.black);
                resetGetTaughtUrgency();
            }
        }
    }

    private float calculateReproductionWill()
    {
        if(m_isPregnant)
        {
            return 0f;
        }
        else
        {
            return Mathf.Min(m_food/m_maxFood, m_water/m_maxWater, m_health/m_maxHealth);
        }
    }

    public float getAttractiveness()
    {
        return (m_health/ m_maxHealth + m_food/m_maxFood + m_water/m_maxWater) / 3f;
    }

    public Gender getGender()
    {
        return m_gender;
    }

    protected void resetGoalActionQueueProgess()
    {
        m_currentActionQueueResults.Clear();
        m_currentActionQueueIndex = 0;
    }

    private object[] getCurrentActionQueueResults(Action action)
    {
        for(int i = 0; i < m_currentActionQueueResults.Count; i++)
        {
            if(m_currentActionQueueResults[i].Item1 == action)
            {
                return m_currentActionQueueResults[i].Item2;
            }
        }

        return null;
    }

    private void onAnimationEventReceived(object sender, string animationEventName)
    {
        if(animationEventName == "AttackHitPosition")
        {
            m_animator.SetBool("IsAttacking",false);

            if(m_activeWeaponScript != null)
            {
                m_activeWeaponScript.onEndAttack();
            }
            else // fist attack
            {
                for(int i = 0; i < m_targetsInPunchZone.Count; i++)
                {
                    if(m_targetsInPunchZone[i] == null)
                    {
                        m_targetsInPunchZone.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        if(!m_targetsInPunchZone[i].hasSameGroup(this))
                        {
                            m_targetsInPunchZone[i].onDamage(m_targetsInPunchZone[i].transform.position, new DamageData(){m_amount= m_punchDamage,m_damageType= DamageTypes.Physical});

                            if(Time.frameCount > m_lastFramePlayedImpactSound)
                            {
                                m_lastFramePlayedImpactSound = Time.frameCount;
                                BasicTools.Audio.SoundManager.singleton.playSoundAt(16, transform.position);
                            }
                        }
                    }
                }
            }
        }
        else if(animationEventName == "FootHitGround")
        {
            int soundIndex =  BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.31f, 0.31f, 0, 9);

            BasicTools.Audio.SoundManager.singleton.playSoundAt(soundIndex, transform.position, BasicTools.Audio.Sound.SoundPlaystyle.Once);
        }
    }

    private GameObject getClosestFoodSource()
    {
        GameObject[] foodSources = m_societyManager.getFoodSources();

        GameObject closestFoodSource = null;
        float closestFoodSourceDistance = float.MaxValue;
        float tempDistance;

        for(int i = 0; i < foodSources.Length; i++)
        {
            tempDistance = Vector3.Distance(transform.position, foodSources[i].transform.position);

            if(tempDistance < closestFoodSourceDistance)
            {
                closestFoodSourceDistance = tempDistance;
                closestFoodSource = foodSources[i];
            }
        }

        return closestFoodSource;
    }

    private void onPlayerTeachSkillClicked(ContextMenuItem skillUiItem)
    {
        PlayerController player = CustomGameManager.Singleton.getActivePlayerController();

        if(player != null)
        {
            m_externalTeacher = player;
            m_gettingTaughtSkill = skillUiItem.m_text;
            m_getTaughtUrgency = calculateRelaxUrgency(null) + 0.001f;

            if(m_currentGoal.getName().Equals("GetTaught"))
            {
                resetGoalActionQueueProgess();
            }
        }
    }

    private void resetGetTaughtUrgency()
    {
        m_getTaughtUrgency = 0f;
    }

    private void onPlayerReproduceClicked(ContextMenuItem skillUiItem)
    {
        PlayerController player = CustomGameManager.Singleton.getActivePlayerController();

        if(player != null)
        {
            //Debug.Log("player Attractiveness: " +player.getAttractiveness() + ", npc Reproduction Will: " + calculateReproductionWill());

            //if(player.getAttractiveness() > calculateReproductionWill() && !m_isPregnant)
            if(!m_isPregnant)
            {
                m_lastReproducer = player;
                m_reproduceUrgency = calculateRelaxUrgency(null) + 0.01f;
            }
            else
            {
                WorldTextManager.Singleton.showText("Not Interested", transform.position + Vector3.up * m_height, transform, Color.black);
            }
        }
    }

    private void resetReproduceUrgency()
    {
        m_reproduceUrgency = 0f;
    }

    protected virtual void Update()
    {
        if(m_isPlayerCursorOver)
        {
            if(Input.GetKeyDown(KeyCode.Mouse1))
            {
                PlayerController player = CustomGameManager.Singleton.getActivePlayerController();

                if(player != null)
                {
                    List<ContextMenuItemBase> menuBase = new List<ContextMenuItemBase>();

                    string[] skillsNames = m_skills.skillsName;

                    ContextMenuItem[] uiSkills = new ContextMenuItem[skillsNames.Length];

                    for(int i = 0; i < uiSkills.Length; i++)
                    {
                        uiSkills[i] = new ContextMenuItem(skillsNames[i], new System.Action<ContextMenuItem>(onPlayerTeachSkillClicked));
                    }

                    menuBase.Add(new ContextMenuItemParent("Teach Skill",uiSkills));

                    if(player.getGender() != getGender())
                    {
                        menuBase.Add(new ContextMenuItem("Reproduce",onPlayerReproduceClicked));
                    }

                    PlayerUIInterface.Singleton.showContextMenu(menuBase.ToArray());
                }
            }
        }

        Debug.DrawLine(transform.position, navMeshAgent.destination);

        if(navMeshAgent.velocity.magnitude > 0.01f)
        {
            m_animator.SetBool("IsWalking",true);
        }
        else
        {
            m_animator.SetBool("IsWalking",false);
        }

        m_food -= m_baseHunger * Time.deltaTime;
        m_water -= m_baseThirst * Time.deltaTime;
    }

    #region AIMindGoalSystem

    private void aiMindGoalsUpdate()
    {
        m_GoalSystem.calculateUrgency();
        ActionableGoal nextGoal = m_GoalSystem.GetMostUrgentActionableGoal();

        if(m_DEBUG_AIMindSystemMessage)
        {
            m_DEBUG_AIMindSystemMessage = false;

            List<Goal_Base> allLowGoals = new List<Goal_Base>();
            m_GoalSystem.appendAllLowestLevelGoals(allLowGoals);

            string message = "";

            for(int i = 0; i < allLowGoals.Count; i++)
            {
                message += allLowGoals[i].getName() + ": " + allLowGoals[i].getUrgency() + "\n";
            }

            Debug.Log("All Executable Goals:\n" +message);
        }

        if(nextGoal != m_currentGoal)
        {
            onAIMindGoalChanged(m_currentGoal, nextGoal);
        }

        if(m_currentActionQueueIndex >= m_currentGoal.actionQueue.Length)
        {
            Debug.LogWarning("AIMindGoalQueueIndex end of queue reached. No action will be executed!"); // should not happen. try expanding queue or make sure goals shifts before last action is done
            return;
        }

        m_DEBUG_CurrentGoalAction = m_currentGoal.actionQueue[m_currentActionQueueIndex].ToString();

        if(executeMindAIGoalAction(m_currentGoal.actionQueue[m_currentActionQueueIndex]))
        {
            m_currentActionQueueIndex++;
        }
    }

    private void onAIMindGoalChanged(ActionableGoal lastGoal, ActionableGoal nextGoal)
    {
        m_currentGoal = nextGoal;
        resetGoalActionQueueProgess();

        if(lastGoal != null)
        {
            switch(lastGoal.getName())
            {
                case "GetTaught":
                {
                    m_isGettingTaught = false;
                    resetGetTaughtUrgency();
                    break;
                }
                case "Reproduce":
                {
                    resetReproduceUrgency();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    protected virtual void createAIMindGoalSystem()
    {
        m_GoalSystem = new AIMindSystem.Goal(gameObject.name, new AIMindSystem.Goal_Base[]
        {
            new Goal("Survive", new Goal_Base[]
            {
                new ActionableGoal("GetFood", calculateFoodUrgency, new Action[]{Action.FindFoodSource, Action.GoToPosition, Action.PickUpFood}),
                new ActionableGoal("GetWater", calculateWaterUrgency, new Action[]{Action.FindWaterSource, Action.GoToPosition, Action.PickUpWater})
            }),
            new ConditionalParentGoal("DealWithEnemyEncounter", calculateDealWithEnemiesUrgency, new Goal_Base[]
            {
                new ActionableGoal("RunToSafety", calculateRunToSafetyUrgency, new Action[]{Action.FindSafeLocation, Action.GoToPosition}),
                new ActionableGoal("FightEnemies", calculateFightEnemiesUrgency, new Action[]{Action.Attack})
            }),
            new ActionableGoal("Relax", calculateRelaxUrgency, new Action[]{Action.FindRelaxLocation, Action.GoToPosition, Action.FindRandomPositionVillagerRelax, Action.GoToPosition, Action.StoreTime_Time, Action.Wait10s, Action.ActionQueueBack4Steps}),
            new ActionableGoal("GetTaught", calculateGetTaughtUrgency, new Action[]{Action.FindTeacher, Action.GoToPosition, Action.ObserveTeacherMinimumDistance}),
            new ActionableGoal("Reproduce", calculateReproduceUrgency, new Action[]{Action.FindReproducer, Action.GoToPosition, Action.Reproduce}),
        });
    }

    protected virtual bool executeMindAIGoalAction(Action currentAction)
    {
        // rules to this method:
        // 1. always call m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{..... })) when returning true

        switch(currentAction)
        {
            case Action.Wait10s:
            {
                if(m_currentActionQueueIndex < 1)
                {
                    Debug.LogWarning("index out of expected range");
                    return false;
                }

                float? startTime = m_currentActionQueueResults[m_currentActionQueueIndex-1].Item2[0] as float?;

                if(startTime == null)
                {
                    Debug.LogWarning("previous Data of wrong type");
                    return false;
                }

                if(Time.time > (float)startTime + 10)
                {
                    m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, null));
                    return true;
                }

                break;
            }
            case Action.StoreTime_Time:
            {
                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{ Time.time}));
                return true;
            }
            case Action.FindRandomPositionVillagerRelax:
            {
                if(m_currentActionQueueIndex < 2)
                {
                    Debug.LogWarning("index out of expected range");
                    return false;
                }

                Vector3? originPosition = m_currentActionQueueResults[m_currentActionQueueIndex-2].Item2[0] as Vector3?;

                if(originPosition == null)
                {
                    Debug.LogWarning("previous Data of wrong type");
                    return false;
                }

                float randomRange = 5f + BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.33f, gameObject.GetInstanceID()/1000f * 0.33f) * 5f;

                Vector3 position = 
                    (Vector3)originPosition
                    + new Vector3(BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.33f),0f,BasicTools.Random.RandomValuesSeed.getRandomValueSeed(Time.time * 0.67f)).normalized * randomRange
                    - new Vector3(5f,0,5f);

                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{ position}));

                return true;
            }
            case Action.ActionQueueBack4Steps:
            {
                m_currentActionQueueResults.RemoveRange(m_currentActionQueueResults.Count -1 -4, 4);
                m_currentActionQueueIndex -= 4;

                break;
            }
            case Action.Reproduce:
            {
                if(m_lastReproducer == null)
                {
                    resetReproduceUrgency();
                }
                else
                {
                    if(Vector3.Distance(m_lastReproducer.getGameObject().transform.position, transform.position) < 2f)
                    {
                        if(m_gender == Gender.Female)
                        {
                            setPregnant(m_lastReproducer);
                        }
                        else
                        {
                            m_lastReproducer.setPregnant(this);
                        }
                    }

                    resetReproduceUrgency();
                }

                break;
            }
            case Action.FindReproducer:
            {
                Vector3 position;
                
                if(m_lastReproducer == null)
                {
                    position = transform.position;
                    resetReproduceUrgency();
                }
                else
                {
                    position = m_lastReproducer.getGameObject().transform.position;
                }

                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction,new object[]{position, m_lastReproducer}));
                return true;
            }
            case Action.ObserveTeacherMinimumDistance:
            {
                if(m_currentActionQueueResults[0].Item2[1] == null) // teacher is dead
                {
                    resetGetTaughtUrgency();
                }
                else
                {
                    Vector3 observationTarget = ((SkillTeacher)m_currentActionQueueResults[0].Item2[1]).getGameObject().transform.position;

                    if(Vector3.Distance(transform.position, observationTarget) > m_teachDistance)
                    {
                        resetGetTaughtUrgency();
                    }
                    else
                    {
                        // TODO: rotate towards teacher
                        m_isGettingTaught = true;
                    }
                }

                break;
            }
            case Action.FindTeacher:
            {
                Vector3 teacherPos;
                
                if(m_externalTeacher == null)
                {
                    teacherPos = transform.position;
                    resetGetTaughtUrgency();
                }
                else
                {
                    teacherPos = m_externalTeacher.getGameObject().transform.position;
                }

                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction,new object[]{teacherPos, m_externalTeacher}));
                return true;
            }
            case Action.FindFoodSource:
            {
                GameObject food = getClosestFoodSource();

                if(food != null)
                {
                    m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction,new object[]{ food.transform.position, food}));
                    return true;
                }
                break;
            }
            case Action.GoToPosition:
            {
                if(m_currentActionQueueIndex < 1)
                {
                    Debug.LogWarning("index out of expected range");
                    return false;
                }

                Vector3? destination = m_currentActionQueueResults[m_currentActionQueueIndex-1].Item2[0] as Vector3?;

                if(destination == null)
                {
                    Debug.LogWarning("previous Data of wrong type");
                    return false;
                }

                if(navMeshAgent.destination != (Vector3)m_currentActionQueueResults[m_currentActionQueueIndex-1].Item2[0])
                {
                    navMeshAgent.destination = (Vector3)m_currentActionQueueResults[m_currentActionQueueIndex-1].Item2[0];
                }

                if(Vector3.Distance(transform.position, (Vector3)destination) < 0.5f)
                {
                    m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction,null));
                    return true;
                }

                break;
            }
            case Action.PickUpFood:
            {
                GameObject food = getCurrentActionQueueResults(Action.FindFoodSource)[1] as GameObject;

                if(food != null && Vector3.Distance(transform.position, food.transform.position) < 1f)
                {
                    Destroy(food);
                    m_food = m_maxFood;

                    m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction,null));
                    return true;
                }
                else // went to position but no more food here -> restart
                {
                    resetGoalActionQueueProgess();
                }
                break;
            }
            case Action.FindWaterSource:
            {
                PickUpWaterSource waterSource = m_societyManager.getWaterSource();
                Vector3 waterPickupTarget = waterSource.GetComponent<Collider>().ClosestPoint(transform.position);

                RaycastHit hit = new RaycastHit();
                Physics.Raycast(waterPickupTarget + Vector3.up * 100, Vector3.down, out hit);

                Vector3 groundAboveWater = hit.point;
                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{ groundAboveWater, waterSource}));
                return true;
            }
            case Action.PickUpWater:
            {
                Vector3 eyesPos = transform.position + Vector3.up * m_eyesHeight;
                Vector3 waterPickupTarget = ((PickUpWaterSource)m_currentActionQueueResults[0].Item2[1]).surfaceCollider.ClosestPoint(transform.position);
                RaycastHit hit = new RaycastHit();

                if(Physics.Raycast(eyesPos, waterPickupTarget - eyesPos, out hit, 5f))
                {
                    if(hit.transform.gameObject == ((PickUpWaterSource)m_currentActionQueueResults[0].Item2[1]).gameObject)
                    {
                        m_water = m_maxWater;
                        navMeshAgent.destination = transform.position;
                        return true;
                    }
                }

                // closests point on water-surface-collide might not be the closests visible point -> go to center of water source
                Vector3 towardsWaterOrigin = transform.position + (((PickUpWaterSource)m_currentActionQueueResults[0].Item2[1]).transform.position - transform.position).normalized;

                if(navMeshAgent.destination != towardsWaterOrigin)
                {
                    navMeshAgent.destination = towardsWaterOrigin;
                }
                break;
            }
            case Action.FindSafeLocation:
            case Action.FindRelaxLocation:
            {
                m_currentActionQueueResults.Add(new System.Tuple<Action, object[]>(currentAction, new object[]{ m_homePosition}));
                return true;
            }
            case Action.Attack:
            {
                Lifeform closestsEnemy = null;
                float closestsEnemyDistance = float.MaxValue;
                float tempDistance;

                for(int i = 0; i < m_lifeformsPerceived.Count; i++)
                {
                    if(m_lifeformsPerceived[i].GetType() != typeof(PlayerController) && m_lifeformsPerceived[i].GetType() != typeof(NPCHuman))
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
                        m_animator.SetBool("IsAttacking",true);

                        if(m_activeWeaponScript != null)
                        {
                            m_activeWeaponScript.onStartAttack();
                        }
                    }
                }

                break;
            }
            case Action.StandStill:
            {
                if(navMeshAgent.destination != transform.position)
                {
                    navMeshAgent.destination = transform.position;
                }

                break;
            }
            default:
            {
                Debug.LogWarning("action not implemented: " +currentAction.ToString());
                break;
            }
        }

        return false;
    }

    #region AIMindGoalSystemUrgencyCalculations

    protected virtual float calculateFoodUrgency(Goal_Base sender)
    {
        return 1f- m_food/ m_maxFood;
    }

    protected virtual float calculateWaterUrgency(Goal_Base sender)
    {
        return 1f- m_water/ m_maxWater;
    }

    protected virtual ConditionalCalculationResult calculateDealWithEnemiesUrgency(Goal_Base sender)
    {
        float urgency = 0f;
        Dictionary<string,object> results = new Dictionary<string, object>();

        bool canRunFromEnemy = true;

        float expectedNewHealthWeight = 0f;
        float worstCaseNewHealthWeight = 0f;

        List<Lifeform> enemyLifeforms = new List<Lifeform>();

        for(int i = 0; i < m_lifeformsPerceived.Count; i++)
        {
            if(m_lifeformsPerceived[i].GetType() != typeof(PlayerController) && m_lifeformsPerceived[i].GetType() != typeof(NPCHuman) )
            {
                enemyLifeforms.Add(m_lifeformsPerceived[i]);
            }
        }

        if(enemyLifeforms.Count > 0)
        {
            float expectedDamage = 0f;
            float worstCaseDamge = 0f;

            for(int i = 0; i < enemyLifeforms.Count; i++)
            {
                if(canRunFromEnemy)
                {
                    if(this.averageSpeed < enemyLifeforms[i].averageSpeed || Vector3.Distance(transform.position,m_homePosition) < 1f)
                    {
                        canRunFromEnemy = false;
                    }
                }

                worstCaseDamge += enemyLifeforms[i].averageDamage;

                if(Vector3.Distance(transform.position, enemyLifeforms[i].transform.position) < enemyLifeforms[i].attackRange)
                {
                    expectedDamage += enemyLifeforms[i].averageDamage;
                }
            }

            float expectedNewHealth = Mathf.Max(health - expectedDamage, 0f);
            float worstCaseNewHealth = Mathf.Max(health - worstCaseDamge, 0f);

            expectedNewHealthWeight = 1f - expectedNewHealth / m_maxHealth; // 0.00 - 1.00
            worstCaseNewHealthWeight = 1f - worstCaseNewHealth / m_maxHealth; // 0.00 - 1.00

            float expectedHealthReductionWeight; // 0.00 - 1.00
            float worstCaseHealthReductionWeight; // 0.00 - 1.00

            if(health > 0)
            {
                expectedHealthReductionWeight = Mathf.Min(expectedDamage/health, 1f);
                worstCaseHealthReductionWeight = Mathf.Min(worstCaseDamge/health, 1f);
            }
            else
            {
                expectedHealthReductionWeight = 1f;
                worstCaseHealthReductionWeight = 1f;
            }

            if(canRunFromEnemy)
            {
                urgency = Mathf.Max(expectedHealthReductionWeight, expectedNewHealthWeight);
            }
            else
            {
                urgency = Mathf.Max(worstCaseHealthReductionWeight, worstCaseNewHealthWeight);
            }

            urgency = Mathf.Max(urgency, m_aggressiveness);
        }

        results.Add("canRunFromEnemy",canRunFromEnemy);
        results.Add("worstCaseNewHealthWeight", worstCaseNewHealthWeight);

        return new ConditionalCalculationResult(urgency,results);
    }

    protected virtual float calculateRunToSafetyUrgency(Goal_Base sender)
    {
        ConditionalCalculationResult parentResult = (sender.getParent() as ConditionalParentGoal).lastCalculationResult;

        if((bool)parentResult.data["canRunFromEnemy"] == true)
        {
            if(parentResult.urgency > m_aggressiveness || (float)parentResult.data["worstCaseNewHealthWeight"] > 0.8f)
            {
                return parentResult.urgency;
            }
        }
        
        return 0f;
    }

    protected virtual float calculateFightEnemiesUrgency(Goal_Base sender)
    {
        ConditionalCalculationResult parentResult = (sender.getParent() as ConditionalParentGoal).lastCalculationResult;

        if((bool)parentResult.data["canRunFromEnemy"] == true)
        {
            if(parentResult.urgency > m_aggressiveness || (float)parentResult.data["worstCaseNewHealthWeight"] > 0.8f)
            {
                return 0f;
            }
        }
        
        return parentResult.urgency;
    }

    protected virtual float calculateRelaxUrgency(Goal_Base sender)
    {
        return 0.1f;
    }

    protected virtual float calculateGetTaughtUrgency(Goal_Base sender)
    {
        return m_getTaughtUrgency;
    }

    protected virtual float calculateReproduceUrgency(Goal_Base sender)
    {
        return m_reproduceUrgency;
    }

    #endregion
    #endregion

}
