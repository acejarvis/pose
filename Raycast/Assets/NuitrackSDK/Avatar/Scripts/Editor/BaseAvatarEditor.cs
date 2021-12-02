using UnityEditor;

using nuitrack;
using NuitrackSDK.Avatar;


namespace NuitrackSDKEditor.Avatar
{
    [CustomEditor(typeof(BaseAvatar), true)]
    public class BaseAvatarEditor : NuitrackSDKEditor
    {
        protected virtual JointType SelectJoint { get; set; } = JointType.None;

        public override void OnInspectorGUI()
        {
            DrawSkeletonSettings();
            DrawDefaultInspector();

            DrawAvatarGUI();
        }

        /// <summary>
        /// Draw basic avatar settings
        /// </summary>
        protected void DrawSkeletonSettings()
        {
            BaseAvatar baseAvatar = serializedObject.targetObject as BaseAvatar;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skeleton settings", EditorStyles.boldLabel);

            SerializedProperty useCurrentUserTracker = serializedObject.FindProperty("useCurrentUserTracker");
            useCurrentUserTracker.boolValue = EditorGUILayout.Toggle("Use current user tracker", useCurrentUserTracker.boolValue);
            serializedObject.ApplyModifiedProperties();

            if (!useCurrentUserTracker.boolValue)
            {
                SerializedProperty skeletonID = serializedObject.FindProperty("skeletonID");
                skeletonID.intValue = EditorGUILayout.IntSlider("Skeleton ID", skeletonID.intValue, baseAvatar.MinSkeletonID, baseAvatar.MaxSkeletonID);
                serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty jointConfidence = serializedObject.FindProperty("jointConfidence");
            jointConfidence.floatValue = EditorGUILayout.Slider("Joint confidence", jointConfidence.floatValue, 0, 1);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Override this method to add your own settings and parameters in the Inspector.
        /// </summary>
        protected virtual void DrawAvatarGUI() { }      
    }
}