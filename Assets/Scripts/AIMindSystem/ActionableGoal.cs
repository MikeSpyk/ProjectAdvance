using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AIMindSystem
{
    public enum Action
                    {
                        FindFoodSource, FindWaterSource, GoToPosition, PickUpWater, PickUpFood, FindSafeLocation, Attack,
                        StandStill, FindRelaxLocation, FindTeacher, ObserveTeacherMinimumDistance, FindReproducer, Reproduce,
                        Follow, FindLeader, FindEnemyTown, PatrolInCircle, FindRandomPositionVillagerRelax, ActionQueueBack4Steps,
                        Wait10s, StoreTime_Time
                    }

    public class ActionableGoal : Goal_Base
    {
        public ActionableGoal(string name, Func<Goal_Base, float> urgencyCalculating, AIMindSystem.Action[] actionQueue)
        {
            m_name = name;
            m_urgencyCalculation = urgencyCalculating;
            m_actionQueue = actionQueue;
        }

        private Func<Goal_Base, float> m_urgencyCalculation;
        private float m_urgency = 0f;
        public override float getUrgency()
        {
            return m_urgency;
        }
        private string m_name;
        public override string getName(){return m_name;}

        private readonly AIMindSystem.Action[] m_actionQueue;
        public AIMindSystem.Action[] actionQueue
        {
            get
            {
                return m_actionQueue;
            }
        }
        private Goal_Base m_parent;
        public override void setParent(Goal_Base parent)
        {
            m_parent = parent;
        }
        public override Goal_Base getParent()
        {
            return m_parent;
        }

        public override float calculateUrgency()
        {
            m_urgency = m_urgencyCalculation(this);
            return m_urgency;
        }

        public void setUrgencyToZero() // ConditionalParentGoal may reset urgency because only of its children can be active at once (the rest is 0)
        {
            m_urgency = 0f;
        }

    }
}
