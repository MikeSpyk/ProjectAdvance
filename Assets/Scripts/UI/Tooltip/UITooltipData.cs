using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace UI
    {
        namespace Tooltip
        {
            public class UITooltipData : MonoBehaviour
            {
                private TextData[] m_textData = null;
                private int m_hashCode = -1;

                public TextData[] textData
                {
                    set
                    {
                        m_textData = value;
                        calculateHashCode();
                    }
                    get
                    {
                        return m_textData;
                    }
                }

                private void calculateHashCode()
                {
                    float tempHashCode = 0f;

                    for (int i = 0; i < m_textData.Length; i++)
                    {
                        if (m_textData[i].m_leftAlignedText != null)
                        {
                            tempHashCode += ((float)m_textData[i].m_leftAlignedText.GetHashCode()) / (m_textData.Length * 2);
                        }
                        if (m_textData[i].m_rightAlignedText != null)
                        {
                            tempHashCode += ((float)m_textData[i].m_rightAlignedText.GetHashCode()) / (m_textData.Length * 2);
                        }
                    }

                    m_hashCode = (int)tempHashCode;
                }

                public override int GetHashCode()
                {
                    return m_hashCode;
                }
            }
        }
    }
}