using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace BasicTools
{
    namespace UI
    {
        namespace Inventory
        {
            public class UIItemContainerChangedEventArgs : EventArgs
            {
                public GameObject m_changedSlot;
                public int m_changedSlotIndex;
                public int m_oldItemIndex;
                public int m_newItemIndex;
            }

            // creates an grid of inventory slots in the UI
            public class UIItemContainer : MonoBehaviour
            {
                [SerializeField] private GameObject m_itemSlotUIControlPrefab;
                [SerializeField] private GameObject m_itemSlotImagePrefab; // mark this as not raycast target!
                [SerializeField] private int m_sizeX = 2;
                [SerializeField] private int m_sizeY = 2;
                [SerializeField] private float m_slotOffsetX = 1f;
                [SerializeField] private float m_slotOffsetY = 1f;
                [SerializeField] private float m_gridOffsetX = 0f;
                [SerializeField] private float m_gridOffsetY = 0f;
                [SerializeField] private bool m_createAtStart = true;
                [Header("allowed Item Types")]
                [SerializeField] private bool m_allItemsAreAllowedAllSlots = true;
                [SerializeField] private StorableItemTemplate.ItemType[] m_allowedItemsAllFields;
                [SerializeField] private StorableItemTemplate.ItemType[] m_allowedItemsForFields;
                [Header("Debug")]
                [SerializeField] private bool m_debug_spawnItem = false;
                [SerializeField] private int m_debug_spawnItemID = 0;

                public event EventHandler<UIItemContainerChangedEventArgs> UIItemChanged;

                private Dictionary<GameObject, int> m_slot_slotIndex = new Dictionary<GameObject, int>();
                private Dictionary<int, int> m_itemID_itemCount = new Dictionary<int, int>();

                void Awake()
                {
                    if (m_createAtStart)
                    {
                        createItemGridUI();
                    }
                }

                void Update()
                {
                    if (m_debug_spawnItem)
                    {
                        tryAddItemNextFreeSlot(m_debug_spawnItemID);
                        m_debug_spawnItem = false;
                    }
                }

                public System.Tuple<int, Dictionary<string, string>>[] getItemsData()
                {
                    System.Tuple<int, Dictionary<string, string>>[] result = new System.Tuple<int, Dictionary<string, string>>[m_slot_slotIndex.Count];

                    int counter = 0;

                    foreach (KeyValuePair<GameObject, int> pair in m_slot_slotIndex)
                    {
                        UIItemData data = pair.Key.GetComponent<UIItemData>();
                        Dictionary<string, string> dictCopy = new Dictionary<string, string>();

                        foreach (KeyValuePair<string, string> pair2 in data.additionalItemData)
                        {
                            dictCopy.Add(pair2.Key, pair2.Value);
                        }

                        result[counter] = new Tuple<int, Dictionary<string, string>>(data.itemIndex, dictCopy);

                        counter++;
                    }

                    return result;
                }

                private void createItemGridUI()
                {
                    foreach (KeyValuePair<GameObject, int> pair in m_slot_slotIndex)
                    {
                        pair.Key.transform.SetParent(null); // remove from this parent so that it will not be found by getChild (because there is no way to know when Destroy will actualy remove the object)
                        Destroy(pair.Key);
                    }
                    m_slot_slotIndex.Clear();

                    RectTransform rect = m_itemSlotUIControlPrefab.GetComponent<RectTransform>();
                    Vector2 itemSlotSize = rect.sizeDelta;

                    Vector2 offset = new Vector2(-(itemSlotSize.x + m_slotOffsetX) * m_sizeX / 2, -(itemSlotSize.y + m_slotOffsetY) * m_sizeY / 2)
                                      + new Vector2((itemSlotSize.x + m_slotOffsetX) * 0.5f, (itemSlotSize.y + m_slotOffsetY) * 0.5f);

                    Vector3 gridOffset = new Vector3(m_gridOffsetX, m_gridOffsetY, 0f);

                    StorableItemTemplate.ItemType[] allowedItemTypes;

                    if (m_allItemsAreAllowedAllSlots)
                    {
                        allowedItemTypes = Enum.GetValues(typeof(StorableItemTemplate.ItemType)) as StorableItemTemplate.ItemType[];
                    }
                    else
                    {
                        allowedItemTypes = m_allowedItemsAllFields;
                    }

                    bool idividualSlotItemTypes = m_allowedItemsForFields != null && m_allowedItemsForFields.Length > 0;

                    for (int i = 0; i < m_sizeY; i++)
                    {
                        for (int j = 0; j < m_sizeX; j++)
                        {
                            GameObject tempItemSlot = Instantiate(m_itemSlotUIControlPrefab, this.gameObject.transform);

                            UISlotData slotData = tempItemSlot.GetComponent<UISlotData>();
                            if (idividualSlotItemTypes)
                            {
                                slotData.m_allowedItemTypes = new StorableItemTemplate.ItemType[] { m_allowedItemsForFields[i] };
                            }
                            else
                            {
                                slotData.m_allowedItemTypes = allowedItemTypes.Clone() as StorableItemTemplate.ItemType[];
                            }

                            RectTransform tempRect = tempItemSlot.transform as RectTransform;
                            tempItemSlot.name = string.Format("UIItemSlot:{0}:{1}", j, i) + ";" + Time.frameCount + ";" + System.DateTime.Now.Ticks;
                            tempRect.localPosition = new Vector3(offset.x + j * (itemSlotSize.x + m_slotOffsetX), offset.y + i * (itemSlotSize.y + m_slotOffsetY), 0) + gridOffset;

                            m_slot_slotIndex.Add(tempItemSlot, j + i * m_sizeX);
                        }
                    }
                }

                public void setItems(System.Tuple<int, Dictionary<string, string>>[] items_index_additionalData)
                {
                    int childOffset = 0;

                    for (int i = 0; i < items_index_additionalData.Length; i++)
                    {
                        UIItemData itemdata = gameObject.transform.GetChild(i + childOffset).GetComponent<UIItemData>();
                        if (itemdata != null)
                        {
                            if (items_index_additionalData[i].Item1 < 0)
                            {
                                continue;
                            }

                            Tuple<string, string>[] additionalParams = null;

                            if (items_index_additionalData[i].Item2 != null && items_index_additionalData[i].Item2.Count > 0)
                            {
                                additionalParams = new Tuple<string, string>[items_index_additionalData[i].Item2.Count];

                                int paramCounter = 0;
                                foreach (KeyValuePair<string, string> pair in items_index_additionalData[i].Item2)
                                {
                                    additionalParams[paramCounter] = new Tuple<string, string>(pair.Key, pair.Value);
                                    paramCounter++;
                                }
                            }

                            int oldItemIndex = itemdata.itemIndex;
                            setItem(items_index_additionalData[i].Item1, additionalParams, i + childOffset);
                            OnUIItemContainerChanged(new UIItemContainerChangedEventArgs() { m_changedSlot = itemdata.gameObject, m_newItemIndex = items_index_additionalData[i].Item1, m_oldItemIndex = oldItemIndex });
                        }
                        else
                        {
                            //Debug.Log("UI Data is null: "+ gameObject.transform.GetChild(i).name + "; i="+i);
                            childOffset++; // wrong gameobject: need to look for an additional one (unexpected child)
                            i--;
                        }
                    }
                }

                private void setItem(UIItemData newItemData, int containerIndex)
                {
                    UIItemData itemdata = gameObject.transform.GetChild(containerIndex).GetComponent<UIItemData>();

                    itemdata.copyFrom(newItemData);

                    Image itemImage = null;

                    if (itemdata.transform.childCount > 0)
                    {
                        itemImage = itemdata.transform.GetChild(0).GetComponent<Image>();
                    }

                    if (itemImage == null)
                    {
                        GameObject tempImageObj = Instantiate(m_itemSlotImagePrefab, itemdata.transform);
                        itemImage = tempImageObj.GetComponent<Image>();
                    }

                    itemImage.sprite = ItemContainerManager.Singleton.getItemIcon(ItemsInterface.singleton.getGUIIconIndex(itemdata.itemIndex));
                }
                private void setItem(int itemIndex, Tuple<string, string>[] additionalParams, int containerIndex)
                {
                    //Debug.Log("setItem: " + itemIndex + " on Pos: " + containerIndex);

                    UIItemData itemdata = gameObject.transform.GetChild(containerIndex).GetComponent<UIItemData>();

                    itemdata.updateItemData(itemIndex, additionalParams);

                    Image itemImage = null;

                    if (itemdata.transform.childCount > 0)
                    {
                        itemImage = itemdata.transform.GetChild(0).GetComponent<Image>();
                    }

                    if (itemImage == null)
                    {
                        GameObject tempImageObj = Instantiate(m_itemSlotImagePrefab, itemdata.transform);
                        itemImage = tempImageObj.GetComponent<Image>();
                    }

                    if (itemIndex > -1)
                    {
                        itemImage.sprite = ItemContainerManager.Singleton.getItemIcon(ItemsInterface.singleton.getGUIIconIndex(itemdata.itemIndex));
                        //Debug.Log("itemImage: " +itemImage.gameObject.name);
                    }
                }

                public void updateLayout(int sizeX, int sizeY)
                {
                    m_sizeX = sizeX;
                    m_sizeY = sizeY;

                    createItemGridUI();
                }

                public bool tryAddItemNextFreeSlot(int itemIndex, float randomAttributesSeed = 1f)
                {
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        UIItemData itemdata = gameObject.transform.GetChild(i).GetComponent<UIItemData>();

                        if (itemdata != null)
                        {
                            if (itemdata.itemIndex == -1) // -1 = empty
                            {
                                setItem(itemIndex, ItemsInterface.singleton.rollRandomAttributes(itemIndex, Time.time + randomAttributesSeed, Time.deltaTime).ToArray(), i);
                                OnUIItemContainerChanged(new UIItemContainerChangedEventArgs() { m_changedSlot = itemdata.gameObject, m_newItemIndex = itemIndex, m_oldItemIndex = -1 });
                                return true;
                            }
                        }
                    }

                    return false;
                }
                public bool tryAddItemNextFreeSlot(UIItemData newItemData)
                {
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        UIItemData itemdata = gameObject.transform.GetChild(i).GetComponent<UIItemData>();

                        if (itemdata != null)
                        {
                            if (itemdata.itemIndex == -1) // -1 = empty
                            {
                                setItem(newItemData, i);
                                OnUIItemContainerChanged(new UIItemContainerChangedEventArgs() { m_changedSlot = itemdata.gameObject, m_newItemIndex = newItemData.itemIndex, m_oldItemIndex = -1 });
                                return true;
                            }
                        }
                    }

                    return false;
                }

                public UIItemData getFirstFreeItemSlot()
                {
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        UIItemData itemdata = gameObject.transform.GetChild(i).GetComponent<UIItemData>();

                        if (itemdata != null)
                        {
                            if (itemdata.itemIndex == -1) // -1 = empty
                            {
                                return itemdata;
                            }
                        }
                    }

                    return null;
                }

                public void OnUIItemContainerChanged(UIItemContainerChangedEventArgs args)
                {
                    changeItemCount(args.m_newItemIndex, 1);
                    changeItemCount(args.m_oldItemIndex, -1);

                    /*string debugMessage = "OnUIItemContainerChanged:"+gameObject.name +"; ";

                    debugMessage += "args.m_changedSlot.name: " + args.m_changedSlot.name + "; ";
                    foreach(KeyValuePair<GameObject,int> pair in m_slot_slotIndex)
                    {
                        if(pair.Key.name.Equals(args.m_changedSlot.name))
                        {
                            debugMessage += "found: "+args.m_changedSlot.name + ", hash:" + args.m_changedSlot.GetHashCode() + ";"+pair.Key.GetHashCode() + "; ";
                        }

                        Debug.Log(pair.Key.name);
                    }

                    Debug.Log(debugMessage);*/

                    args.m_changedSlotIndex = m_slot_slotIndex[args.m_changedSlot];
                    UIItemChanged?.Invoke(this, args);
                }

                public int getItemCount(int itemIndex)
                {
                    if (m_itemID_itemCount.ContainsKey(itemIndex))
                    {
                        return m_itemID_itemCount[itemIndex];
                    }
                    else
                    {
                        return 0;
                    }
                }

                public bool removeItems(int itemIndex, int count)
                {
                    //Debug.Log("removeItems: " + itemIndex);

                    int removeCounter = 0;

                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        UIItemData itemdata = gameObject.transform.GetChild(i).GetComponent<UIItemData>();

                        if (itemdata != null)
                        {
                            if (itemdata.itemIndex == itemIndex)
                            {
                                itemdata.updateItemData(-1, null);  // -1 = empty

                                if (itemdata.transform.childCount > 0)
                                {
                                    Image itemImage = itemdata.transform.GetChild(0).GetComponent<Image>();
                                    if (itemImage != null)
                                    {
                                        Destroy(itemImage.gameObject);
                                    }
                                }

                                OnUIItemContainerChanged(new UIItemContainerChangedEventArgs() { m_changedSlot = itemdata.gameObject, m_newItemIndex = -1, m_oldItemIndex = itemIndex });
                                removeCounter++;
                            }
                        }

                        if (removeCounter >= count)
                        {
                            return true;
                        }
                    }

                    Debug.LogWarning("could not find " + count + " of item " + itemIndex + ". removed " + removeCounter + " instead");

                    return false;
                }

                public void clear()
                {
                    createItemGridUI();
                }

                private void changeItemCount(int itemIndex, int toAdd)
                {
                    if (m_itemID_itemCount.ContainsKey(itemIndex))
                    {
                        m_itemID_itemCount[itemIndex] += toAdd;
                    }
                    else
                    {
                        m_itemID_itemCount.Add(itemIndex, toAdd);
                    }
                }
            }
        }
    }
}
