using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExpandDirectionX{Right,Left}
public enum ExpandDirectionY{Up,Down}

namespace BasicTools
{
    namespace UI
    {
        public static class UITools
        {
            // if out of border of screen: set exactaly on border
            public static void adjustRectIfOutOfScreen(RectTransform rect)
            {
                if (rect.position.x + rect.sizeDelta.x / 2 > Screen.width)
                {
                    rect.position = new Vector3(Screen.width - rect.sizeDelta.x / 2, rect.position.y, 0f);
                }
                else if (rect.position.x - rect.sizeDelta.x / 2 < 0)
                {
                    rect.position = new Vector3(rect.sizeDelta.x / 2, rect.position.y, 0f);
                }

                if (rect.position.y + rect.sizeDelta.y / 2 > Screen.height)
                {
                    rect.position = new Vector3(rect.position.x, Screen.height - rect.sizeDelta.y / 2, 0f);
                }
                else if (rect.position.y - rect.sizeDelta.y / 2 < 0)
                {
                    rect.position = new Vector3(rect.position.x, rect.sizeDelta.y / 2, 0f);
                }
            }

            public static void findDirectionToExpandUIAtMousePosition(Vector2 uISize, out ExpandDirectionX expandX, out ExpandDirectionY expandY)
            {
                Vector3 mousePosition = Input.mousePosition;

                if (mousePosition.x + uISize.x < Screen.width)
                {
                    expandX = ExpandDirectionX.Right;
                }
                else
                {
                    expandX = ExpandDirectionX.Left;
                }

                if (mousePosition.y - uISize.y > 0)
                {
                    expandY = ExpandDirectionY.Down;
                }
                else
                {
                    expandY = ExpandDirectionY.Up;
                }
            }
        }
    }
}
