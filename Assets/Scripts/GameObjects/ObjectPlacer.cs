using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace GameObjects
    {
        public class ObjectPlacer : MonoBehaviour
        {
            [SerializeField] private Material m_bluprintMaterial;
            [SerializeField] private GameObject[] m_objectsPrefabs;
            [SerializeField] private Vector3[] m_prefabsOffset;

            private GameObject[] m_objectsBluprints = null;
            private GameObject m_currentBluprint = null;
            private Vector3 m_currentBluprintOffset = Vector3.zero;
            private int m_currentPrefabIndex = -1;
            private bool m_placingMode = false;
            private int m_rayCastLayerMask;
            private System.Func<bool> m_currentBluprintPlaceCondition = null;
            private System.Action m_currentBluprintBuiltCallback = null;

            void Awake()
            {
                cacheBluprints();
                m_rayCastLayerMask = (int.MaxValue - (int)Mathf.Pow(2, 10)); // everything except layer 10
            }

            // Start is called before the first frame update
            void Start()
            {

            }

            // Update is called once per frame
            void Update()
            {
                if (m_placingMode)
                {
                    RaycastHit hit = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit, float.MaxValue, m_rayCastLayerMask))
                    {
                        //Debug.Log("hit: "+hit.collider.gameObject.name);
                        //Debug.DrawRay(hit.point + m_currentBluprintOffset, Vector3.up);
                        m_currentBluprint.transform.position = hit.point + Quaternion.LookRotation(m_currentBluprint.transform.forward) * m_currentBluprintOffset;

                        if (Input.GetKey(KeyCode.R))
                        {
                            m_currentBluprint.transform.Rotate(0f, 1f, 0f);
                        }

                        if (Input.GetKeyDown(KeyCode.Mouse0) && m_currentBluprintPlaceCondition())
                        {
                            m_currentBluprintBuiltCallback();
                            Instantiate(m_objectsPrefabs[m_currentPrefabIndex], m_currentBluprint.transform.position, m_currentBluprint.transform.rotation);
                            BasicTools.Audio.SoundManager.singleton.playGlobalSound(20);
                            stopPlacingMode();
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        stopPlacingMode();
                    }
                }
            }

            private void cacheBluprints()
            {
                m_objectsBluprints = new GameObject[m_objectsPrefabs.Length];

                for (int i = 0; i < m_objectsPrefabs.Length; i++)
                {
                    m_objectsBluprints[i] = Instantiate(m_objectsPrefabs[i]);
                    m_objectsBluprints[i].transform.SetParent(this.transform);
                    setMaterialChildren(m_objectsBluprints[i].transform, m_bluprintMaterial);
                    removeComponentChildren<MonoBehaviour>(m_objectsBluprints[i].transform);
                    removeComponentChildren<Collider>(m_objectsBluprints[i].transform);
                    m_objectsBluprints[i].SetActive(false);
                }
            }

            private void setMaterialChildren(Transform source, Material material)
            {
                Renderer renderer = source.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = material;
                    }

                    renderer.materials = materials;
                }

                for (int i = 0; i < source.childCount; i++)
                {
                    setMaterialChildren(source.GetChild(i), material);
                }
            }

            private void removeComponentChildren<T>(Transform source)
            {
                T[] components = source.GetComponents<T>();
                if (components != null)
                {
                    for (int i = 0; i < components.Length; i++)
                    {
                        Destroy(components[i] as UnityEngine.Object);
                    }
                }

                for (int i = 0; i < source.childCount; i++)
                {
                    removeComponentChildren<T>(source.GetChild(i));
                }
            }

            private void stopPlacingMode()
            {
                m_placingMode = false;

                if (m_currentBluprint != null)
                {
                    m_currentBluprint.SetActive(false);
                }
            }

            public void startPlacingMode(int prefabIndex, GameObject placer, System.Func<bool> placeCondition, System.Action builtCallback)
            {
                m_currentBluprintPlaceCondition = placeCondition;
                m_currentBluprintBuiltCallback = builtCallback;

                m_currentPrefabIndex = prefabIndex;
                m_currentBluprint = m_objectsBluprints[prefabIndex];
                m_currentBluprint.SetActive(true);
                m_currentBluprintOffset = m_prefabsOffset[prefabIndex];
                m_placingMode = true;
            }
        }
    }
}