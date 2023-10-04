using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BasicTools
{
    namespace Audio
    {
        [CustomPropertyDrawer(typeof(SoundAsset))]
        public class SoundAssetDrawer : PropertyDrawer
        {
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                // Create property container element.
                var container = new VisualElement();

                Foldout foldout = new Foldout() { text = "Settings" };

                // Create property fields.
                var audioClipField = new PropertyField(property.FindPropertyRelative("m_audioClip")) { label = string.Format("{0}: AudioClip", property.displayName) };
                var volumeClipField = new PropertyField(property.FindPropertyRelative("m_volume"));
                var rangeClipField = new PropertyField(property.FindPropertyRelative("m_range"));
                var falloffClipField = new PropertyField(property.FindPropertyRelative("m_falloff"));
                var warmUpCountField = new PropertyField(property.FindPropertyRelative("m_warmUpCount"));

                // Add fields to the container.
                container.Add(audioClipField);
                container.Add(foldout);
                foldout.Add(volumeClipField);
                foldout.Add(rangeClipField);
                foldout.Add(falloffClipField);
                foldout.Add(warmUpCountField);

                return container;
            }
        }
    }
}
