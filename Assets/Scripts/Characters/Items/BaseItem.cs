using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI.Inventory;

public enum HoldingPose{Undefined, Spear}

public class BaseItem : MonoBehaviour
{
    [SerializeField] private Vector3 m_positionOffset = Vector3.zero;
    [SerializeField] private Vector3 m_rotationOffset = Vector3.zero;
    [SerializeField] private HoldingPose m_holdingPose = HoldingPose.Undefined;

    public HoldingPose holdingPose {get{return m_holdingPose;}}

    protected Skills m_carrierSkills = null;

    public void setCarrierSkills(Skills skills)
    {
        m_carrierSkills = skills;
    }

    public void applyOffset()
    {
        transform.localPosition = m_positionOffset;
        transform.localRotation = Quaternion.Euler(m_rotationOffset);
    }

    public virtual void applyItemData(UIItemData data)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
