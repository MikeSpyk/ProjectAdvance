using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LightningAura : MonoBehaviour
{
    [SerializeField] private GameObject m_lineRendererPrefab;
    [SerializeField] private float m_lightningLifeTime = 1f;
    [SerializeField, Min(0.001f)] private float m_spawnCooldown = 1f;
    [SerializeField] private float m_spawnHeight = 1f;
    [SerializeField, Min(3)] private int m_lightningSteps = 5;
    [SerializeField, Range(0,360)] private float m_stepMaxAngle = 45;
    [SerializeField] private float m_lightningSize = 1f;
    [SerializeField] private Gradient m_color;
    [SerializeField] private float m_width = 1f;

    private float m_lastTimeLightningSpawn = 0f;
    private List<Tuple<LineRenderer,float,Vector3[]>> m_activeLightnings_spawnTime_positions = new List<Tuple<LineRenderer, float, Vector3[]>>();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * m_spawnHeight);
    }

    void Update()
    {
        // TODO: cache für lineRenderer-gameobject

        if(Time.time > m_lastTimeLightningSpawn + m_spawnCooldown)
        {
            Vector3 originPosition = transform.up * Mathf.PerlinNoise(Time.time * 13.33f, Time.time * 9.33f) * m_spawnHeight;
            GameObject newLightning = Instantiate(m_lineRendererPrefab, originPosition, Quaternion.identity, transform);

            LineRenderer lineRenderer = newLightning.GetComponent<LineRenderer>();
            lineRenderer.positionCount = m_lightningSteps;

            Vector3 currentDirection = new Vector3(
                                                    Mathf.PerlinNoise(Time.deltaTime, Time.time * 6.33f) * 2f - 1f,
                                                    Mathf.PerlinNoise(Time.deltaTime, Time.time * 6.61f) * 2f - 1f,
                                                    Mathf.PerlinNoise(Time.deltaTime, Time.time * 6.97f) * 2f - 1f
                                                    ).normalized;

            Vector3[] lightningPositions = new Vector3[m_lightningSteps];

            lightningPositions[0] = originPosition;
            lightningPositions[1] = originPosition + currentDirection * m_lightningSize;

            for(int i = 2; i< m_lightningSteps; i++)
            {
                currentDirection = Quaternion.Euler(
                                    (Mathf.PerlinNoise(Time.deltaTime, Time.time * 6.33f * i) * 2f - 1f) * m_stepMaxAngle,
                                    (Mathf.PerlinNoise(Time.deltaTime, Time.time * 6.61f * i) * 2f - 1f) * m_stepMaxAngle,
                                    (Mathf.PerlinNoise(Time.deltaTime, Time.time * 6.97f * i) * 2f - 1f) * m_stepMaxAngle) * currentDirection;

                lightningPositions[i] = lightningPositions[i-1] + currentDirection * m_lightningSize;
            }

            updatePositionToParent(lineRenderer,lightningPositions);

            lineRenderer.colorGradient = m_color;
            lineRenderer.startWidth = m_width;
            lineRenderer.endWidth = m_width;

            m_activeLightnings_spawnTime_positions.Add(new Tuple<LineRenderer, float, Vector3[]>(lineRenderer,Time.time, lightningPositions));

            m_lastTimeLightningSpawn = Time.time;
        }

        if(m_activeLightnings_spawnTime_positions.Count > 0)
        {
            if(Time.time > m_activeLightnings_spawnTime_positions[0].Item2 + m_lightningLifeTime)
            {
                Destroy(m_activeLightnings_spawnTime_positions[0].Item1.gameObject);
                m_activeLightnings_spawnTime_positions.RemoveAt(0);
            }
        }

        for(int i = 0; i < m_activeLightnings_spawnTime_positions.Count; i++)
        {
            updatePositionToParent(m_activeLightnings_spawnTime_positions[i].Item1, m_activeLightnings_spawnTime_positions[i].Item3);
        }
    }

    void updatePositionToParent(LineRenderer lineRenderer, Vector3[] relativPositions)
    {
        Vector3[] absolutPositions = new Vector3[relativPositions.Length];

        for(int i = 0; i < absolutPositions.Length; i++)
        {
            absolutPositions[i] = transform.position + relativPositions[i];
        }

        lineRenderer.SetPositions(absolutPositions);
    }

}
