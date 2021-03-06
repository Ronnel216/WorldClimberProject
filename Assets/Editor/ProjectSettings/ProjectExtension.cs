﻿using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[InitializeOnLoad]
public static class EditorApplicationUtility
{
    private const BindingFlags BINDING_ATTR = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

    private static readonly FieldInfo m_info0 = typeof(EditorApplication).GetField("projectWasLoaded", BINDING_ATTR);
    private static readonly FieldInfo m_info1 = typeof(EditorApplication).GetField("editorApplicationQuit", BINDING_ATTR);

    public static UnityAction projectWasLoaded
    {
        get
        {
            return m_info0.GetValue(null) as UnityAction;
        }
        set
        {
            var functions = m_info0.GetValue(null) as UnityAction;
            functions += value;
            m_info0.SetValue(null, functions);
        }
    }

    public static UnityAction editorApplicationQuit
    {
        get
        {
            return m_info1.GetValue(null) as UnityAction;
        }
        set
        {
            var functions = m_info1.GetValue(null) as UnityAction;
            functions += value;
            m_info1.SetValue(null, functions);
        }
    }
}

public static class EditorCallBackMethod
{
    [InitializeOnLoadMethod]
    private static void AddEventProjectWasLoaded()
    {
        EditorApplicationUtility.projectWasLoaded += () =>
        {
            EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
        };


    }

    [InitializeOnLoadMethod]
    private static void AddEventEditorApplicationQuit()
    {
        EditorApplicationUtility.editorApplicationQuit += () =>
        {
            System.Diagnostics.Process.Start("C:/Users/s162121/AppData/Roaming/Microsoft/Windows/Start Menu/Programs/Atlassian/Sourcetree");
        };
    }
}

[CustomEditor(typeof(LineRenderByConnector))]//拡張するクラスを指定
public class ExampleScriptEditor : Editor
{

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        var targetScript = target as LineRenderByConnector;

        //ボタンを表示
        if (GUILayout.Button("Apply"))
        {
            targetScript.Apply();
        }
    }

}
