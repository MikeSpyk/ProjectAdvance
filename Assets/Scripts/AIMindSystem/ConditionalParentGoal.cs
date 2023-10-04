using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AIMindSystem
{
    public class ConditionalParentGoal : Goal // exclusive OR goals
    {
        public ConditionalParentGoal(string name,  Func<Goal_Base, ConditionalCalculationResult> urgencyCalculating, Goal_Base[] children) : base(name, children)
        {
            m_urgencyCalculation = urgencyCalculating;
        }

        private Func<Goal_Base, ConditionalCalculationResult> m_urgencyCalculation;
        private Goal_Base m_parent;
        public override void setParent(Goal_Base parent)
        {
            m_parent = parent;
        }
        public override Goal_Base getParent()
        {
            return m_parent;
        }
        private ConditionalCalculationResult m_lastCalculationResult = null;
        public ConditionalCalculationResult lastCalculationResult{get{return m_lastCalculationResult;}}

        public override float calculateUrgency()
        {
            m_lastCalculationResult = m_urgencyCalculation(this);
            m_urgency = m_lastCalculationResult.urgency;

            setAllChildrenUrgencyZero(); // not really needed for function but for clearer view in output-data

            return m_urgency;
        }

        public override ActionableGoal GetMostUrgentActionableGoal()
        {
            calculateUrgencyChildren();

            if(m_mostUrgentChild.GetType() == typeof(ActionableGoal))
            {
                return m_mostUrgentChild as ActionableGoal;
            }
            else
            {
                throw new NotSupportedException("ConditionalParentGoal-Child is type of " + m_mostUrgentChild.GetType().Name + ". But only Type \"ActionableGoal\" is allowed " + "(" +getName()+ ")");
            }
        }

        private void setAllChildrenUrgencyZero()
        {
            for(int i = 0; i < m_children.Length; i++)
            {
                ActionableGoal childAsActionalbeGoal = m_children[i] as ActionableGoal;

                if(childAsActionalbeGoal == null)
                {
                    throw new NotSupportedException("ConditionalParentGoal-Child is type of " + m_children[i].GetType().Name + ". But only Type \"ActionableGoal\" is allowed "+ "(" +getName()+ ")");
                }
                else
                {
                    childAsActionalbeGoal.setUrgencyToZero();
                }
            }
        }

        private float calculateUrgencyChildren() //returns urgency of child with biggest urgency
        {
            /* 
            warning:    urgency of children is not equal to urgency of the ConditionalParent. ConditionalParent may only change its urgency by calculateUrgency().
                        urgency of children is only used to compare children
            */
            float maxChildUrgency = float.MinValue;

            for(int i = 0; i < m_children.Length; i++)
            {
                if(m_children[i].calculateUrgency() > maxChildUrgency)
                {
                    m_mostUrgentChild = m_children[i];
                    maxChildUrgency = m_children[i].getUrgency();
                }
            }

            for(int i = 0; i < m_children.Length; i++) // is this really necessary since it is an exclusiv OR decision (there will be only 1 child with a urgency != 0)
            {
                if(m_children[i] != m_mostUrgentChild)
                {
                    ActionableGoal childAsActionalbeGoal = m_children[i] as ActionableGoal;

                    if(childAsActionalbeGoal == null)
                    {
                        throw new NotSupportedException("ConditionalParentGoal-Child is type of " + m_children[i].GetType().Name + ". But only Type \"ActionableGoal\" is allowed "+ "(" +getName()+ ")");
                    }
                    else
                    {
                        childAsActionalbeGoal.setUrgencyToZero();
                    }
                }
            }

            return m_urgency;
        }

    }
}
