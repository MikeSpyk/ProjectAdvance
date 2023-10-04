using UnityEngine;

namespace BasicTools
{
    namespace Audio
    {
        /// <summary>
        /// Class for storing information of the sound assets for the soundmanager
        /// </summary>
        [System.Serializable]
        public class SoundAsset
        {
            public AudioClip m_audioClip;
            public float m_volume = 0.01f;
            public float m_range = 100;
            public AnimationCurve m_falloff;
            public int m_warmUpCount = 2;
        }
    }
}