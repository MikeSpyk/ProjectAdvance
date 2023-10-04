// https://forum.unity.com/threads/tutorial-how-to-make-clothes-animate-along-with-character.475253/

using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace Animations
    {
        /// <summary>
        /// used to mirror animations from one skeleton to another. like animate a piece of clothing with a character
        /// </summary>
        [RequireComponent(typeof(SkinnedMeshRenderer))]
        public class EquipmentParenter : MonoBehaviour
        {
            [SerializeField] private SkinnedMeshRenderer TargetMeshRenderer;

            void Start()
            {
                Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
                foreach (Transform bone in TargetMeshRenderer.bones)
                    boneMap[bone.gameObject.name] = bone;

                SkinnedMeshRenderer myRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                Transform[] newBones = new Transform[myRenderer.bones.Length];
                for (int i = 0; i < myRenderer.bones.Length; ++i)
                {
                    GameObject bone = myRenderer.bones[i].gameObject;
                    if (!boneMap.TryGetValue(bone.name, out newBones[i]))
                    {
                        Debug.Log(gameObject.name + ": Unable to map bone \"" + bone.name + "\" to target skeleton.");
                        break;
                    }
                }
                myRenderer.bones = newBones;

                Destroy(this);
            }
        }
    }
}