using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BasicTools.UI.Tooltip;
public class UIBuilder : MonoBehaviour
{
    public enum ControlType{Text, Slider}

    public class UIBuilderRowData
    {
        public string identifier = "Generic Control";
        public ControlType[] controlsType = null;
        public string[] controlsContent = null;
        public FontStyle fontStyle = FontStyle.Normal;
        public TextData[] tooltipData = null;
    }

    [SerializeField] private GameObject m_textPrefab;
    [SerializeField] private GameObject m_sliderPrefab;
    [SerializeField] private Vector2 m_border;

    private Dictionary<string,Tuple<ControlType, GameObject>[]> m_identifier_storedControls = new Dictionary<string, Tuple<ControlType, GameObject>[]>();

    public void updateRowChildContent(string identifier, int index, string content)
    {
        Tuple<ControlType, GameObject> control = m_identifier_storedControls[identifier][index];

        switch(control.Item1)
        {
            case ControlType.Slider:
            {
                string[] silderData = content.Split(';'); // 0: sliderValue, 1: sliderMaxValue 2: Width
                Slider slider = control.Item2.GetComponent<Slider>();
                slider.value = float.Parse(silderData[0]) / float.Parse(silderData[1]);
                
                Text silderText = slider.GetComponentInChildren<Text>();
                silderText.text = String.Format("{0}/{1}",silderData[0],silderData[1]);
                
                if(silderData.Length > 2)
                {
                    RectTransform rect = (slider.gameObject.transform as RectTransform);
                    rect.sizeDelta = new Vector2(float.Parse(silderData[2]),rect.sizeDelta.y);
                }

                break;
            }
            case ControlType.Text:
            {
                string[] controlData = content.Split(';'); // 0: string-text, 1: alignment

                Text text = control.Item2.GetComponent<Text>();
                text.text = controlData[0];

                if(controlData.Length > 1)
                {
                    text.alignment = (TextAnchor) Enum.Parse(typeof(TextAnchor), controlData[1]);
                }

                break;
            }
        }
    }

    public void updateRowTooltip(string identifier, TextData[] tooltipData)
    {
        Tuple<ControlType,GameObject>[] rowData = m_identifier_storedControls[identifier];

        for(int i = 0; i < rowData.Length; i++)
        {
            updateTooltipData(rowData[i].Item1,rowData[i].Item2,tooltipData);
        }
    }

    public void clear()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private static void updateTooltipData(ControlType type, GameObject obj, TextData[] tooltipData)
    {
        UITooltipData data = null;

        switch(type)
        {
            case ControlType.Text:
            {
                data = obj.GetComponent<UITooltipData>();
                if(data == null)
                {
                    data = obj.AddComponent<UITooltipData>();
                }
                break;
            }
            case ControlType.Slider:
            {
                data = obj.GetComponentInChildren<UITooltipData>();
                if(data == null)
                {
                    data = obj.GetComponentInChildren<Image>().gameObject.AddComponent<UITooltipData>();
                }
                break;
            }
            default:
            {
                Debug.LogError("unkown ControlType: " + type.ToString());
                return;
            }
        }

        data.textData = tooltipData;
    }

    public void buildUI(params UIBuilderRowData[] inputData)
    {
        m_identifier_storedControls.Clear();

        float allRowsHeightSum = 0f;

        List<RectTransform[]> uiRows = new List<RectTransform[]>();
        List<float> uiRowsMaxWidth = new List<float>();

        for(int i = inputData.Length-1; i > -1; i--)
        {
            float rowHeight = 0f;
            float rowWidth = 0f;

            RectTransform[] currentRowRects = new RectTransform[inputData[i].controlsType.Length];
            Tuple<ControlType, GameObject>[] storedControls = new Tuple<ControlType, GameObject>[inputData[i].controlsType.Length];

            for(int j = 0; j < inputData[i].controlsType.Length; j++)
            {
                GameObject newUiGameObject = null;

                switch(inputData[i].controlsType[j])
                {
                    case ControlType.Text:
                    {
                        Text newControl = Instantiate(m_textPrefab, Vector3.zero, Quaternion.identity, gameObject.transform).GetComponent<Text>();
                        storedControls[j] = new Tuple<ControlType, GameObject>(inputData[i].controlsType[j], newControl.gameObject);
                        newUiGameObject = newControl.gameObject;

                        newControl.alignment = TextAnchor.MiddleLeft;
                        newControl.fontStyle = inputData[i].fontStyle;

                        string[] controlData = inputData[i].controlsContent[j].Split(';'); // 0: string-text, 1: alignment

                        if(controlData.Length > 1)
                        {
                            newControl.alignment = (TextAnchor) Enum.Parse(typeof(TextAnchor), controlData[1]);
                        }

                        newControl.text = controlData[0];
                        newControl.rectTransform.sizeDelta = new Vector2(newControl.preferredWidth, newControl.preferredHeight);

                        rowHeight = Mathf.Max(rowHeight, newControl.preferredHeight);
                        rowWidth += newControl.preferredWidth;

                        if(uiRowsMaxWidth.Count > j)
                        {
                            uiRowsMaxWidth[j] = Mathf.Max(uiRowsMaxWidth[j],newControl.preferredWidth);
                        }
                        else
                        {
                            uiRowsMaxWidth.Add(newControl.preferredWidth);
                        }

                        currentRowRects[j] = newControl.rectTransform;
                        break;
                    }
                    case ControlType.Slider:
                    {
                        Slider newControl = Instantiate(m_sliderPrefab, Vector3.zero, Quaternion.identity, gameObject.transform).GetComponent<Slider>();
                        storedControls[j] = new Tuple<ControlType, GameObject>(inputData[i].controlsType[j], newControl.gameObject);
                        
                        newUiGameObject = newControl.gameObject;

                        string[] silderData = inputData[i].controlsContent[j].Split(';'); // 0: sliderValue, 1: sliderMaxValue 2: Width
                        newControl.value = float.Parse(silderData[0]) / float.Parse(silderData[1]);
                        float silderWidth = float.Parse(silderData[2]);

                        Text silderText = newControl.GetComponentInChildren<Text>();
                        silderText.text = String.Format("{0}/{1}",silderData[0],silderData[1]);

                        (newControl.transform as RectTransform).sizeDelta = new Vector2(silderWidth, rowHeight);

                        rowWidth += silderWidth;

                        if(uiRowsMaxWidth.Count > j)
                        {
                            uiRowsMaxWidth[j] = Mathf.Max(uiRowsMaxWidth[j],silderWidth);
                        }
                        else
                        {
                            uiRowsMaxWidth.Add(silderWidth);
                        }

                        currentRowRects[j] = newControl.transform as RectTransform;
                        break;
                    }
                    default:
                    {
                        Debug.LogWarning("unkown controlType:" + inputData[i].controlsType[j]);
                        break;
                    }
                }

                if(newUiGameObject != null)
                {
                    if(inputData[i].tooltipData != null)
                    {
                        updateTooltipData(inputData[i].controlsType[j], newUiGameObject, inputData[i].tooltipData);
                    }
                }
            }

            if(!inputData[i].identifier.Equals("Generic Control"))
            {
                m_identifier_storedControls.Add(inputData[i].identifier, storedControls);
            }

            uiRows.Add(currentRowRects);
            allRowsHeightSum += rowHeight;
        }

        float maxRowWidth = 0f;

        for(int i = 0; i < uiRowsMaxWidth.Count; i++)
        {
            maxRowWidth += uiRowsMaxWidth[i];
        }

        RectTransform parentRect = gameObject.transform as RectTransform;
        parentRect.sizeDelta = new Vector2(maxRowWidth,allRowsHeightSum) + m_border * 2;

        float heightCounter = 0f;

        for(int i = 0; i < uiRows.Count; i++)
        {
            float rowHeightCounter = 0f;

            for(int j = 0; j < uiRows[i].Length; j++)
            {
                RectTransform rowControlRect = uiRows[i][j];

                if(rowControlRect != null)
                {
                    float xOffset = 0f;

                    for(int k = 0; k < j; k++)
                    {
                        xOffset += uiRowsMaxWidth[k];
                    }

                    rowControlRect.sizeDelta = new Vector2(uiRowsMaxWidth[j],rowControlRect.sizeDelta.y);
                    rowControlRect.localPosition = new Vector3(
                                                                xOffset - maxRowWidth/2 + rowControlRect.sizeDelta.x/2,
                                                                heightCounter - allRowsHeightSum/2 + rowControlRect.sizeDelta.y/2,
                                                                0f);

                    if(rowHeightCounter == 0f)
                    {
                        rowHeightCounter += rowControlRect.sizeDelta.y;
                    }
                }
            }

            heightCounter += rowHeightCounter;
        }
    }
}
