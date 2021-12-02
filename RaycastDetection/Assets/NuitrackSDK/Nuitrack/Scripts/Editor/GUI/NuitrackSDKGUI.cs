using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

using System;
using System.IO;
using System.Collections.Generic;


namespace NuitrackSDKEditor
{
    /// <summary>
    /// Put the <see cref="GUI"/> block code in the using statement to color the <see cref="GUI"/> elements in the specified color
    /// After the using block, the <see cref="GUI"/> color will return to the previous one
    ///
    /// <para>
    /// <example>
    /// This shows how to change the GUI color
    /// <code>
    /// using (new GUIColor(Color.green))
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class GUIColor : IDisposable
    {
        Color oldColor;

        public GUIColor(Color newColor)
        {
            oldColor = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = oldColor;
        }
    }

    /// <summary>
    /// Put the <see cref="Handles"/> block code in the using statement to color the <see cref="Handles"/> elements in the specified color
    /// After the using block, the <see cref="Handles"/> color will return to the previous one
    ///
    /// <para>
    /// <example>
    /// This shows how to change the Handles color
    /// <code>
    /// using (new HandlesColor(Color.green))
    /// {
    ///     // Your Handles code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class HandlesColor : IDisposable
    {
        Color oldColor;

        public HandlesColor(Color newColor)
        {
            oldColor = Handles.color;
            Handles.color = newColor;
        }

        public void Dispose()
        {
            Handles.color = oldColor;
        }
    }

    /// <summary>
    /// Place GUI elements in a horizontal group <seealso cref="EditorGUILayout.BeginHorizontal(GUIStyle, GUILayoutOption[])"/>
    /// 
    /// <para>
    /// <example>
    /// This shows how to put GUI elements in a horizontal group via using
    /// <code>
    /// using (new HorizontalGroup())
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class HorizontalGroup : IDisposable
    {
        /// <summary>
        /// Create a horizontal group
        /// </summary>
        /// <param name="guiStyle">GUI style (default is GUIStyle.none)</param>
        /// <param name="options">GUI layout option (GUILayout.Width, GUILayout.MinWidth and others)</param>
        public HorizontalGroup(GUIStyle guiStyle = null, GUILayoutOption[] options = null)
        {
            guiStyle ??= GUIStyle.none;
            EditorGUILayout.BeginHorizontal(guiStyle, options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Place GUI elements in a vertical group <seealso cref="EditorGUILayout.BeginVertical(GUIStyle, GUILayoutOption[])"/>
    /// 
    /// <para>
    /// <example>
    /// This shows how to put GUI elements in a vertical group via using
    /// <code>
    /// using (new VerticalGroup())
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class VerticalGroup : IDisposable
    {
        /// <summary>
        /// Create a vertical group
        /// </summary>
        /// <param name="guiStyle">GUI style (default is GUIStyle.none)</param>
        /// <param name="options">GUI layout option (GUILayout.Width, GUILayout.MinWidth and others)</param>
        public VerticalGroup(GUIStyle styles = null, GUILayoutOption[] options = null)
        {
            styles ??= GUIStyle.none;
            EditorGUILayout.BeginVertical(styles, options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// GUI draw helper class
    /// </summary>
    public static class NuitrackSDKGUI
    {
        /// <summary>
        ///  Draw an additional button to the right of the GUI element (for example, the clear or help button)
        ///  Return a rectangle to draw your GUI element with an indent for the button.
        /// </summary>
        /// <param name="buttonAction">Action when clicking on an button</param>
        /// <param name="iconName">Name of the icon for the button</param>
        /// <param name="tooltip">(optional) ToolTip displayed when hovering over the button</param>
        /// <returns>Rectangle to draw your GUI element with an indent for the button.</returns>
        public static Rect WithRightButton(UnityAction buttonAction, string iconName, string tooltip = "")
        {
            GUIContent buttonContent = EditorGUIUtility.IconContent(iconName);
            buttonContent.tooltip = tooltip;

            Rect main = EditorGUILayout.GetControlRect();
            main.xMax -= buttonContent.image.width;

            Rect addButtonRect = new Rect(main.x + main.width, main.y, buttonContent.image.width, main.height);

            if (GUI.Button(addButtonRect, buttonContent, GUIStyle.none))
                buttonAction.Invoke();

            return main;
        }

        /// <summary>
        /// Draw property with "Help" button.
        /// </summary>
        /// <param name="serializedObject">Target serialized object</param>
        /// <param name="propertyName">Name of object property</param>
        /// <param name="url">Click-through link</param>
        /// <param name="toolTip">(optional) ToolTip displayed when hovering over the button</param>
        public static void PropertyWithHelpButton(SerializedObject serializedObject, string propertyName, string url, string toolTip = "")
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            UnityAction helpClick = delegate { Application.OpenURL(url); };

            Rect propertyRect = WithRightButton(helpClick, "_Help", toolTip);

            EditorGUI.PropertyField(propertyRect, property);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw a GUI block of the path to the file supplemented with the "Browse" and "Clear" buttons. 
        /// This element also provides a file selection dialog box.
        /// </summary>
        /// <param name="path">Current path to file</param>
        /// <param name="filterLabel">Filter label</param>
        /// <param name="extension">Filterable file extensions</param>
        /// <returns>Path to file</returns>
        public static string OpenFileField(string path, string filterLabel, params string[] extension)
        {
            GUIContent browseButtonContent = EditorGUIUtility.IconContent("Project");
            browseButtonContent.text = "Browse";

            GUIContent clearButtonContent = EditorGUIUtility.IconContent("TreeEditor.Trash");
            clearButtonContent.text = "Clear";

            GUIContent errorMessage = EditorGUIUtility.IconContent("console.erroricon.sml");
            errorMessage.text = "Specified file was not found, check the correctness of the path";

            GUIContent warningMessage = EditorGUIUtility.IconContent("console.warnicon.sml");
            warningMessage.text = "Path is not specified";

            bool pathIsCorrect = File.Exists(path);

            Color color;

            if (path == string.Empty)
                color = Color.yellow;
            else if (!pathIsCorrect)
                color = Color.red;
            else
                color = Color.green;

            using (new GUIColor(color))
                GUILayout.BeginVertical(EditorStyles.helpBox);

            if (!pathIsCorrect || path == string.Empty)
            {
                GUIContent message = path == string.Empty ? warningMessage : errorMessage;
                GUILayout.Label(message, EditorStyles.wordWrappedLabel);
            }

            path = EditorGUILayout.TextField("Path to file", path);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(browseButtonContent))
            {
                string windowLabel = string.Format("Open {0} file", string.Join(", ", extension));
                string[] fileFilter = new string[]
                {
                    filterLabel,
                    string.Join(",", extension)
                };

                string newFilePath = EditorUtility.OpenFilePanelWithFilters(windowLabel, Application.dataPath, fileFilter);

                if (newFilePath != null && newFilePath != string.Empty)
                    path = newFilePath;
            }

            EditorGUI.BeginDisabledGroup(path == string.Empty);

            if (GUILayout.Button(clearButtonContent))
            {
                path = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            return path;
        }


        #region Message

        static Dictionary<LogType, Color> messageColors = new Dictionary<LogType, Color>()
        {
            { LogType.Warning, Color.yellow },
            { LogType.Log, Color.white },
            { LogType.Error, Color.red },
            { LogType.Assert, Color.red },
            { LogType.Exception, Color.red }
        };

        public static void NuitrackNotExistMessage()
        {
            DrawMessage("Make sure that when the script is running, the NuitrackScripts prefab will be on the scene.", LogType.Warning);
        }

        public static void DrawMessage(string message, LogType messageType, UnityAction fixAction = null, string fixButtonLabel = null)
        {
            EditorGUILayout.Space();

            using (new GUIColor(messageColors[messageType]))
                GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);

            if (fixAction != null)
                if (GUILayout.Button(fixButtonLabel))
                    fixAction.Invoke();

            GUILayout.EndVertical();
        }

        #endregion
    }
}