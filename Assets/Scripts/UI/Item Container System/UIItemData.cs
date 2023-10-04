using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BasicTools
{
    namespace UI
    {
        namespace Inventory
        {
            public class UIItemData : MonoBehaviour
            {
                public int itemIndex
                {
                    get
                    {
                        return m_itemIndex;
                    }
                }

                private int m_itemIndex = -1;
                private Dictionary<string, string> m_additionalItemData = new Dictionary<string, string>();
                public Dictionary<string, string> additionalItemData
                {
                    get
                    {
                        return m_additionalItemData;
                    }
                }
                private int m_hashcode;

                public void copyFrom(UIItemData source)
                {
                    Tuple<string, string>[] additionalData = new Tuple<string, string>[source.m_additionalItemData.Count];

                    int counter = 0;

                    foreach (KeyValuePair<string, string> entry in source.m_additionalItemData)
                    {
                        additionalData[counter] = new Tuple<string, string>(entry.Key, entry.Value);
                        counter++;
                    }

                    updateItemData(source.itemIndex, additionalData);
                }

                public void updateItemData(int itemIndex, params Tuple<string, string>[] additionalData)
                {
                    m_itemIndex = itemIndex;
                    m_additionalItemData.Clear();

                    if (additionalData != null)
                    {
                        for (int i = 0; i < additionalData.Length; i++)
                        {
                            m_additionalItemData.Add(additionalData[i].Item1, additionalData[i].Item2);
                        }
                    }

                    updateHashcode();
                }

                private void updateHashcode()
                {
                    m_hashcode = Tuple.Create(m_additionalItemData, m_itemIndex).GetHashCode();
                }

                // returns null if key is not present
                public string getAdditionalData(string key)
                {
                    string result;

                    if (m_additionalItemData.TryGetValue(key, out result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }

                public override int GetHashCode()
                {
                    return m_hashcode;
                }

                public static void switchItemData(UIItemData source, UIItemData target)
                {
                    UIItemContainer sourceContainer = source.transform.parent.GetComponent<UIItemContainer>();
                    UIItemContainer targetContainer = target.transform.parent.GetComponent<UIItemContainer>();

                    int tempTargetIndex = target.m_itemIndex;
                    Dictionary<string, string> tempAdditionalItemData = target.m_additionalItemData;

                    target.m_itemIndex = source.m_itemIndex;
                    target.m_additionalItemData = source.m_additionalItemData;

                    source.m_itemIndex = tempTargetIndex;
                    source.m_additionalItemData = tempAdditionalItemData;

                    target.updateHashcode();
                    source.updateHashcode();

                    sourceContainer.OnUIItemContainerChanged(new UIItemContainerChangedEventArgs() { m_changedSlot = source.gameObject, m_oldItemIndex = target.itemIndex, m_newItemIndex = source.m_itemIndex });
                    targetContainer.OnUIItemContainerChanged(new UIItemContainerChangedEventArgs() { m_changedSlot = target.gameObject, m_newItemIndex = target.itemIndex, m_oldItemIndex = source.m_itemIndex });
                }
            }
        }
    }
}
