using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using NuitrackSDKEditor.Documentation;

namespace NuitrackSDKEditor
{
    [InitializeOnLoad]
    public class NuitrackMenu : MonoBehaviour
    {
        static string nuitrackScriptsPath = "Assets/NuitrackSDK/Nuitrack/Prefabs/NuitrackScripts.prefab";

        [MenuItem("Nuitrack/Prepare The Scene")]
        public static void AddNuitrackToScene()
        {
            UnityEngine.Object nuitrackScriptsPrefab = AssetDatabase.LoadAssetAtPath(nuitrackScriptsPath, typeof(GameObject));

            if (nuitrackScriptsPrefab == null)
                Debug.LogAssertion(string.Format("Prefab NuitrackScripts was not found at {0}", nuitrackScriptsPath));
            else
            {
                NuitrackManager nuitrackManager = FindObjectOfType<NuitrackManager>();

                if (nuitrackManager != null)
                {
                    EditorGUIUtility.PingObject(nuitrackManager);
                    Debug.LogWarning("NuitrackManager already exists on the scene.");
                }
                else
                {
                    UnityEngine.Object nuitrackScripts = PrefabUtility.InstantiatePrefab(nuitrackScriptsPrefab);
                    Undo.RegisterCreatedObjectUndo(nuitrackScripts, string.Format("Create object {0}", nuitrackScripts.name));
                    Selection.activeObject = nuitrackScripts;
                }
            }
        }

        [MenuItem("Nuitrack/Help/Open Github Page", priority = 20)]
        public static void GoToGithubPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/");
        }

        [MenuItem("Nuitrack/Help/Open tutorials list", priority = 21)]
        public static void OpenTutoralList()
        {
            NuitrackTutorialsEditorWindow.Open();
        }

        [MenuItem("Nuitrack/Help/Open Troubleshooting Page", priority = 22)]
        public static void GoToTroubleshootingPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md#troubleshooting");
        }

        [MenuItem("Nuitrack/Manage Nuitrack License", priority = 1)]
        public static void GoToLicensePage()
        {
            Application.OpenURL("https://cognitive.3divi.com");
        }

        [MenuItem("Nuitrack/Open Nuitrack Activation Tool", priority = 0)]
        public static void OpenNuitrackApp()
        {
            string nuitrackHomePath = Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string workingDir = Path.Combine(nuitrackHomePath, "activation_tool");
            string path = Path.Combine(workingDir, "Nuitrack.exe");

            if (nuitrackHomePath != null)
                ProgramStarter.Run(path, workingDir, true);
        }

        [MenuItem("Nuitrack/Open Nuitrack Test Sample", priority = 1)]
        public static void OpenNuitrackTestSample()
        {
            string nuitrackHomePath = Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string workingDir = Path.Combine(nuitrackHomePath, "bin");
            string path = Path.Combine(workingDir, "nuitrack_sample.exe");

            if (nuitrackHomePath != null)
                ProgramStarter.Run(path, workingDir, true);
        }
    }
}