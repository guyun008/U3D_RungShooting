using UnityEngine;
using UnityEditor;
using Pathfinding;


/*
----IMPORTANT----
You need to add the following to line 59 in AstarData.cs:
typeof(JumpGraph),

so that the code looks like this:

        public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
            typeof(GridGraph),
            typeof(PointGraph),
            typeof(NavMeshGraph),
            typeof(JumpGraph),
        };

    Otherwise JumpGraph will not show up in the editor
    */
[CustomGraphEditor(typeof(JumpGraph), "JumpGraph")]
public class JumpGraphEditor : PointGraphEditor {

    public override void OnInspectorGUI(NavGraph target) {
        var graph = target as JumpGraph;

        graph.maxJumpHeight = EditorGUILayout.FloatField(new GUIContent("Max Jump Height", "The maximum distance a character can jump. The max distance in world space for a connection to be valid. A zero counts as infinity"), graph.maxJumpHeight);
        graph.maxDropHeight = EditorGUILayout.FloatField(new GUIContent("Max Drop Height", "The maximum distance a character can drop willingly. The max distance in world space for a connection to be valid. A zero counts as infinity"), graph.maxDropHeight);

        base.OnInspectorGUI(graph);

    }

    
}
