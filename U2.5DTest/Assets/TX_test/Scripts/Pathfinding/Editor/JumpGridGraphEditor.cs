using UnityEngine;
using UnityEditor;
using Pathfinding;


/*
----IMPORTANT----
You need to add the following to line 59 in AstarData.cs:
typeof(GridJumpGraph),

so that the code looks like this:

        public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
            typeof(GridGraph),
            typeof(PointGraph),
            typeof(NavMeshGraph),
            typeof(GridJumpGraph),
        };

    Otherwise JumpGraph will not show up in the editor
    */
[CustomGraphEditor(typeof(GridJumpGraph), "GridJumpGraph")]
public class JumpGridGraphEditor : GridGraphEditor {

    public override void OnInspectorGUI(NavGraph target) {
        var graph = target as GridJumpGraph;

        graph.maxJumpHeight = EditorGUILayout.FloatField(new GUIContent("Max Jump Height", "The maximum distance a character can jump. The max distance in world space for a connection to be valid. A zero counts as infinity"), graph.maxJumpHeight);
        graph.maxFlyHeight = EditorGUILayout.FloatField(new GUIContent("Max Fly Height", "The maximum distance a character can fly. The max distance in world space for a connection to be valid. A zero counts as infinity"), graph.maxFlyHeight);
        graph.diggableLayers = EditorGUILayoutx.LayerMaskField("Diggable layers", graph.diggableLayers);

        GUILayout.Label("Make sure to set seeker to search using correct tags depending on what it can do");
        graph.walkableTag = (uint)EditorGUILayoutx.TagField("Walkable tag", (int)graph.walkableTag);
        graph.jumpableTag = (uint)EditorGUILayoutx.TagField("Jumpable tag", (int)graph.jumpableTag);
        graph.flyableTag = (uint)EditorGUILayoutx.TagField("Flyable tag", (int)graph.flyableTag);
        graph.diggableTag = (uint)EditorGUILayoutx.TagField("Diggable tag", (int)graph.diggableTag);
       
        //graph.maxDropHeight = EditorGUILayout.FloatField(new GUIContent("Max Drop Height", "The maximum distance a character can drop willingly. The max distance in world space for a connection to be valid. A zero counts as infinity"), graph.maxDropHeight);

        base.OnInspectorGUI(graph);

    }

    
}
