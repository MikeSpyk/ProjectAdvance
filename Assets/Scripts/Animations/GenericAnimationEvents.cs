using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace Animations
    {
        public class GenericAnimationEvents : StateMachineBehaviour
        {
            [SerializeField] private string[] m_eventNames;
            [SerializeField, Range(0f, 1f)] private float[] m_eventTimes; // normalized: 0.0 ... 1.0
            [SerializeField] private bool[] m_eventIsFired;
            private AnimationEventReceiver m_animationEventReceiver = null;
            private int m_lastIntegerTimeUpdate = 0;

            override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                m_lastIntegerTimeUpdate = 0;
                resetAllEvents();
            }

            override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                if ((int)stateInfo.normalizedTime > m_lastIntegerTimeUpdate)
                {
                    m_lastIntegerTimeUpdate = (int)stateInfo.normalizedTime;
                    fireAllUnfiredEvents(animator);
                    resetAllEvents();
                }

                float normalizedTimeFixed = stateInfo.normalizedTime - m_lastIntegerTimeUpdate;

                for (int i = 0; i < m_eventNames.Length; i++)
                {
                    if (!m_eventIsFired[i])
                    {
                        if (normalizedTimeFixed > m_eventTimes[i])
                        {
                            fireEvent(m_eventNames[i], animator);
                            m_eventIsFired[i] = true;
                        }
                    }
                }
            }

            override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                //fireAllUnfiredEvents(animator);
            }

            private void fireAllUnfiredEvents(Animator animator)
            {
                for (int i = 0; i < m_eventNames.Length; i++)
                {
                    if (!m_eventIsFired[i])
                    {
                        fireEvent(m_eventNames[i], animator);
                    }
                }
            }

            private void resetAllEvents()
            {
                for (int i = 0; i < m_eventIsFired.Length; i++)
                {
                    m_eventIsFired[i] = false;
                }
            }

            private void fireEvent(string eventName, Animator animator)
            {
                if (m_animationEventReceiver == null)
                {
                    m_animationEventReceiver = animator.GetComponent<AnimationEventReceiver>();

                    if (m_animationEventReceiver == null)
                    {
                        Debug.LogError("no AnimationEventReceiver-component on receiving Gameobject with animator. Add AnimationEventReceiver to GameObject \"" + animator.gameObject.name + "\"! (event: " + eventName + ")");
                        return;
                    }
                }

                m_animationEventReceiver.fireAnimationEvent(eventName);
            }
        }
    }
}
