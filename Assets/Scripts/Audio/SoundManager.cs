using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace Audio
    {
        public class SoundManager : MonoBehaviour
        {
            public static SoundManager singleton;

            [SerializeField] private GameObject soundPrefab;

            [Header("Audio Clips")]
            [SerializeField] private SoundAsset[] m_audioAssets;
            [Header("Debug")]
            [SerializeField] private bool m_hideSoundsInHierarchy = true;
            [SerializeField] private bool DEBUG_noCache = false;

            private bool m_lastHideSoundsInHierarchy;
            private Dictionary<int, List<Sound>> m_soundIndex_freeSounds = null;
            private List<Sound> m_activeSounds = new List<Sound>();

            private void Awake()
            {
                m_lastHideSoundsInHierarchy = m_hideSoundsInHierarchy;

                singleton = this;

                if (DEBUG_noCache)
                {
                    Debug.LogWarning("SoundManager: Awake: sound cache is deactivated !");
                }

                m_soundIndex_freeSounds = new Dictionary<int, List<Sound>>();

                for (int i = 0; i < m_audioAssets.Length; i++)
                {
                    m_soundIndex_freeSounds.Add(i, new List<Sound>());
                }
            }

            private void Start()
            {
                soundsWarmUp();
            }

            private void Update()
            {
                updateHideInHierarchy();
            }

            private void updateHideInHierarchy()
            {
                if (m_lastHideSoundsInHierarchy != m_hideSoundsInHierarchy)
                {
                    m_lastHideSoundsInHierarchy = m_hideSoundsInHierarchy;

                    for (int i = 0; i < m_activeSounds.Count; i++)
                    {
                        if (m_hideSoundsInHierarchy)
                        {
                            m_activeSounds[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                        }
                        else
                        {
                            m_activeSounds[i].gameObject.hideFlags = HideFlags.None;
                        }
                    }

                    foreach (KeyValuePair<int, List<Sound>> pair in m_soundIndex_freeSounds)
                    {
                        for (int i = 0; i < pair.Value.Count; i++)
                        {
                            if (m_hideSoundsInHierarchy)
                            {
                                pair.Value[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                            }
                            else
                            {
                                pair.Value[i].gameObject.hideFlags = HideFlags.None;
                            }
                        }
                    }
                }
            }

            private void soundsWarmUp()
            {
                for (int i = 0; i < m_audioAssets.Length; i++)
                {
                    for (int j = 0; j < m_audioAssets[i].m_warmUpCount; j++)
                    {
                        Sound tempSound = getSound(i);

                        tempSound.setVolume(0);
                        tempSound.playOnce();
                    }
                }
            }

            public Sound playGlobalSound(int soundIndex, Sound.SoundPlaystyle playStyle = Sound.SoundPlaystyle.Once)
            {
                if (soundIndex >= m_audioAssets.Length || soundIndex < 0)
                {
                    Debug.LogError("SoundManager: soundIndex out of range: " + soundIndex);
                    return null;
                }

                Sound tempSound = getSound(soundIndex);
                tempSound.setGlobalLocal(true);

                if (playStyle == Sound.SoundPlaystyle.loop)
                {
                    tempSound.playLooping();
                }
                else if (playStyle == Sound.SoundPlaystyle.Once)
                {
                    tempSound.playOnce();
                }

                return tempSound;
            }

            public Sound playSoundAt(int soundIndex, Vector3 position, Sound.SoundPlaystyle playStyle = Sound.SoundPlaystyle.Once)
            {
                if (soundIndex >= m_audioAssets.Length || soundIndex < 0)
                {
                    Debug.LogError("SoundManager: soundIndex out of range: " + soundIndex);
                    return null;
                }

                Sound tempSound = getSound(soundIndex);

                tempSound.transform.position = position;
                tempSound.setGlobalLocal(false);

                if (playStyle == Sound.SoundPlaystyle.loop)
                {
                    tempSound.playLooping();
                }
                else if (playStyle == Sound.SoundPlaystyle.Once)
                {
                    tempSound.playOnce();
                }

                return tempSound;
            }

            private Sound getSound(int soundIndex)
            {
                Sound returnValue = null;

                if (m_soundIndex_freeSounds[soundIndex].Count < 1 || DEBUG_noCache)
                {
                    returnValue = (Instantiate(soundPrefab) as GameObject).GetComponent<Sound>();
                    returnValue.initialize(soundIndex, m_audioAssets[soundIndex].m_audioClip, m_audioAssets[soundIndex].m_volume, false, m_audioAssets[soundIndex].m_falloff);
                    if (m_hideSoundsInHierarchy)
                    {
                        returnValue.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
                else
                {
                    returnValue = m_soundIndex_freeSounds[soundIndex][0];
                    m_soundIndex_freeSounds[soundIndex].RemoveAt(0);
                    returnValue.setDefaultVolume();
                    returnValue.gameObject.SetActive(true);
                }

                m_activeSounds.Add(returnValue);

                return returnValue;
            }

            public void recyleSound(Sound sound)
            {
                sound.transform.SetParent(null);
                sound.stopPlaying();
                sound.gameObject.SetActive(false);
                m_activeSounds.Remove(sound);
                m_soundIndex_freeSounds[sound.SoundIndex].Add(sound);
            }

            public float getMaxHearableDistance(int soundIndex)
            {
                if (soundIndex < 0 || soundIndex >= m_audioAssets.Length)
                {
                    Debug.LogWarning("SoundManager: getMaxHearableDistance: index out of range");
                    return 0;
                }
                else
                {
                    return m_audioAssets[soundIndex].m_range;
                }
            }
        }
    }
}
