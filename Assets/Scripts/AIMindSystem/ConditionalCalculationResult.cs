using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIMindSystem
{
    public class ConditionalCalculationResult
    {
        public ConditionalCalculationResult(float urgency, Dictionary<string, object> data)
        {
            m_urgency = urgency;
            m_data = data;
        }

        private float m_urgency;
        public float urgency{get{return m_urgency;}}
        private readonly Dictionary<string,object> m_data;
        public Dictionary<string,object> data{get{return m_data;}}
    }
}
