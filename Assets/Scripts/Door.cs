using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    enum MoveState{Opening, Closing, IsOpen, IsClosed}

    [SerializeField] private BasicTools.Triggers.GenericTriggerEvents m_openTrigger;
    [SerializeField] private Vector3 m_origin;
    [SerializeField] private float m_maxAngle = 180;
    [SerializeField] private float m_speed = 1f;
    [SerializeField] private float m_soundsCooldown = 1f;
    [SerializeField] private MoveState m_moveState = MoveState.IsClosed;

    private List<GameObject> m_gameObjectsInTrigger = new List<GameObject>();
    private Vector3 m_offsetRotation;
    private Vector3 m_startPosition;
    private Vector3 m_startRotation;
    private float m_lastRotationDivToOrigin = 0f;
    private float m_lastTimePlayedOpenSound = 0f;
    private float m_lastTimePlayedCloseSound = 0f;

    void OnDrawGizmosSelected()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(m_startPosition + m_origin, Vector3.up);
            Gizmos.DrawRay(m_startPosition, Vector3.up);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_startPosition, m_origin.magnitude);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_startPosition + m_origin,m_startPosition);
            Gizmos.DrawLine(m_startPosition, m_startPosition + Quaternion.Euler(0f,m_maxAngle,0f) * m_origin);
        }
        else
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position - m_origin, Vector3.up);
            Gizmos.DrawRay(transform.position, Vector3.up);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position - m_origin, m_origin.magnitude);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position - m_origin, transform.position);
            Gizmos.DrawLine(transform.position - m_origin, (transform.position - m_origin) + Quaternion.Euler(0f,m_maxAngle,0f) * m_origin );
        }
    }

    void Awake()
    {
        m_openTrigger.triggerEntered += onTriggerEnter;
        m_openTrigger.triggerExit += onTriggerExited;

        m_offsetRotation = m_origin;
        m_startPosition = transform.position - m_origin;
        if(m_origin == Vector3.zero)
        {
            m_startRotation = transform.forward;
        }
        else
        {
            m_startRotation = Quaternion.Euler(Quaternion.LookRotation(transform.forward).eulerAngles * -1) * transform.forward;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_moveState == MoveState.Opening)
        {
            transform.Rotate(0f,m_speed * Time.deltaTime,0f);
            m_offsetRotation = Quaternion.AngleAxis(m_speed * Time.deltaTime, Vector3.up) * m_offsetRotation;
            transform.position = m_startPosition + m_offsetRotation;

            float angleToOriginDistance = Vector3.Angle(transform.forward, m_startRotation);

            if(angleToOriginDistance > m_maxAngle)
            {
                transform.rotation = Quaternion.LookRotation(m_startRotation) * Quaternion.Euler(0f,m_maxAngle,0f);
                m_offsetRotation = Quaternion.LookRotation(m_startRotation) * Quaternion.Euler(0f,m_maxAngle,0f) * m_origin;
                transform.position = m_startPosition + m_offsetRotation;

                m_moveState = MoveState.IsOpen;
            }
        }
        else if(m_moveState == MoveState.Closing)
        {
            transform.Rotate(0f,-m_speed * Time.deltaTime,0f);
            m_offsetRotation = Quaternion.AngleAxis(-m_speed * Time.deltaTime, Vector3.up) * m_offsetRotation;
            transform.position = m_startPosition + m_offsetRotation;

            float angleToOriginDistance = Vector3.Angle(transform.forward, m_startRotation);

            if(angleToOriginDistance > m_lastRotationDivToOrigin) // div to orgin got bigger -> angle went through origin (0°)
            {
                transform.rotation = Quaternion.LookRotation(m_startRotation);
                m_offsetRotation = Quaternion.LookRotation(m_startRotation) * m_origin;
                transform.position = m_startPosition + m_offsetRotation;

                m_moveState = MoveState.IsClosed;
            }

            m_lastRotationDivToOrigin = angleToOriginDistance;
        }
    }

    void onTriggerEnter(object sender, Collider other)
    {
        if(!other.isTrigger)
        {
            m_gameObjectsInTrigger.Add(other.gameObject);

            if(m_moveState != MoveState.Opening)
            {
                if(Time.time > m_lastTimePlayedOpenSound + m_soundsCooldown)
                {
                    BasicTools.Audio.SoundManager.singleton.playSoundAt(14, transform.position);
                    m_lastTimePlayedOpenSound = Time.time;
                }
            }

            m_moveState = MoveState.Opening;
        }
    }

    void onTriggerExited(object sender, Collider other)
    {
        if(!other.isTrigger)
        {
            m_gameObjectsInTrigger.Remove(other.gameObject);

            if(m_gameObjectsInTrigger.Count < 1)
            {
                if(m_moveState != MoveState.Closing)
                {
                    if(Time.time > m_lastTimePlayedCloseSound + m_soundsCooldown)
                    {
                        BasicTools.Audio.SoundManager.singleton.playSoundAt(13, transform.position);
                        m_lastTimePlayedCloseSound = Time.time;
                    }
                }

                m_moveState = MoveState.Closing;

                m_lastRotationDivToOrigin = float.MaxValue;
            }
        }
    }
}
