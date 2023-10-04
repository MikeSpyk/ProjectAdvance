using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private GameObject m_spawnPrefab;
    [SerializeField] private SpawnerArea[] m_spawnAreas;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [SerializeField] private int m_maxAliveChildren = 10;
    [SerializeField] private float m_timeBetweenSpawns = 10f;
    [SerializeField] private bool m_startSpawnAll = false;
    [SerializeField] private Terrain m_baseTerrain;
    [Header("Min Distance")]
    [SerializeField] private bool m_minDistanceEnabled = true;
    [SerializeField] private float m_minDistance = 1f; // between 2 children
    private float[] m_spawnAreaSizes = null;
    private float[] m_spawnAreaCapacities = null;
    private float m_spawnCost = 0f;
    private float m_lastTimeSpawned = 0f;
    private List<GameObject> m_childrenAlive = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        setUpSpawnAreas();

        if(m_startSpawnAll)
        {
            float nameOffset = gameObject.name.GetHashCode() / 1000000f;

            for(int i = 0; i< m_maxAliveChildren; i++)
            {
                spawn(i * 0.33f + nameOffset);
            }
        }
    }

    private void setUpSpawnAreas()
    {
        m_spawnAreaSizes = new float[m_spawnAreas.Length];
        m_spawnAreaCapacities = new float[m_spawnAreas.Length];
        m_spawnCost = m_spawnAreaSizes[0];

        for(int i = 0; i < m_spawnAreaSizes.Length; i++)
        {
            m_spawnAreaSizes[i] = m_spawnAreas[i].getSurfaceArea();
            m_spawnAreaCapacities[i] = m_spawnAreaSizes[i];
            m_spawnCost = Mathf.Min(m_spawnCost, m_spawnAreaSizes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_childrenAlive.Count <= m_maxAliveChildren && Time.time > m_lastTimeSpawned + m_timeBetweenSpawns)
        {
            spawn();
            m_lastTimeSpawned = Time.time;
        }
    }

    private void spawn(float seed = 0f)
    {
        SpawnerArea spawnArea = getNextSpawnArea();

        Vector2 spawnPositionXZ = spawnArea.getRandomPositionWithin(Time.time * 0.33f + seed);
        Vector3 spawnPosition = new Vector3(spawnPositionXZ.x,0f,spawnPositionXZ.y);

        RaycastHit tempHit;
        if(m_baseTerrain == null)
        {
            tempHit = new RaycastHit();
            if(Physics.Raycast(new Vector3(spawnPositionXZ.x,2000f,spawnPositionXZ.y), Vector3.down, out tempHit))
            {
                spawnPosition = tempHit.point;
            }
            else
            {
                Debug.LogError("no raycast hit on surface");
            }
        }
        else
        {
            spawnPosition += Vector3.up * m_baseTerrain.SampleHeight(spawnPosition);
        }

        if(m_minDistanceEnabled)
        {
            bool tooClose = true;
            int tryCounter = 0;

            while(tryCounter < 10)
            {
                tooClose = false;

                for(int i = 0; i< m_childrenAlive.Count; i++)
                {
                    if(Vector3.Distance(m_childrenAlive[i].transform.position, spawnPosition) < m_minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if(tooClose)
                {
                    spawnPositionXZ = spawnArea.getRandomPositionWithin(Time.time * 0.33f + seed + 0.31f * tryCounter*tryCounter);

                    if(m_baseTerrain == null)
                    {
                        Physics.Raycast(new Vector3(spawnPositionXZ.x,2000f,spawnPositionXZ.y), Vector3.down, out tempHit);
                        spawnPosition = tempHit.point;
                    }
                    else
                    {
                        spawnPosition = new Vector3(spawnPositionXZ.x,0f,spawnPositionXZ.y);
                        spawnPosition += Vector3.up * m_baseTerrain.SampleHeight(spawnPosition);
                    }
                }

                tryCounter++;
            }

            if(tooClose)
            {
                Debug.LogWarning(gameObject.name + ": could not find spawn location after 10 tries. skipping this spawn");
                return;
            }

        }

        GameObject newSpawn = Instantiate(m_spawnPrefab, spawnPosition + spawnOffset,Quaternion.Euler(0,Mathf.PerlinNoise(seed, seed *1.33f) * 360f,0));
        m_childrenAlive.Add(newSpawn);
        newSpawn.AddComponent<OnDestroyCallback>().m_callbackAction = new System.Action<GameObject>(onChildDestroyed);
    }

    private SpawnerArea getNextSpawnArea()
    {
        List<int> availableSpawnAreasIndex = new List<int>();

        for(int i = 0; i < m_spawnAreaCapacities.Length; i++)
        {
            if(m_spawnAreaCapacities[i] > 0)
            {
                availableSpawnAreasIndex.Add(i);
            }
        }

        if(availableSpawnAreasIndex.Count == 0) // all spawn areas are exhausted. refill all
        {
            for(int i = 0; i < m_spawnAreaCapacities.Length; i++)
            {
                m_spawnAreaCapacities[i] += m_spawnAreaSizes[i];
                availableSpawnAreasIndex.Add(i);
            }
        }

        int randomIndex = Mathf.RoundToInt(Random.value * (availableSpawnAreasIndex.Count-1));
        m_spawnAreaCapacities[availableSpawnAreasIndex[randomIndex]] -= m_spawnCost;

        return m_spawnAreas[availableSpawnAreasIndex[randomIndex]];
    }

    private void onChildDestroyed(GameObject child)
    {
        m_childrenAlive.Remove(child);
    }

    public GameObject[] getChildren()
    {
        return m_childrenAlive.ToArray();
    }

}
