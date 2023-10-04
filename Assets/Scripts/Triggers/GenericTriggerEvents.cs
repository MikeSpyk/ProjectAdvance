using UnityEngine;
using System;

namespace BasicTools
{
    namespace Triggers
    {
        [RequireComponent(typeof(Collider))]
        public class GenericTriggerEvents : MonoBehaviour
        {
            public event EventHandler<Collider> triggerEntered;
            public event EventHandler<Collider> triggerExit;

            private void fireTriggerEntered(Collider collider)
            {
                triggerEntered?.Invoke(this, collider);
            }

            private void fireTriggerExited(Collider collider)
            {
                triggerExit?.Invoke(this, collider);
            }

            private void OnTriggerEnter(Collider collider)
            {
                fireTriggerEntered(collider);
            }

            private void OnTriggerExit(Collider collider)
            {
                fireTriggerExited(collider);
            }
        }
    }
}
