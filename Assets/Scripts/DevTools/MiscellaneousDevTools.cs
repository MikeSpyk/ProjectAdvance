using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    public class MiscellaneousDevTools : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool m_sceneViewOnStart = true;

        void Start()
        {
            if (m_sceneViewOnStart)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }
        }

        void Update()
        {

        }
#endif
    }
}
