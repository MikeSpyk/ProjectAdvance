using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    public static class HierarchyTools
    {
        public static GameObject findGameObjectWithNameInChildren(GameObject parentObj, string nameToFind)
        {
            if (parentObj.name.Equals(nameToFind))
            {
                return parentObj;
            }

            for (int i = 0; i < parentObj.transform.childCount; i++)
            {
                GameObject result = findGameObjectWithNameInChildren(parentObj.transform.GetChild(i).gameObject, nameToFind);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
