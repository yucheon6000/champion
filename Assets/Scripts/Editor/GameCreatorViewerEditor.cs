using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(GameCreator))]
[CanEditMultipleObjects]
public class GameCreatorViewerEditor : Editor
{
    private void OnEnable()
    {
        EditorApplication.update += RepaintInspector;
    }

    private void OnDisable()
    {
        EditorApplication.update -= RepaintInspector;
    }

    private void RepaintInspector()
    {
        if (this != null)
            Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Global Blackboard Variables", EditorStyles.boldLabel);

        // Blackboard의 글로벌 변수(static) 직접 표시
        var globalDict = Blackboard.GetGlobalData();

        if (globalDict != null && globalDict.Count > 0)
        {
            foreach (var pair in globalDict)
            {
                EditorGUILayout.LabelField($"{pair.Key}: {pair.Value}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No global variables.");
        }
    }
}