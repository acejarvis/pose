using System.IO;
using UnityEditor;

public class TBBReplacer
{
    static string batName = "TBBReplacer.bat";

    public static void CreateBat()
    {
#if UNITY_EDITOR_WIN
        CheckBat();
        string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
        string nuitrackTbbPath = CmdPath(Path.Combine(nuitrackHomePath, "bin", "tbb.dll"));

        string editorPath = EditorApplication.applicationPath.Replace("Unity.exe", "");
        string unityTbbPath = CmdPath(Path.Combine(editorPath, "tbb.dll"));
        string unityTbbBackupPath = CmdPath(Path.Combine(editorPath, "tbb_backup.dll"));

        FileInfo fi = new FileInfo(batName);
        using (StreamWriter sw = fi.AppendText())
        {
            sw.WriteLine("rename " + CmdPath(Path.Combine(editorPath, "tbb.dll")) + " " + "tbb_backup.dll");
            sw.WriteLine("copy " + nuitrackTbbPath + " " + unityTbbPath);
            sw.WriteLine("start \"\" " + CmdPath(EditorApplication.applicationPath) + " -projectPath " + CmdPath(Directory.GetCurrentDirectory()));
            sw.WriteLine("del " + batName);
        }

        EditorApplication.quitting += Quit;
        EditorApplication.Exit(0);
#endif
    }

    static void Quit()
    {
        ProgramStarter.Run(batName, "");
    }

    public static void ShowMessage()
    {
#if UNITY_EDITOR_WIN
        if (EditorUtility.DisplayDialog("TBB-file",
                "You need to replace the tbb.dll file in Editor with Nuitrack compatible tbb.dll file. \n" +
                "If you click [Yes] the editor will be restarted and the file will be replaced automatically \n" +
                "(old tbb-file will be renamed to tbb_backup.dll)", "Yes", "No"))
        {
            TBBReplacer.CreateBat();
        }
#endif
    }

    static string CmdPath(string path)
    {
        path = "\"" + path.Replace("/", "\\") + "\"";
        return path;
    }

    public static void CheckBat()
    {
        if (File.Exists(batName))
        {
            FileInfo fi = new FileInfo(batName);
            fi.Delete();
        }
    }
}
