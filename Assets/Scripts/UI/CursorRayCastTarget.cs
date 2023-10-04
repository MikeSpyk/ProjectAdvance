using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BasicTools
{
    namespace UI
    {
        [RequireComponent(typeof(Collider))]
        public class CursorRayCastTarget : MonoBehaviour
        {
            public event EventHandler cursorEntered;
            public event EventHandler cursorExited;
            public event EventHandler cursorStay;

            private int m_lastFrameCursorOver = 0;
            private bool m_isCursorOver = false;

            private void fireCursorEntered()
            {
                cursorEntered?.Invoke(this, null);
            }

            private void fireCursorExited()
            {
                cursorExited?.Invoke(this, null);
            }

            private void fireCursorStay()
            {
                cursorStay?.Invoke(this, null);
            }

            void LateUpdate()
            {
                if (m_isCursorOver)
                {
                    if (Time.frameCount > m_lastFrameCursorOver)
                    {
                        fireCursorExited();
                        m_isCursorOver = false;
                    }
                }
            }

            public void onCursorOver()
            {
                if (m_isCursorOver == false)
                {
                    fireCursorEntered();
                }

                m_isCursorOver = true;
                m_lastFrameCursorOver = Time.frameCount;
                fireCursorStay();
            }
        }
    }
}
