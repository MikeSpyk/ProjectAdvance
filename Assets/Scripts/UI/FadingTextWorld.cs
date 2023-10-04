using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadingTextWorld : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 1f;
    [SerializeField] private float m_lifeTime = 5f;

    public Transform m_parent;
    private TextMeshPro m_textMesh;
    private float m_endTime = 0f;
    private float m_startTime;
    private Vector3 m_offset = Vector3.zero;


    void Awake()
    {
        m_textMesh = GetComponent<TextMeshPro>();
    }

    void Start()
    {
        m_startTime = Time.time;
        m_endTime = m_startTime + m_lifeTime;
        transform.rotation = Quaternion.LookRotation( Camera.main.transform.forward);

        if(m_parent != null)
        {
            m_offset = transform.position - m_parent.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > m_endTime)
        {
            Destroy(this.gameObject);
        }
        else
        {
            float timeDelta = m_endTime - m_startTime;
            float deltaRelativ = (Time.time - m_startTime) / timeDelta;

            m_textMesh.color = new Color(m_textMesh.color.r,m_textMesh.color.g,m_textMesh.color.b,1f- deltaRelativ);
            moveText();
        }
    }

    private void moveText()
    {
        if(m_parent == null)
        {
            transform.position += Vector3.up * m_moveSpeed * Time.deltaTime;
        }
        else
        {
            m_offset += Vector3.up * m_moveSpeed * Time.deltaTime;
            transform.position = m_parent.position + m_offset;
        }
    }

    public void setText(string text)
    {
        m_textMesh.text = text;
    }

    public void setColor(Color color)
    {
        m_textMesh.color = color;
    }
}
