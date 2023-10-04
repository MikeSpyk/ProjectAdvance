using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HideTrigger : MonoBehaviour
{
    [SerializeField] private GameObject m_objectToHide;
    [SerializeField] private string m_triggerGameObjectTag = "";

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals(m_triggerGameObjectTag))
        {
            m_objectToHide.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag.Equals(m_triggerGameObjectTag))
        {
            m_objectToHide.SetActive(true);
        }
    }

}
