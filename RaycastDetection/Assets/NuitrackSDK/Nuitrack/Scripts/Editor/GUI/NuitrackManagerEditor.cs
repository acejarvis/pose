using UnityEngine;
using UnityEditor;


namespace NuitrackSDKEditor
{
    [CustomEditor(typeof(NuitrackManager), true)]
    public class NuitrackManagerEditor : NuitrackSDKEditor
    {
        readonly string[] modulesFlagNames = new string[]
        {
            "depthModuleOn",
            "colorModuleOn",
            "userTrackerModuleOn",
            "skeletonTrackerModuleOn",
            "gesturesRecognizerModuleOn",
            "handsTrackerModuleOn"
        };

        bool openMdules = false;

        public override void OnInspectorGUI()
        {
            DrawModules();

            DrawDefaultInspector();

            DrawConfiguration();
            DrawSensorOptions();
            DrawRecordFileGUI();

            DrawInitEvetn();
        }

        void DrawModules()
        {
            openMdules = EditorGUILayout.BeginFoldoutHeaderGroup(openMdules, "Modules");

            if (openMdules)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                foreach (string propertyName in modulesFlagNames)
                {
                    SerializedProperty property = serializedObject.FindProperty(propertyName);
                    EditorGUILayout.PropertyField(property);
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawConfiguration()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            SerializedProperty runInBackground = serializedObject.FindProperty("runInBackground");
            EditorGUILayout.PropertyField(runInBackground);
            serializedObject.ApplyModifiedProperties();

            NuitrackSDKGUI.PropertyWithHelpButton(
                serializedObject,
                "wifiConnect",
                "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#wireless-case",
                "Only skeleton. PC, Unity Editor, MacOS and IOS");


            NuitrackSDKGUI.PropertyWithHelpButton(
                serializedObject,
                "useNuitrackAi",
                "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md",
                "ONLY PC! Nuitrack AI is the new version of Nuitrack skeleton tracking middleware");

            NuitrackSDKGUI.PropertyWithHelpButton(
                 serializedObject,
                 "useFaceTracking",
                 "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Unity_Face_Tracking.md",
                 "Track and get information about faces with Nuitrack (position, angle of rotation, box, emotions, age, gender)");
        }

        void DrawSensorOptions()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sensor options", EditorStyles.boldLabel);

            SerializedProperty depth2ColorRegistration = serializedObject.FindProperty("depth2ColorRegistration");
            EditorGUILayout.PropertyField(depth2ColorRegistration);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty mirrorProp = serializedObject.FindProperty("mirror");
            EditorGUILayout.PropertyField(mirrorProp);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty sensorRotation = serializedObject.FindProperty("sensorRotation");

            if (mirrorProp.boolValue)
                sensorRotation.enumValueIndex = 0;

            EditorGUI.BeginDisabledGroup(mirrorProp.boolValue);

            EditorGUILayout.PropertyField(sensorRotation);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
        }

        void DrawRecordFileGUI()
        {
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            
            SerializedProperty useFileRecordProp = serializedObject.FindProperty("useFileRecord");
            EditorGUILayout.PropertyField(useFileRecordProp, new GUIContent("Use record file"));
            serializedObject.ApplyModifiedProperties();

            if (useFileRecordProp.boolValue)
            {
                SerializedProperty pathProperty = serializedObject.FindProperty("pathToFileRecord");

                pathProperty.stringValue = NuitrackSDKGUI.OpenFileField(pathProperty.stringValue, "Bag or oni file", "bag", "oni");

                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawInitEvetn()
        {
            EditorGUILayout.Space();

            SerializedProperty asyncInit = serializedObject.FindProperty("asyncInit");
            EditorGUILayout.PropertyField(asyncInit);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty initEvent = serializedObject.FindProperty("initEvent");
            EditorGUILayout.PropertyField(initEvent);
            serializedObject.ApplyModifiedProperties();
        }
    }
}