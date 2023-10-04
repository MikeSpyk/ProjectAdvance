using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace UI
    {
        namespace Inventory
        {
            public class UISlotData : MonoBehaviour
            {
                [SerializeField] public StorableItemTemplate.ItemType[] m_allowedItemTypes;

                public bool isItemTypeAllowed(StorableItemTemplate.ItemType itemType)
                {
                    for (int i = 0; i < m_allowedItemTypes.Length; i++)
                    {
                        if (m_allowedItemTypes[i] == itemType)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public bool isItemAllowed(UIItemData item)
                {
                    return isItemTypeAllowed(ItemsInterface.singleton.getItemType(item.itemIndex));
                }
            }
        }
    }
}
