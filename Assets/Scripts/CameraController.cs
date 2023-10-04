using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] public GameObject observationTarget = null;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float m_minDistance = 5f;
    [SerializeField] private float m_maxDistance = 5f;

    void Start()
    {
        
    }

    void Update()
    {
        if(observationTarget != null)
        {
            transform.position = observationTarget.transform.position + offset;
        }
    }

    public void changeCameraDistance(float moveDistance)
    {
        Vector3 direction = (transform.position - observationTarget.transform.position).normalized;

        offset -= direction * moveDistance;
        float distance = Vector3.Distance(transform.position, observationTarget.transform.position);

        if(distance < m_minDistance)
        {
            offset = direction * m_minDistance;
        }
        else if(distance > m_maxDistance)
        {
            offset = direction * m_maxDistance;
        }
    }

}
