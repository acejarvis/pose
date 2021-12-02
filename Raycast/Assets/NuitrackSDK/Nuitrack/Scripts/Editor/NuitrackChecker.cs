using UnityEngine;
using UnityEditor;
using System.IO;

public class NuitrackChecker
{
    static BuildTargetGroup buildTargetGroup;
    static string backendMessage;

    static string filename = "nuitrack.lock";

    public static void Check()
    {
        buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        backendMessage = "Current Scripting Backend " + PlayerSettings.GetScriptingBackend(buildTargetGroup) + "  Target:" + buildTargetGroup;

        PingNuitrack();
    }

    static void PingNuitrack()
    {
        try
        {
            nuitrack.Nuitrack.Init();

            string initSuccessMessage = "<color=green><b>Test Nuitrack (ver." + nuitrack.Nuitrack.GetVersion() + ") init was successful!</b></color>\n" + backendMessage;

            if (nuitrack.Nuitrack.GetDeviceList().Count > 0)
            {
                for (int i = 0; i < nuitrack.Nuitrack.GetDeviceList().Count; i++)
                {
                    nuitrack.device.NuitrackDevice device = nuitrack.Nuitrack.GetDeviceList()[i];
                    string sensorName = device.GetInfo(nuitrack.device.DeviceInfoType.DEVICE_NAME);
                    initSuccessMessage += "\nDevice " + i + " [Sensor Name: " + sensorName + ", License: " + device.GetActivationStatus() + "]";
                }
            }
            else
            {
                initSuccessMessage += "\nSensor not connected";
            }

            nuitrack.Nuitrack.Release();
            Debug.Log(initSuccessMessage);
        }
        catch (System.Exception ex)
        {
            Debug.Log("<color=red><b>Test Nuitrack init failed!</b></color>\n" +
                "<color=red><b>It is recommended to test on AllModulesScene. (Start the scene and follow the on-screen instructions)</b></color>\n" + backendMessage);

            Debug.Log(ex.ToString());
        }

        if (!File.Exists(filename))
        {
            FileInfo fi = new FileInfo(filename);
            fi.Create();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}
