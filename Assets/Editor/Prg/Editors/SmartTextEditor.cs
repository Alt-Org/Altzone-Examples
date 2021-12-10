using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Editors
{
    [CustomEditor(typeof(SmartText))]
    public class SmartTextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            if (GUILayout.Button("Create Default Config"))
            {
                serializedObject.Update();
                UpdateState(serializedObject);
                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }

        private static void UpdateState(SerializedObject serializedObject)
        {
            var _localizationKey = serializedObject.FindProperty("_localizationKey");
            var curValue = _localizationKey.stringValue;
            if (string.IsNullOrWhiteSpace(curValue))
            {
                if (serializedObject.targetObject is SmartText smartText)
                {
                    _localizationKey.stringValue = smartText.ComponentName;
                }
            }
        }
    }
}