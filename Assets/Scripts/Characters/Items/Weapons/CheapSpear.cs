using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheapSpear : BaseWeapon
{
    private bool m_isDamaging = false;
    private List<GameObject> m_processedTargets = new List<GameObject>();
    private Collider m_collider;
    private int m_lastFramePlayedImpactSound = 0;

    void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void onStartAttack()
    {
        m_isDamaging = true;
        BasicTools.Audio.SoundManager.singleton.playSoundAt(10, transform.position, BasicTools.Audio.Sound.SoundPlaystyle.Once);
    }

    public override void onEndAttack()
    {
        m_isDamaging = false;
        m_processedTargets.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if(m_isDamaging)
        {
            Damageable target = other.GetComponent<Damageable>();

            if(target != null)
            {
                if(!m_processedTargets.Contains(target.gameObject))
                {
                    if(!(m_carrierDamagable != null && target.hasSameGroup(m_carrierDamagable)))
                    {
                        target.onDamage(m_collider.ClosestPoint(other.transform.position), getDamageData());
                        m_processedTargets.Add(target.gameObject);

                        if(Time.frameCount > m_lastFramePlayedImpactSound)
                        {
                            m_lastFramePlayedImpactSound = Time.frameCount;
                            BasicTools.Audio.SoundManager.singleton.playSoundAt(16, transform.position);
                        }

                        m_carrierSkills.addExperienceSkill("Spears", 1);
                    }
                }
            }
        }
    }
}
