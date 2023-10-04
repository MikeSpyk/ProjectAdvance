using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using BasicTools.UI.Tooltip;

namespace BasicTools
{
    namespace UI
    {
        public class UIManager : MonoBehaviour
        {
            public static UIManager Singleton
            {
                get
                {
                    return singleton;
                }
            }
            private static UIManager singleton;

            [SerializeField] private EventSystem m_eventSystem;
            [SerializeField] private GraphicRaycaster m_graphicRaycaster;
            [SerializeField] private int m_noTriggerLayerMask = 0;

            private ItemContainerWorld m_lastOpenedExternalContainer = null;
            private List<RaycastResult> m_lastUiRaycastResult = new List<RaycastResult>();
            private int m_lastFrameUiRaycast = 0;
            Dictionary<GameObject, float> m_menuLastTimeToggled = new Dictionary<GameObject, float>();
            Dictionary<GameObject, bool> m_menuCanBeClosedByUser = new Dictionary<GameObject, bool>();
            Dictionary<GameObject, int> m_menuNestedLevel = new Dictionary<GameObject, int>();
            private List<GameObject> m_activeGameObjects = new List<GameObject>();
            private List<GameObject> m_managedMenus = new List<GameObject>();

            protected virtual void Awake()
            {
                singleton = this;
            }

            protected virtual void Update()
            {
                notifyWorldObjectsUnderCursor();

                UITooltipData tooltipData = getUIComponentUnderMouse<UITooltipData>();

                if (tooltipData != null)
                {
                    showGenericTooltip(tooltipData);
                }
            }

            /// <summary>
            /// Adds Menu to be managed by this object. Managed Menus can be closed in the right order by using closeLatestAvailabelMenu()
            /// </summary>
            /// <param name="menu"></param>
            /// <param name="closeEscape">can this menu be closed by the user?</param>
            protected void addManagedMenu(GameObject menu, bool canBeClosedByUser)
            {
                if (m_managedMenus.Contains(menu))
                {
                    throw new System.ArgumentException("item is already in collection: " + menu.name);
                }

                if (menu.activeInHierarchy)
                {
                    m_activeGameObjects.Add(menu);
                }
                m_menuCanBeClosedByUser.Add(menu, canBeClosedByUser);
                m_menuLastTimeToggled.Add(menu, Time.time);
                m_menuNestedLevel.Add(menu, getNestedLevel(menu.transform));
                m_managedMenus.Add(menu);
            }

            protected GameObject closeLatestAvailableMenu()
            {
                float latestTimeMenuToggled = float.MinValue;
                int latestMenuIndex = -1;

                for (int i = 0; i < m_activeGameObjects.Count; i++)
                {
                    if (m_menuCanBeClosedByUser[m_activeGameObjects[i]] &&
                        m_menuLastTimeToggled[m_activeGameObjects[i]] > latestTimeMenuToggled)
                    {
                        latestTimeMenuToggled = m_menuLastTimeToggled[m_activeGameObjects[i]];
                        latestMenuIndex = i;
                    }
                }

                if (latestMenuIndex > -1)
                {
                    GameObject returnValue = m_activeGameObjects[latestMenuIndex];

                    closeMenu(m_activeGameObjects[latestMenuIndex]);

                    return returnValue;
                }

                return null;
            }

            protected void toggleManagedMenu(GameObject menu)
            {
                if (menu.activeInHierarchy)
                {
                    closeMenu(menu);
                }
                else
                {
                    openMenu(menu);
                }
            }

            protected virtual void closeMenu(GameObject menu)
            {
                menu.SetActive(false);
                m_activeGameObjects.Remove(menu);
            }

            protected virtual void openMenu(GameObject menu)
            {
                menu.SetActive(true);
                m_menuLastTimeToggled.Remove(menu);
                m_menuLastTimeToggled.Add(menu, Time.time);

                if (m_activeGameObjects.Contains(menu))
                {
                    Debug.LogWarning("Menu was disabled out of manager control. GameObject: \"" + menu.name + "\". Use closeMenu()!");
                }
                else
                {
                    m_activeGameObjects.Add(menu);
                }
            }

            private List<int> findDeepstNestedLevelMenus()
            {
                int deepestGameObjectLevel = int.MinValue;
                List<int> deepestLevelGameObjectsIndex = new List<int>();

                for (int i = 0; i < m_activeGameObjects.Count; i++)
                {
                    if (m_activeGameObjects[i].activeInHierarchy)
                    {
                        if (m_menuNestedLevel[m_activeGameObjects[i]] > deepestGameObjectLevel)
                        {
                            deepestGameObjectLevel = m_menuNestedLevel[m_activeGameObjects[i]];
                            deepestLevelGameObjectsIndex.Clear();
                        }

                        if (m_menuNestedLevel[m_activeGameObjects[i]] == deepestGameObjectLevel)
                        {
                            deepestLevelGameObjectsIndex.Add(i);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Menu was disabled out of manager control. GameObject: \"" + m_activeGameObjects[i].name + "\". Use closeMenu()!");
                        m_activeGameObjects.RemoveAt(i);
                        i--;
                    }
                }

                return deepestLevelGameObjectsIndex;
            }

            private int getNestedLevel(Transform transform)
            {
                int counter = 0;

                while (transform.parent != null && counter < 10000)
                {
                    transform = transform.parent;
                    counter++;
                }

                return counter;
            }

            private void notifyWorldObjectsUnderCursor()
            {
                RaycastHit hit = new RaycastHit();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, float.MaxValue, m_noTriggerLayerMask))
                {
                    CursorRayCastTarget target = hit.collider.GetComponent<CursorRayCastTarget>();
                    //Debug.Log("under Mouse: " + hit.collider.gameObject.name);

                    if (target != null)
                    {
                        target.onCursorOver();
                    }
                }
            }

            private void showGenericTooltip(UITooltipData data)
            {
                TooltipManager.Singleton.setTooltipData(data.GetHashCode(), data.textData);
            }

            public T getUIComponentUnderMouse<T>()
            {
                List<RaycastResult> results = getUIGameobjectsUnderMouse();

                //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                foreach (RaycastResult result in results)
                {
                    T component = result.gameObject.GetComponent<T>();

                    if (component != null)
                    {
                        return component;
                    }
                }

                return default(T);
            }

            public List<RaycastResult> getUIGameobjectsUnderMouse()
            {
                if (Time.frameCount != m_lastFrameUiRaycast)
                {
                    //Set up the new Pointer Event
                    PointerEventData m_PointerEventData = new PointerEventData(m_eventSystem);
                    //Set the Pointer Event Position to that of the mouse position
                    m_PointerEventData.position = Input.mousePosition;

                    //Create a list of Raycast Results
                    m_lastUiRaycastResult.Clear();

                    //Raycast using the Graphics Raycaster and mouse click position
                    m_graphicRaycaster.Raycast(m_PointerEventData, m_lastUiRaycastResult);

                    m_lastFrameUiRaycast = Time.frameCount;
                }

                return m_lastUiRaycastResult;
            }
        }
    }
}
