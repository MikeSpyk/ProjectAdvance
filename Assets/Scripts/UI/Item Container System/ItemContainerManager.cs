using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BasicTools
{
    namespace UI
    {
        namespace Inventory
        {
            public class ItemContainerManager : MonoBehaviour
            {
                private static ItemContainerManager singleton = null;
                public static ItemContainerManager Singleton
                {
                    get
                    {
                        return singleton;
                    }
                }

                public bool m_isActive = true;
                [SerializeField] private GameObject m_dragNDropIconParent; // needed to have right render order of drag and drop item while dragging (so that Drag item is not renderd behind container control)
                private UIItemData m_dragItem = null;
                private Image m_dragImage = null;
                private Transform m_dragItemParent = null;
                void Awake()
                {
                    singleton = this;
                }


                // Start is called before the first frame update
                void Start()
                {

                }

                // Update is called once per frame
                void Update()
                {
                    UIItemData mouseOverItem = PlayerUIInterface.Singleton.getUIComponentUnderMouse<UIItemData>();

                    if (mouseOverItem != null)
                    {
                        if (mouseOverItem.itemIndex > -1)
                        {
                            PlayerUIInterface.Singleton.showItemTooltip(mouseOverItem);
                        }
                    }

                    if (m_isActive || m_dragItem != null)
                    {
                        if (m_dragImage != null)
                        {
                            m_dragImage.rectTransform.position = Input.mousePosition;
                        }

                        if (mouseOverItem != null)
                        {
                            if (Input.GetKeyDown(KeyCode.Mouse0)) // drag
                            {
                                // TODO: what if m_dragItem is not null?
                                m_dragItem = mouseOverItem;

                                if (m_dragItem.transform.childCount > 0)
                                {
                                    m_dragImage = m_dragItem.transform.GetChild(0).GetComponent<Image>();

                                    m_dragItemParent = m_dragImage.transform.parent;
                                    m_dragImage.transform.SetParent(m_dragNDropIconParent.transform); // need to unattach from parent to stay ontop of render order
                                }
                            }
                        }

                        if (Input.GetKeyUp(KeyCode.Mouse0) && m_dragImage != null) // drop
                        {

                            m_dragImage.transform.SetParent(m_dragItemParent); // was needed to unattach from parent to stay ontop of render order

                            if (mouseOverItem == null) // drop outside of container
                            {
                                m_dragImage.rectTransform.localPosition = Vector3.zero;
                            }
                            else // drop on a container slot
                            {
                                UISlotData dropSlotData = mouseOverItem.GetComponent<UISlotData>();

                                if (dropSlotData.isItemAllowed(m_dragItem))
                                {
                                    switchUIItem(m_dragItem, mouseOverItem);
                                }
                                else
                                {
                                    m_dragImage.rectTransform.localPosition = Vector3.zero;
                                }
                            }

                            m_dragItem = null;
                            m_dragImage = null;
                        }
                    }
                }

                private void switchUIItem(UIItemData source, UIItemData target)
                {
                    // switch item images
                    Image sourceImage = null;
                    Image targetImage = null;

                    if (source.transform.childCount > 0)
                    {
                        sourceImage = source.transform.GetChild(0).GetComponent<Image>();
                    }

                    if (target.transform.childCount > 0)
                    {
                        targetImage = target.transform.GetChild(0).GetComponent<Image>();
                    }

                    if (sourceImage != null)
                    {
                        sourceImage.transform.SetParent(target.transform);
                        sourceImage.rectTransform.localPosition = Vector3.zero;
                    }

                    if (targetImage != null)
                    {
                        targetImage.transform.SetParent(source.transform);
                        targetImage.rectTransform.localPosition = Vector3.zero;
                    }

                    // switch item Data
                    UIItemData.switchItemData(source, target);
                }

                public Sprite getItemIcon(int index)
                {
                    return ItemsInterface.singleton.getGUIIconSprite(index);
                }
            }
        }
    }
}
