using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using UnityEditor;
using UnityEditor.UI;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Frame
{
    [CustomEditor(typeof(NuitrackAspectRatioFitter), true)]
    public class NuitrackAspectRatioFitter_Editor : AspectRatioFitterEditor
    {
        SerializedProperty m_FrameMode;
        SerializedProperty m_AspectMode;

        NuitrackAspectRatioFitter nuitrackAspectRatioFitter;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_FrameMode = serializedObject.FindProperty("frameMode");
            m_AspectMode = serializedObject.FindProperty("m_AspectMode");

            nuitrackAspectRatioFitter = target as NuitrackAspectRatioFitter;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_FrameMode);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();

            base.OnInspectorGUI();

            if (FindObjectOfType<NuitrackManager>(true) == null)
                NuitrackSDKGUI.NuitrackNotExistMessage();

            if (nuitrackAspectRatioFitter.aspectMode != AspectRatioFitter.AspectMode.FitInParent)
            {
                UnityAction fixAspectMode = delegate { FixAspectMode(m_AspectMode); };

                string message = string.Format("Aspect Mode is set to {0}." +
                    "The frame from the sensor may not be displayed correctly." +
                    "\nRecommended: Fit In Parent.",
                    nuitrackAspectRatioFitter.aspectMode);

                NuitrackSDKGUI.DrawMessage(message, LogType.Warning, fixAspectMode, "Fix");
            }

            serializedObject.ApplyModifiedProperties();
        }

        void FixAspectMode(SerializedProperty aspectmodeProperty)
        {
            aspectmodeProperty.enumValueIndex = (int)AspectRatioFitter.AspectMode.FitInParent;
        }
    }
}
