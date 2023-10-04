using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingTextList : MonoBehaviour
{
    [SerializeField] private GameObject m_printTextPrefab;
    [SerializeField] private float m_textDistance = 30f;
    [SerializeField] private float m_textVisibleTime = 5f;
    [SerializeField] private AnimationCurve m_fadeCurve;

    List<float> m_textStartTime = new List<float>();
    List<GameObject> m_textGameobjects = new List<GameObject>();
    List<Text> m_texts = new List<Text>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_textGameobjects.Count > 0)
        {
            // delete old texts
            if(Time.time > m_textStartTime[0] + m_textVisibleTime)
            {
                Destroy(m_textGameobjects[0]);
                m_textGameobjects.RemoveAt(0);
                m_textStartTime.RemoveAt(0);
                m_texts.RemoveAt(0);
            }

            // fade texts

            for(int i = 0; i < m_texts.Count; i++)
            {
                Color textColor = new Color(
                                                m_texts[i].color.r,
                                                m_texts[i].color.g,
                                                m_texts[i].color.b,
                                                1f- m_fadeCurve.Evaluate( Mathf.Min((Time.time - m_textStartTime[i])/m_textVisibleTime,1f)) );
                m_texts[i].color = textColor;
            }
        }
    }

    public void printText(string text)
    {
        GameObject textGameobject = Instantiate(m_printTextPrefab, gameObject.transform);
        Text tempText = textGameobject.GetComponent<Text>();
        tempText.text = text;

        textGameobject.transform.position = new Vector3(textGameobject.transform.position.x,m_textDistance,textGameobject.transform.position.z);

        for(int i = 0; i < m_textGameobjects.Count; i++)
        {
            m_textGameobjects[i].transform.position += new Vector3(0,m_textDistance,0);
        }

        m_textGameobjects.Add(textGameobject);
        m_textStartTime.Add(Time.time);
        m_texts.Add(tempText);
    }
}
