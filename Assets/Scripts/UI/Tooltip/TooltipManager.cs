using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BasicTools
{
    namespace UI
    {
        namespace Tooltip
        {
            public class TooltipManager : MonoBehaviour
            {
                private static TooltipManager singleton;
                public static TooltipManager Singleton
                {
                    get
                    {
                        return singleton;
                    }
                }

                [SerializeField] private GameObject m_tooltipPanel;
                [SerializeField] private Vector2 m_tooltipOffset;
                [SerializeField] private GameObject m_textPrefab;
                [SerializeField] private Vector2 m_borderOffset;

                private int m_lastFrameActivateTooltip = 0;
                private List<Text> m_tooltipChildren = new List<Text>();
                private Queue<Text> m_textCache = new Queue<Text>();
                private int m_latestDataHash = 0;

                private void Awake()
                {
                    singleton = this;
                }

                private void Start()
                {
                    m_tooltipPanel.SetActive(false);
                }

                private void LateUpdate()
                {
                    updateTooltip();
                }

                private void updateTooltip()
                {
                    if (m_tooltipPanel.activeSelf)
                    {
                        if (m_lastFrameActivateTooltip != Time.frameCount)
                        {
                            m_tooltipPanel.SetActive(false);
                        }
                        else
                        {
                            RectTransform rect = m_tooltipPanel.transform as RectTransform;
                            rect.position = Input.mousePosition + new Vector3(rect.sizeDelta.x / 2f + m_tooltipOffset.x, -rect.sizeDelta.y / 2f + m_tooltipOffset.y, 0f);

                            UITools.adjustRectIfOutOfScreen(rect);
                        }
                    }
                }

                private Text getNewTextObject()
                {
                    if (m_textCache.Count > 0)
                    {
                        Text tempText = m_textCache.Dequeue();
                        tempText.gameObject.SetActive(true);
                        return tempText;
                    }
                    else
                    {
                        return Instantiate(m_textPrefab, m_tooltipPanel.transform.position, Quaternion.identity, m_tooltipPanel.transform).GetComponent<Text>();
                    }
                }

                public void setTooltipData(int dataHash = -1, params TextData[] textData)
                {
                    m_tooltipPanel.SetActive(true);

                    if (dataHash == -1 || m_latestDataHash != dataHash)
                    {
                        m_latestDataHash = dataHash;

                        for (int i = 0; i < m_tooltipChildren.Count; i++)
                        {
                            m_tooltipChildren[i].gameObject.SetActive(false);
                            m_textCache.Enqueue(m_tooltipChildren[i]);
                        }
                        m_tooltipChildren.Clear();

                        List<Text[]> activeRows = new List<Text[]>();

                        float panelHeight = 0f;
                        float panelWidth = 0f;

                        for (int i = textData.Length - 1; i > -1; i--)
                        {
                            float rowWidth = 0f;
                            float rowHeight = 0f;

                            Text leftAlignedText = null;

                            if (textData[i].m_leftAlignedText != null)
                            {
                                leftAlignedText = getNewTextObject();

                                leftAlignedText.verticalOverflow = VerticalWrapMode.Truncate;
                                leftAlignedText.horizontalOverflow = HorizontalWrapMode.Overflow;

                                leftAlignedText.fontStyle = textData[i].m_fontStyle;
                                leftAlignedText.color = textData[i].m_color;
                                leftAlignedText.alignment = TextAnchor.MiddleLeft;
                                leftAlignedText.text = textData[i].m_leftAlignedText;

                                leftAlignedText.rectTransform.sizeDelta = new Vector2(leftAlignedText.preferredWidth, leftAlignedText.preferredHeight);

                                rowHeight = Mathf.Max(rowHeight, leftAlignedText.rectTransform.sizeDelta.y);
                                rowWidth += leftAlignedText.rectTransform.sizeDelta.x;

                                m_tooltipChildren.Add(leftAlignedText);
                            }

                            Text rightAlignedText = null;

                            if (textData[i].m_rightAlignedText != null)
                            {
                                rightAlignedText = getNewTextObject();

                                rightAlignedText.verticalOverflow = VerticalWrapMode.Truncate;
                                rightAlignedText.horizontalOverflow = HorizontalWrapMode.Overflow;

                                rightAlignedText.fontStyle = textData[i].m_fontStyle;
                                rightAlignedText.color = textData[i].m_color;
                                rightAlignedText.alignment = TextAnchor.MiddleRight;
                                rightAlignedText.text = textData[i].m_rightAlignedText;

                                rightAlignedText.rectTransform.sizeDelta = new Vector2(rightAlignedText.preferredWidth, rightAlignedText.preferredHeight);

                                rowHeight = Mathf.Max(rowHeight, rightAlignedText.rectTransform.sizeDelta.y);
                                rowWidth += rightAlignedText.rectTransform.sizeDelta.x;

                                m_tooltipChildren.Add(rightAlignedText);
                            }

                            rowWidth += textData[i].m_blankSpaceWidth;

                            if (leftAlignedText != null)
                            {
                                leftAlignedText.rectTransform.sizeDelta = new Vector2(rowWidth, leftAlignedText.rectTransform.sizeDelta.y);
                            }

                            if (rightAlignedText != null)
                            {
                                rightAlignedText.rectTransform.sizeDelta = new Vector2(rowWidth, rightAlignedText.rectTransform.sizeDelta.y);
                            }

                            panelWidth = Mathf.Max(panelWidth, rowWidth);
                            panelHeight += rowHeight;

                            activeRows.Add(new Text[] { leftAlignedText, rightAlignedText });
                        }

                        RectTransform panelRect = m_tooltipPanel.transform as RectTransform;
                        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight) + m_borderOffset * 2;

                        float heightCounter = 0f;

                        for (int i = 0; i < activeRows.Count; i++)
                        {
                            float rowHeightCounter = 0f;

                            for (int j = 0; j < activeRows[i].Length; j++)
                            {
                                Text rowText = activeRows[i][j];

                                if (rowText != null)
                                {
                                    rowText.rectTransform.localPosition = Vector3.up * (heightCounter - panelHeight / 2 + rowText.rectTransform.sizeDelta.y / 2);
                                    rowText.rectTransform.sizeDelta = new Vector2(panelWidth, rowText.rectTransform.sizeDelta.y);

                                    if (rowHeightCounter == 0f)
                                    {
                                        rowHeightCounter += rowText.rectTransform.sizeDelta.y;
                                    }
                                }
                            }

                            heightCounter += rowHeightCounter;
                        }
                    }

                    m_lastFrameActivateTooltip = Time.frameCount;
                }
            }
        }
    }
}
