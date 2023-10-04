using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI.Tooltip;
using BasicTools.UI.Inventory;

public class PlayerController : Lifeform, SkillTeacher, Reproducer
{
    [Header("Internal Stats")]
    [SerializeField] public float m_moveSpeed = 1;
    [SerializeField] public float m_stepHeight = 1f; // how much height-difference can the player move
    [SerializeField] public float m_animationMaleWalkSpeedFactor = 1f;
    [SerializeField] public float m_animationFemaleWalkSpeedFactor = 1f;
    [SerializeField] public float m_rotationSpeed = 1;
    [SerializeField] public float m_livingHunger = 0.01f;
    [SerializeField] public float m_livingThirst = 0.01f;
    [SerializeField] public float m_moveHunger = 0.01f;
    [SerializeField] public float m_moveThirst = 0.01f;
    [SerializeField] public Gender m_gender;
    [SerializeField] public float m_pregnancyDuration = 5f;
    [SerializeField] public int m_ageRate = 10000;
    [SerializeField] public int m_ageYears = 0;
    [SerializeField] public int m_maxAge = 70;
    [Header("RPG Stats")]
    [SerializeField] public float m_stamina = 100;
    [SerializeField] public float m_food = 100;
    [SerializeField] public float m_water = 100;
    [SerializeField] public float m_maxHealth = 100;
    [SerializeField] public float m_maxStamina = 100;
    [SerializeField] public float m_maxFood = 100;
    [SerializeField] public float m_maxWater = 100;
    [SerializeField] private float m_painSoundMinTimeDistance = 1f;
    [Header("Debug Settings")]
    [SerializeField] public bool m_noMetabolism = false;
    [SerializeField] public bool m_kill = false;
    [Header("Controls")]
    [SerializeField] public KeyCode m_attack1Key = KeyCode.Mouse0;
    [Header("References")]
    [SerializeField] public Skills m_skills;
    [SerializeField] public GameObject m_weaponMountPoint;
    [SerializeField] public Animator m_animator;
    [SerializeField] public BasicTools.Animations.AnimationEventReceiver m_animationEventReceiver;
    [SerializeField] public BasicTools.Triggers.GenericTriggerEvents m_punchTrigger;
    [SerializeField] public GameObject m_childMalePrefab;
    [SerializeField] public GameObject m_childFemalePrefab;

    private bool m_isMoving = false;
    private Rigidbody m_rigidbody = null;
    private GameObject m_activeWeapon = null;
    private BaseWeapon m_activeWeaponScript = null;
    private string m_lastAnimatorHoldingParameterName = null;
    private bool m_isAttacking = false;
    private List<Damageable> m_targetsInPunchZone = new List<Damageable>();
    private Vector3 m_lastPosition = Vector3.zero;
    private bool m_isPregnant = false;
    private float m_pregnancyStartTime = 0f;
    private SocietyManager m_latestReproducersSociety = null;
    private System.TimeSpan m_age;
    private CameraController m_cameraController = null;
    private Collider m_collider;
    private float m_lastTimeDamagedSound = 0f;
    private int m_lastFramePlayedImpactSound = 0;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        m_cameraController = Camera.main.GetComponent<CameraController>();
        m_cameraController.observationTarget = this.gameObject;
        base.destroyed += onDeath;
    }

    void Start()
    {
        m_animationEventReceiver.AnimationEventFired += onAnimationEventReceived;
        m_punchTrigger.triggerEntered += onPunchTriggerEntered;
        m_punchTrigger.triggerExit += onPunchTriggerExited;
        m_age = new System.TimeSpan(365 * m_ageYears,0,0,0);

        CustomGameManager.Singleton.setActivePlayerController(this);
        PlayerUIInterface.Singleton.getPlayerEquipmentContainer().UIItemChanged += onEquipmentChanged;
        PlayerUIInterface.Singleton.setActiveSkills(m_skills);

        m_skills.skillLeveledUp += onSkillLevelUp;
        m_skills.experienceAdded += onSkillExperienceAdded;

        damaged += onDamage;

        PlayerUIInterface.Singleton.setHealth(health, m_maxHealth);
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

    protected void onDeath(object sender, Damageable sender2)
    {
        base.destroyed -= onDeath;

        dropAllItems();

        m_animationEventReceiver.AnimationEventFired -= onAnimationEventReceived;
        m_punchTrigger.triggerEntered -= onPunchTriggerEntered;
        m_punchTrigger.triggerExit -= onPunchTriggerExited;
        PlayerUIInterface.Singleton.getPlayerEquipmentContainer().UIItemChanged -= onEquipmentChanged;
        m_skills.skillLeveledUp -= onSkillLevelUp;
        m_skills.experienceAdded -= onSkillExperienceAdded;

        CustomGameManager.Singleton.onPlayerDied();
    }

    void onSkillLevelUp(object sender, SkillLevelUpEventArgs args)
    {
        BasicTools.Audio.SoundManager.singleton.playGlobalSound(15);
        PlayerUIInterface.Singleton.printInformationText(string.Format("\"{0}\" level {1}", args.name, args.level));
        PlayerUIInterface.Singleton.getSkillsMenuBuilder().updateRowTooltip(args.name,new TextData[]{new TextData(){m_leftAlignedText=((Skills)sender).getTooltipText(args.name)}});        
    }

    void onSkillExperienceAdded(object sender, SkillExperienceAddedEventArgs args)
    {
        PlayerUIInterface.Singleton.getSkillsMenuBuilder().updateRowChildContent(args.name,1,args.level.ToString());
        PlayerUIInterface.Singleton.getSkillsMenuBuilder().updateRowChildContent(args.name,2,string.Format("{0};{1}", args.experience, args.maxExperience));    
    }

    void Update()
    {
        if(m_kill)
        {
            kill();
        }

        playerMovement();
        fixFallThruGround();
        weaponUpdate();
        
        if(!m_noMetabolism)
        {
            playerMetabolism();
        }

        if(m_isMoving)
        {
            float movementSpeed = Vector3.Distance(transform.position, m_lastPosition) / Time.deltaTime;

            if (m_gender == Gender.Male)
            {
                m_animator.SetFloat("WalkSpeed", movementSpeed * m_animationMaleWalkSpeedFactor);
            }
            else
            {
                m_animator.SetFloat("WalkSpeed", movementSpeed * m_animationFemaleWalkSpeedFactor);
            }
        }
        else
        {
            m_animator.SetFloat("WalkSpeed", 1f);
        }

        if(Input.mouseScrollDelta != Vector2.zero)
        {
            m_cameraController.changeCameraDistance(Input.mouseScrollDelta.y);
        }

        m_lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        pregnancyUpdate();
        ageUpdate();
    }

    private void ageUpdate()
    {
        m_age = m_age.Add(new System.TimeSpan(0,0,0, m_ageRate));
        int lastYears = m_ageYears;
        m_ageYears = (int)(m_age.TotalDays / 365);

        if(m_ageYears > lastYears)
        {
            WorldTextManager.Singleton.showText(m_ageYears + " years old", transform.position + Vector3.up, transform, Color.black);

            if(m_ageYears > m_maxAge)
            {
                onDamage(transform.position, new DamageData(){m_amount= float.MaxValue, m_damageType = DamageTypes.Pure});
            }
        }
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

        child.setSociety(m_latestReproducersSociety);

        m_isPregnant = false;
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

    private void onAnimationEventReceived(object sender, string animationEventName)
    {
        if(animationEventName == "AttackHitPosition")
        {
            setIsAttacking(false);

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
                        m_targetsInPunchZone[i].onDamage(m_targetsInPunchZone[i].transform.position, new DamageData(){m_amount= 20,m_damageType= DamageTypes.Physical});

                        if(Time.frameCount > m_lastFramePlayedImpactSound)
                        {
                            m_lastFramePlayedImpactSound = Time.frameCount;
                            BasicTools.Audio.SoundManager.singleton.playSoundAt(16, transform.position);
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

    private void unequipWeapon()
    {
        if(m_activeWeapon != null)
        {
            Destroy(m_activeWeapon);
            m_activeWeapon = null;
            m_activeWeaponScript = null;
        }
    } 

    private void weaponUpdate()
    {
        if(Input.GetKeyDown(m_attack1Key) && !PlayerUIInterface.Singleton.isMouseOnUi())
        {
            if(!m_isAttacking)
            {
                setIsAttacking(true);

                if(m_activeWeaponScript != null)
                {
                    m_activeWeaponScript.onStartAttack();
                }
            }
        }
    }

    private void playerMovement()
    {
        Vector2 moveDirection = Vector2.zero;

        if(Input.GetKey(KeyCode.W))
        {
            moveDirection += new Vector2(0,1);
        }
        if(Input.GetKey(KeyCode.S))
        {
            moveDirection += new Vector2(0,-1);
        }
        if(Input.GetKey(KeyCode.D))
        {
            moveDirection += new Vector2(1,0);
        }
        if(Input.GetKey(KeyCode.A))
        {
            moveDirection += new Vector2(-1,0);
        }

        if(moveDirection != Vector2.zero)
        {
            m_isMoving = true;
            m_animator.SetBool("IsWalking",true);

            Vector2 characterForward = new Vector2( transform.forward.x, transform.forward.z);
            float toRotateAngle = Vector2.Angle(moveDirection,characterForward);
            float toRotate = toRotateAngle / 180f; // same as toRotateAngle but within range 0.00....1.00
        
            Vector3 playerControlledMovement =  new Vector3(moveDirection.x,0,moveDirection.y).normalized * 
                                                m_moveSpeed * 
                                                (1f- toRotate) *
                                                Time.deltaTime;


            RaycastHit[] hits = m_rigidbody.SweepTestAll(playerControlledMovement, playerControlledMovement.magnitude);

            bool willHitObstacle = false;

            if(hits != null && hits.Length > 0)
            {
                for(int i = 0; i < hits.Length; i++)
                {
                    if(!hits[i].collider.isTrigger)
                    {
                        if(hits[i].point.y < transform.position.y + m_stepHeight)
                        {
                            Vector3 stepUpPosition = hits[i].point + Vector3.up * m_stepHeight;
                            Vector3 stepCheckOrigin = transform.position + Vector3.up * m_stepHeight;
                            Vector3 stepUpDirection = stepUpPosition - stepCheckOrigin;

                            RaycastHit hitti = new RaycastHit();

                            if(Physics.Raycast(stepCheckOrigin, stepUpDirection.normalized, out hitti, stepUpDirection.magnitude))
                            {
                                /*
                                Debug.DrawRay(hitti.point, Vector3.up, Color.blue);
                                Debug.DrawRay(hitti.point, Vector3.right, Color.blue);

                                Debug.DrawRay(stepCheckOrigin, stepUpDirection, Color.red);

                                Debug.Break();
                                Debug.DrawRay( hits[i].point, Vector3.up, Color.magenta);
                                Debug.DrawRay(transform.position, playerControlledMovement * 10f, Color.green);
                                Debug.Log("player hit obstacle: " + hits[i].collider.gameObject.name);
                                */

                                willHitObstacle = true;
                                break;
                            }
                        }
                    }
                }
            }

            if(!willHitObstacle)
            {
                float maxHeightOffset = 1f;

                Debug.DrawRay(transform.position + playerControlledMovement + Vector3.up * maxHeightOffset, Vector3.down * 2*maxHeightOffset );
                RaycastHit[] hits2 = Physics.RaycastAll(transform.position + playerControlledMovement + Vector3.up * maxHeightOffset, Vector3.down, 2*maxHeightOffset);
                
                List<Vector3> possiblePositions = new List<Vector3>();

                if(hits2 != null && hits2.Length > 0)
                {
                    for(int i = 0; i < hits2.Length; i++)
                    {
                        if(!hits2[i].collider.isTrigger && hits2[i].collider != m_collider)
                        {
                            possiblePositions.Add(hits2[i].point);
                        }
                    }
                }

                float closestDistance = float.MaxValue;
                Vector3? closestPoint = null;
                float tempDistance;

                for(int i = 0; i < possiblePositions.Count; i++)
                {
                    tempDistance = Vector3.Distance(transform.position, possiblePositions[i]);

                    if(tempDistance < closestDistance)
                    {
                        closestDistance = tempDistance;
                        closestPoint = possiblePositions[i];
                    }
                }

                if(closestPoint != null)
                {
                    transform.position = (Vector3)closestPoint;
                }
            }

            float rotationDirection = Vector2.SignedAngle(moveDirection,characterForward);
            rotationDirection = rotationDirection / Mathf.Abs(rotationDirection);
            
            if(toRotateAngle != 0f)
            {
                transform.Rotate(new Vector3(0,rotationDirection * toRotate * m_rotationSpeed * Time.deltaTime,0));
            }
        }
        else
        {
            m_isMoving = false;
            m_animator.SetBool("IsWalking",false);
        }
    }

    private bool checkIfLayerInLayerMask(int layerMask, int layerIndex)
    {
        return (layerMask & (int)Mathf.Pow(2,layerIndex)) == 1;
    }

    private void playerMetabolism()
    {
        m_food -= m_livingHunger * Time.deltaTime;
        m_water -= m_livingThirst * Time.deltaTime;

        if(m_isMoving)
        {
            m_food -= m_moveHunger * Time.deltaTime;
            m_water -= m_moveThirst * Time.deltaTime;
        }

        PlayerUIInterface.Singleton.setFood(Mathf.Round(m_food), m_maxFood);
        PlayerUIInterface.Singleton.setWater(Mathf.Round(m_water), m_maxWater);
    }

    public override void onDamage(Vector3 position, params DamageData[] damageData)
    {
        base.onDamage(position, damageData);

        PlayerUIInterface.Singleton.setHealth(health, m_maxHealth);
    }

    private void fixFallThruGround()
    {
        if(transform.position.y < -10f)
        {
            RaycastHit hit = new RaycastHit();

            Physics.Raycast(new Vector3(transform.position.x,100f,transform.position.z),Vector3.down, out hit);

            if(hit.point != Vector3.zero)
            {
                transform.position = hit.point + Vector3.up * 3f;
            }
        }
    }

    private void dropAllItems()
    {
        dropItems(PlayerUIInterface.Singleton.getPlayerInventoryContainer().getItemsData());
        dropItems(PlayerUIInterface.Singleton.getPlayerEquipmentContainer().getItemsData());

        PlayerUIInterface.Singleton.getPlayerInventoryContainer().clear();
        PlayerUIInterface.Singleton.getPlayerEquipmentContainer().clear();
    }

    private void dropItems(System.Tuple<int,Dictionary<string,string>>[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            if(items[i].Item1 == -1)
            {
                continue;
            }

            System.Tuple<string,string>[] additionalData = new System.Tuple<string,string>[items[i].Item2.Count];

            int counter1 = 0;

            foreach(KeyValuePair<string,string> pair in items[i].Item2) 
            {
                additionalData[counter1] = new System.Tuple<string, string>(pair.Key, pair.Value);
                counter1++;
            }

            GameObject drop = ItemsInterface.singleton.spawnItemDropWorld(items[i].Item1, additionalData);
            drop.transform.position = transform.position;
            drop.transform.rotation = drop.transform.rotation * BasicTools.Random.RandomValuesSeed.getRandomRotationYAxis(i * 0.33f, Time.deltaTime);
        }
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public Skills getSkills()
    {
        return m_skills;
    }

    public float getAttractiveness()
    {
        return (m_health/ m_maxHealth + m_food/m_maxFood + m_water/m_maxWater) / 3f;
    }

    public Gender getGender()
    {
        return m_gender;
    }

    public void setPregnant(Reproducer parent)
    {
        m_latestReproducersSociety = parent.getGameObject().GetComponent<NPCHumanoidBase>().societyManager;
        m_isPregnant = true;
        m_pregnancyStartTime = Time.time;
        WorldTextManager.Singleton.showText("Is pregnant", transform.position, transform, Color.black);
    }

    public bool addToPlayerInventory(int itemIndex)
    {
        return PlayerUIInterface.Singleton.getPlayerInventoryContainer().tryAddItemNextFreeSlot(itemIndex);
    }
    public bool addToPlayerInventory(UIItemData itemData)
    {
        return PlayerUIInterface.Singleton.getPlayerInventoryContainer().tryAddItemNextFreeSlot(itemData);
    }

    public void addExperienceSkill(string skill, float experience)
    {
        m_skills.addExperienceSkill(skill,experience);
    }

    public int getSkillLevel(string skill)
    {
        return m_skills.getSkillLevel(skill);
    }

    private void onEquipmentChanged(object sender, UIItemContainerChangedEventArgs args)
    {


        UIItemData itemData = args.m_changedSlot.GetComponentInChildren<UIItemData>();
        UIItemContainer source = sender as UIItemContainer;

        switch(args.m_changedSlotIndex)
        {
            case 0: // weapon-slot
            {
                unequipWeapon();

                int itemIndex = itemData.itemIndex;

                if(itemIndex > 0)
                {
                    m_activeWeapon = equipNewItem(itemData);
                    m_activeWeaponScript = m_activeWeapon.GetComponent<BaseWeapon>();
                    setHoldingAnimation(m_activeWeaponScript.holdingPose);
                    m_activeWeaponScript.setCarrierSkills(m_skills);
                }
                else
                {
                    setHoldingAnimation(HoldingPose.Undefined);
                }
                break;
            }
            case 1: // comsumables
                {
                    if (args.m_newItemIndex == 0) // water
                    {
                        m_water += 10f;
                        if (m_water > m_maxWater)
                        {
                            m_water = m_maxWater;
                        }

                        BasicTools.Audio.SoundManager.singleton.playGlobalSound(19);
                    }
                    else if (args.m_newItemIndex == 1) // apple
                    {
                        m_food += 10f;
                        if (m_food > m_maxFood)
                        {
                            m_food = m_maxFood;
                        }

                        m_health += 10;
                        if (m_health > m_maxHealth)
                        {
                            m_health = m_maxHealth;
                        }
                        PlayerUIInterface.Singleton.setHealth(health, m_maxHealth);

                        BasicTools.Audio.SoundManager.singleton.playGlobalSound(18);
                    }

                    if (args.m_newItemIndex > -1)
                    {
                        source.removeItems(itemData.itemIndex, 1);
                    }

                    break;
                }
            default:
            {
                Debug.LogError("onEquipmentChanged: unknown item-slot-index: " + args.m_changedSlotIndex);
                break;
            }
        }
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

    private void setIsAttacking(bool state)
    {
        m_isAttacking = state;
        m_animator.SetBool("IsAttacking",state);
    }


}
