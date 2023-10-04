using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace UI
    {
        namespace Tooltip
        {
            public class SimpleTextTooltipData : MonoBehaviour
            {
                [SerializeField] private string m_text;

                void Start()
                {
                    UITooltipData data = gameObject.AddComponent<UITooltipData>();
                    data.textData = new TextData[] { new TextData() { m_leftAlignedText = m_text } };
                    Destroy(this);
                }
            }
        }
    }
}
