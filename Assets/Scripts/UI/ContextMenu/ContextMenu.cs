using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BasicTools.UI;

public class ContextMenu : MonoBehaviour
{
    [SerializeField] private GameObject m_buttonPrefab;
    [SerializeField] private string m_borderWhitespace = "  "; // not working with current unity version https://forum.unity.com/threads/textmeshpro-ignoring-non-breaking-space.981690/

    private bool m_isMenuVisible = false;
    private int m_frameMenuCreated = 0;
    private HashSet<GameObject> m_currentMenuGameobjects = new HashSet<GameObject>();
    private Dictionary<GameObject, ContextMenuItemBase> m_uiGameObjects_ContextMenuItem = new Dictionary<GameObject, ContextMenuItemBase>();
    private GameObject m_lastMouseOverUiGameObject = null;
    private List<float> m_columnMaxWidths = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateMenuVisibility();
    }

    private void updateMenuVisibility()
    {
        if(m_isMenuVisible && Time.frameCount > m_frameMenuCreated)
        {
            if(!checkMouseOverMenu())
            {
                hideMenu();
            }
        }
    }

    private bool checkMouseOverMenu()
    {
        List<UnityEngine.EventSystems.RaycastResult> uiElements = PlayerUIInterface.Singleton.getUIGameobjectsUnderMouse();

        for(int i = 0; i < uiElements.Count; i++)
        {
            if(m_currentMenuGameobjects.Contains(uiElements[i].gameObject))
            {
                if(uiElements[i].gameObject != m_lastMouseOverUiGameObject)
                {
                    onUiElementUnderCursorChanged(m_lastMouseOverUiGameObject, uiElements[i].gameObject);
                    m_lastMouseOverUiGameObject = uiElements[i].gameObject;
                }

                return true;
            }
        }

        return false;
    }

    private void onUiElementUnderCursorChanged(GameObject lastGameobject, GameObject newGameobject)
    {
        ContextMenuItemBase lastUiElement = m_uiGameObjects_ContextMenuItem[lastGameobject];
        ContextMenuItemBase newUiElement = m_uiGameObjects_ContextMenuItem[newGameobject];

        ContextMenuItemParent newAsParent = newUiElement as ContextMenuItemParent;
        ContextMenuItemParent lastAsParent = lastUiElement as ContextMenuItemParent;

        if(newAsParent != null)
        {
            showTreeBranch(newAsParent.children);
        }

        if(newUiElement.m_depth < lastUiElement.m_depth)
        {
            if(newAsParent == null)
            {
                hideTreeBranch(lastUiElement.m_parent.children);
            }
            else
            {
                if(newAsParent != lastUiElement.m_parent)
                {
                    hideTreeBranch(lastUiElement.m_parent.children);
                }
            }
        }

        if(lastAsParent != null)
        {
            if(newUiElement.m_depth <= lastUiElement.m_depth)
            {
                hideTreeBranch(lastAsParent.children);
            }
        }

    }

    private void hideMenu()
    {
        foreach(GameObject obj in m_currentMenuGameobjects)
        {
            obj.SetActive(false);
        }

        m_isMenuVisible = false;
    }

    private void deleteAllUiGameObjects()
    {
        foreach(GameObject obj in m_currentMenuGameobjects)
        {
            Destroy(obj);
        }
    }

    public void showContextMenu(ContextMenuItemBase[] items)
    {
        deleteAllUiGameObjects();

        m_currentMenuGameobjects.Clear();
        m_uiGameObjects_ContextMenuItem.Clear();

        Vector3 mousePos = Input.mousePosition;

        float highestYPos = float.MinValue;
        float buttonHeight = (m_buttonPrefab.transform as RectTransform).sizeDelta.y;

        createTreeBranch(null, items, m_columnMaxWidths, 0, 0f, ref highestYPos);

        float widthSum = 0f;

        for(int i = 0; i < m_columnMaxWidths.Count; i++)
        {
            widthSum += m_columnMaxWidths[i];
        }

        Vector2 outerBounds = new Vector2(widthSum, highestYPos);
        ExpandDirectionX expandDirectionX;
        ExpandDirectionY expandDirectionY;
        UITools.findDirectionToExpandUIAtMousePosition(outerBounds, out expandDirectionX, out expandDirectionY);

        // apply Sizes and positions
        {
            Vector2 mouseOffset = new Vector2(5f,5f); // move everything a litte bit close towards mouse or it will be awkward to stay over menu

            float baseOffsetX;
            float baseOffsetY;

            if(expandDirectionX == ExpandDirectionX.Right)
            {
                baseOffsetX = mousePos.x + m_columnMaxWidths[0]/2 - mouseOffset.x;
            }
            else // left
            {
                baseOffsetX = mousePos.x - m_columnMaxWidths[0]/2 + mouseOffset.x;
            }

            if(expandDirectionY == ExpandDirectionY.Down)
            {
                baseOffsetY = mousePos.y - buttonHeight/2 + mouseOffset.y;
            }
            else // up
            {
                baseOffsetY = mousePos.y + buttonHeight/2 - mouseOffset.y;
            }

            arrangeTreeBranch(items, baseOffsetX, baseOffsetY, 0, expandDirectionY, expandDirectionX);
        }

        hideMenu();
        showTreeBranch(items);

        m_lastMouseOverUiGameObject = items[0].m_associatedButton.gameObject;

        ContextMenuItemParent firstElementAsParent = items[0] as ContextMenuItemParent;
        if(firstElementAsParent != null)
        {
            showTreeBranch(firstElementAsParent.children);
        }
        
        m_isMenuVisible = true;
        m_frameMenuCreated = Time.frameCount;
    }

    private void arrangeTreeBranch(ContextMenuItemBase[] items, float offsetX, float offsetY, int depth, ExpandDirectionY expandDirectionY, ExpandDirectionX expandDirectionX)
    {
        float tempOffsetY;

        for(int i = 0; i < items.Length; i++)
        {
            RectTransform currentRect = (RectTransform)items[i].m_associatedButton.transform;
            currentRect.sizeDelta = new Vector2(m_columnMaxWidths[depth], currentRect.sizeDelta.y);
            TMPro.TMP_Text text = items[i].m_associatedButton.GetComponentInChildren<TMPro.TMP_Text>();

            if(expandDirectionY == ExpandDirectionY.Down)
            {
                tempOffsetY = -currentRect.sizeDelta.y * i;
            }
            else
            {
                tempOffsetY = currentRect.sizeDelta.y * i;
            }

            currentRect.position = new Vector3(offsetX, offsetY, 0f) + new Vector3(0f, tempOffsetY ,0f);

            ContextMenuItemParent parentItem = items[i] as ContextMenuItemParent;

            if(expandDirectionX == ExpandDirectionX.Left)
            {
                text.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            }
            else // right
            {
                text.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Right;
            }

            if(parentItem != null)
            {
                if(expandDirectionX == ExpandDirectionX.Left)
                {
                    text.text = string.Format("< {0}", parentItem.m_text); // change expand letter from right to left. right is default
                }

                float nextOffsetX;

                if(expandDirectionX == ExpandDirectionX.Right)
                {
                    nextOffsetX = offsetX + m_columnMaxWidths[depth]/2 + m_columnMaxWidths[depth+1]/2;
                }
                else
                {
                    nextOffsetX = offsetX - m_columnMaxWidths[depth]/2 - m_columnMaxWidths[depth+1]/2;
                }

                arrangeTreeBranch(parentItem.children, nextOffsetX, currentRect.position.y, depth+1, expandDirectionY, expandDirectionX);
            }
        }
    }

    private void showTreeBranch(ContextMenuItemBase[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            items[i].m_associatedButton.gameObject.SetActive(true);
        }
    }

    private void hideTreeBranch(ContextMenuItemBase[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            items[i].m_associatedButton.gameObject.SetActive(false);

            ContextMenuItemParent itemAsParent = items[i] as ContextMenuItemParent;

            if(itemAsParent!= null)
            {
                if(itemAsParent.children[0].m_associatedButton.gameObject.activeSelf)
                {
                    hideTreeBranch(itemAsParent.children);
                }
            }
        }
    }

    private void createTreeBranch(ContextMenuItemParent parent, ContextMenuItemBase[] items, List<float> columnMaxWidths, int depth, float yOffset, ref float highestYPos)
    {
        if(columnMaxWidths.Count <= depth)
        {
            columnMaxWidths.Add(float.MinValue);
        }

        // calculate outer bounds
        for(int i = 0; i < items.Length; i++)
        {
            Button newButton = Instantiate(m_buttonPrefab, transform).GetComponent<Button>();

            items[i].m_associatedButton = newButton;
            items[i].m_depth = depth;
            items[i].m_parent = parent;
            m_currentMenuGameobjects.Add(newButton.gameObject);
            m_uiGameObjects_ContextMenuItem.Add(newButton.gameObject, items[i]);
            
            ContextMenuItemParent parentItem = items[i] as ContextMenuItemParent;

            TMPro.TMP_Text text = newButton.GetComponentInChildren<TMPro.TMP_Text>();

            if(parentItem == null)
            {
                text.text = string.Format("{0}{1}{0}", m_borderWhitespace, items[i].m_text);
            }
            else
            {
                text.text = string.Format("{0}{1} >{0}", m_borderWhitespace, items[i].m_text);
            }

            RectTransform rect = newButton.transform as RectTransform;

            rect.sizeDelta = new Vector2(text.preferredWidth, rect.sizeDelta.y);
            columnMaxWidths[depth] = Mathf.Max(columnMaxWidths[depth],rect.sizeDelta.x);
            rect.position = new Vector3(rect.position.x, yOffset, 0f);
            highestYPos = Mathf.Max(highestYPos, rect.position.y + rect.sizeDelta.y/2);

            ContextMenuItem clickableItem = items[i] as ContextMenuItem;

            if(clickableItem != null)
            {
                newButton.onClick.AddListener(()=>
                {
                    clickableItem.doAction();
                    onMenuItemClicked();
                });
            }
            else
            {
                if(parentItem != null)
                {
                    createTreeBranch(parentItem, parentItem.children, columnMaxWidths, depth+1, yOffset, ref highestYPos);
                }
            }

            yOffset += rect.sizeDelta.y;
        }
    }

    private void onMenuItemClicked()
    {
        hideMenu();
    }
}
