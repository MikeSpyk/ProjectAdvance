using UnityEngine;
using System;

namespace BasicTools
{
    namespace Animations
    {
        /// <summary>
        /// receives Events form GenericAnimationEvents-class
        /// </summary>
        [RequireComponent(typeof(Animator))]
        public class AnimationEventReceiver : MonoBehaviour
        {
            public event EventHandler<string> AnimationEventFired;
            public void fireAnimationEvent(string eventName)
            {
                AnimationEventFired?.Invoke(this, eventName);
            }
        }
    }
}
