using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomItemAttributeMinMax
{
    public RandomItemAttributeMinMax(string name, float chance, float minValue, float maxValue)
    {
        m_name = name;
        m_chance = chance;
        m_minValue = minValue;
        m_maxValue = maxValue;
    }

    public string m_name;
    public float m_chance;
    public float m_minValue;
    public float m_maxValue;
}
