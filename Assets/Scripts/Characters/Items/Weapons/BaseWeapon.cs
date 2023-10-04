using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI.Inventory;

public class BaseWeapon : BaseItem
{
    [SerializeField] private float m_swingTime = 1f;
    [Header("Stats")]
    [SerializeField] private float m_physicalDamage = 1f;
    [SerializeField] private float m_fireDamage = 0f;
    [SerializeField] private float m_iceDamage = 0f;
    [SerializeField] private float m_lightningDamage = 0f;

    [Header("Effects")]
    [SerializeField] private GameObject m_fireEffect;
    [SerializeField] private GameObject m_iceEffect;
    [SerializeField] private GameObject m_lightningEffect;

    private DamageData[] m_damageData;
    protected Damageable m_carrierDamagable = null;

    public float SwingTime
    {
        get
        {
            return m_swingTime;
        }
    }

    protected DamageData[] getDamageData()
    {
        return m_damageData;
    }

    protected Collider m_attackHitbox = null;
    private int m_carrierSkill = 0;

    public virtual void onStartAttack(){}

    public virtual void onEndAttack(){}

    // Start is called before the first frame update
    protected void Start()
    {
        m_attackHitbox = GetComponent<Collider>();

        initializeDamageTypes();
    }

    void FixedUpdate()
    {
        float previousSkill = m_carrierSkill;
        m_carrierSkill = m_carrierSkills.getSkillLevel("Spears");

        if(m_carrierSkill != previousSkill)
        {
            for(int i = 0; i < m_damageData.Length; i++)
            {
                if(m_damageData[i].m_damageType == DamageTypes.Physical)
                {
                    m_damageData[i].m_amount = m_physicalDamage * (1 + m_carrierSkill/10f);
                    break;
                }
            }
        }
    }

    public override void applyItemData(UIItemData data)
    {
        base.applyItemData(data);
        applyDamageEffectIfAvailable(data, "physicalDamage");
        applyDamageEffectIfAvailable(data, "fireDamage");
        applyDamageEffectIfAvailable(data, "iceDamage");
        applyDamageEffectIfAvailable(data, "lightningDamage");
    }

    public void setCarrierDamageable(Damageable damageable)
    {
        m_carrierDamagable = damageable;
    }

    private void applyDamageEffectIfAvailable(UIItemData data, string effectName)
    {
        string effectAmount = data.getAdditionalData(effectName);

        if(effectAmount != null)
        {
            float amountParsed = float.Parse(effectAmount);

            switch(effectName)
            {
                case "physicalDamage":
                    m_physicalDamage = amountParsed;
                    break;
                case "fireDamage":
                    m_fireDamage = amountParsed;
                    break;
                case "iceDamage":
                    m_iceDamage = amountParsed;
                    break;
                case "lightningDamage":
                    m_lightningDamage = amountParsed;
                    break;
                default:
                    Debug.LogWarning("unknown Damage Effect: " + effectName);
                    break;
            }
        }
    }

    private void initializeDamageTypes()
    {
        List<DamageData> damageDataList = new List<DamageData>();

        if(m_physicalDamage > 0f)
        {
            damageDataList.Add(new DamageData(){m_amount = m_physicalDamage * (1 + m_carrierSkill), m_damageType = DamageTypes.Physical});
        }
        if(m_fireDamage > 0f)
        {
            damageDataList.Add(new DamageData(){m_amount = m_fireDamage, m_damageType = DamageTypes.Fire});
            activateGameobjectIfPresent(m_fireEffect);
        }
        if(m_iceDamage > 0f)
        {
            damageDataList.Add(new DamageData(){m_amount = m_iceDamage, m_damageType = DamageTypes.Ice});
            activateGameobjectIfPresent(m_iceEffect);
        }
        if(m_lightningDamage > 0f)
        {
            damageDataList.Add(new DamageData(){m_amount = m_lightningDamage, m_damageType = DamageTypes.Lightning});
            activateGameobjectIfPresent(m_lightningEffect);
        }

        m_damageData = new DamageData[damageDataList.Count];

        for(int i = 0; i < damageDataList.Count; i++)
        {
            m_damageData[i] = damageDataList[i];
        }
    }

    private static void activateGameobjectIfPresent(GameObject obj)
    {
        if(obj != null)
        {
            obj.SetActive(true);
        }
    }
}
