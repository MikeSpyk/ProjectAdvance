using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTextManager : MonoBehaviour
{
    private static WorldTextManager singleton;
    public static WorldTextManager Singleton
    {
        get
        {
            return singleton;
        }
    }

    [SerializeField] private GameObject m_textPrefab;

    void Awake()
    {
        singleton = this;
    }

    public void showText(string text, Vector3 position, Transform parent, Color color)
    {
        FadingTextWorld textObject = Instantiate(m_textPrefab, position, Quaternion.identity).GetComponent<FadingTextWorld>();
        textObject.setText(text);
        textObject.setColor(color);
        textObject.m_parent = parent;
    }
}
