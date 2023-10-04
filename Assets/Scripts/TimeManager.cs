using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float m_normalTimeScale = 1f;
    [SerializeField] private int m_dayTimeScale = 1;
    [SerializeField] private int m_daysInAYear = 365;

    [Header("Date Time")]
    [SerializeField] private int m_currentSecounds = 0;
    [SerializeField] private int m_currentMinutes = 0;
    [SerializeField] private int m_currentHours = 0;
    [SerializeField] private int m_currentDay = 0;
    [SerializeField] private int m_currentYear = 0;

    private System.TimeSpan m_timeSinceStart = new System.TimeSpan();
    private float m_oneSecoundCounter = 0f;

    void Update()
    {
        if(Time.timeScale != m_normalTimeScale)
        {
            Time.timeScale = m_normalTimeScale;
        }

        m_oneSecoundCounter += Time.deltaTime;

        while(m_oneSecoundCounter > 1f)
        {
            m_oneSecoundCounter -= 1f;
            m_timeSinceStart = m_timeSinceStart.Add(new System.TimeSpan(0,0,1 * m_dayTimeScale));
            m_currentSecounds = m_timeSinceStart.Seconds;
            m_currentMinutes = m_timeSinceStart.Minutes;
            m_currentHours = m_timeSinceStart.Hours;

            while(m_timeSinceStart.Days > 0)
            {
                m_timeSinceStart = m_timeSinceStart.Add(new System.TimeSpan(-1,0,0,0));
                m_currentDay++;

                if(m_currentDay > m_daysInAYear)
                {
                    m_currentDay = 0;
                    m_currentYear++;
                }
            }
        }
    }

}
