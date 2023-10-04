using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIMindSystem
{
    public class Goal : Goal_Base
    {
        public Goal(string name, Goal_Base[] children)
        {
            m_name = name;
            m_children = children;

            for(int i = 0; i < m_children.Length; i++)
            {
                m_children[i].setParent(this);
            }
        }

        protected float m_urgency = 0f;
        public override float getUrgency()
        {
            return m_urgency;
        }

        protected Goal_Base[] m_children;
        protected Goal_Base m_mostUrgentChild = null;
        private string m_name;
        public override string getName(){return m_name;}
        private Goal_Base m_parent;
        public override void setParent(Goal_Base parent)
        {
            m_parent = parent;
        }
        public override Goal_Base getParent()
        {
            return m_parent;
        }

        public override float calculateUrgency() //returns urgency of child with biggest urgency
        {
            m_urgency = float.MinValue;

            for(int i = 0; i < m_children.Length; i++)
            {
                if(m_children[i].calculateUrgency() > m_urgency)
                {
                    m_mostUrgentChild = m_children[i];
                    m_urgency = m_children[i].getUrgency();
                }
            }

            return m_urgency;
        }

        public virtual ActionableGoal GetMostUrgentActionableGoal()
        {
            if(m_mostUrgentChild.GetType() == typeof(ActionableGoal))
            {
                return m_mostUrgentChild as ActionableGoal;
            }
            else
            {
                Goal childAsGoal = m_mostUrgentChild as Goal;
                return childAsGoal.GetMostUrgentActionableGoal();
            }
        }

        // appends the lowest level of goals. Are eighter of type ConditionalParentGoal or ActionableGoal
        public void appendAllLowestLevelGoals(List<Goal_Base> providedList)
        {
            for(int i = 0; i < m_children.Length; i++)
            {
                if(m_children[i].GetType() == typeof(ActionableGoal))
                {
                    providedList.Add(m_children[i]);
                }
                else if(m_children[i].GetType() == typeof(ConditionalParentGoal))
                {
                    providedList.Add(m_children[i]);
                    (m_children[i] as ConditionalParentGoal).appendAllLowestLevelGoals(providedList);
                }
                else // Goal-class
                {
                    (m_children[i] as Goal).appendAllLowestLevelGoals(providedList);
                }
            }
        }

    }
}
