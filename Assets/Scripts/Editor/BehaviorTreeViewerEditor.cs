using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Entity))]
public class BehaviorTreeViewerEditor : Editor
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
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Entity entity = (Entity)target;
        var runnerField = typeof(Entity).GetField("behaviorTreeRunner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var runner = runnerField?.GetValue(entity) as BehaviorTreeRunner;

        // Blackboard 변수 표시
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Blackboard Variables", EditorStyles.boldLabel);
        var blackboardField = typeof(Entity).GetField("blackboard", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var blackboard = blackboardField?.GetValue(entity) as Blackboard;
        if (blackboard != null)
        {
            var dataField = typeof(Blackboard).GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var data = dataField?.GetValue(blackboard) as Dictionary<string, object>;
            if (data != null && data.Count > 0)
            {
                foreach (var pair in data)
                {
                    EditorGUILayout.LabelField($"{pair.Key}: {pair.Value}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("No variables in blackboard.");
            }
        }
        else
        {
            EditorGUILayout.LabelField("Blackboard not found.");
        }

        // 기존 트리 표시
        EditorGUILayout.Space();
        if (runner != null)
        {
            EditorGUILayout.LabelField("Behavior Tree", EditorStyles.boldLabel);

            var rootNodeField = typeof(BehaviorTreeRunner).GetField("rootNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Node rootNode = rootNodeField?.GetValue(runner) as Node;

            if (rootNode != null)
            {
                DrawNodeRecursive(rootNode, 0);
            }
            else
            {
                EditorGUILayout.LabelField("No root node found.");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No BehaviorTreeRunner found.");
        }
    }

    private void DrawNodeRecursive(Node node, int indent)
    {
        EditorGUI.indentLevel = indent;

        // BehaviorTreeRunner에서 상태 가져오기
        Entity entity = (Entity)target;
        var runnerField = typeof(Entity).GetField("behaviorTreeRunner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var runner = runnerField?.GetValue(entity) as BehaviorTreeRunner;

        NodeState execState = NodeState.None;
        if (runner != null && runner.GetType().GetField("nodeStates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(runner) is Dictionary<Node, NodeState> nodeStates && nodeStates.ContainsKey(node))
        {
            execState = nodeStates[node];
        }

        // 상태에 따라 색상/폰트 스타일 결정
        Color color = Color.black;
        GUIStyle style = new GUIStyle(EditorStyles.label);

        switch (execState)
        {
            case NodeState.Success:
                color = Color.green;
                style.fontStyle = FontStyle.Bold;
                break;
            case NodeState.Failure:
                color = Color.red;
                style.fontStyle = FontStyle.Bold;
                break;
            case NodeState.Running:
                color = Color.yellow;
                style.fontStyle = FontStyle.Bold;
                break;
            case NodeState.None:
            default:
                color = Color.white;
                style.fontStyle = FontStyle.Normal;
                break;
        }

        style.normal.textColor = color;

        EditorGUILayout.LabelField(node.EditorTreeViewer(execState), style);

        // CompositeNode
        if (node is CompositeNode composite)
        {
            var children = composite.GetType().GetField("childrenNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(composite) as List<Node>;
            foreach (var child in children)
            {
                DrawNodeRecursive(child, indent + 1);
            }
        }
        // DecoratorNode
        else if (node is DecoratorNode decorator)
        {
            var childNode = decorator.GetType().GetField("childNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(decorator) as Node;
            if (childNode != null)
                DrawNodeRecursive(childNode, indent + 1);
        }
    }
}