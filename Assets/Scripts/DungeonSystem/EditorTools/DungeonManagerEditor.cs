#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DungeonSystem.EditorTools       
{
    [CustomEditor(typeof(DungeonManager))]
    public class DungeonManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var manager = (DungeonManager)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Generation Controls", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Complete Dungeon", GUILayout.Height(30)))
                {
                    manager.GenerateCompleteDungeon();
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("New Random Seed", GUILayout.Height(30)))
                {
                    manager.GenerateNewSeed();
                    SceneView.RepaintAll();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Structure Only", GUILayout.Height(25)))
                {
                    manager.GenerateMapStructure();
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Select Starting Point", GUILayout.Height(25)))
                {
                    manager.SelectStartingPoint();
                    SceneView.RepaintAll();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Populate Entities", GUILayout.Height(25)))
                {
                    manager.PopulateWithEntities();
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Render Dungeon", GUILayout.Height(25)))
                {
                    manager.RenderDungeon();
                    SceneView.RepaintAll();
                }
            }

            if (GUILayout.Button("Clear Dungeon", GUILayout.Height(25)))
            {
                manager.ClearDungeon();
                SceneView.RepaintAll();
            }
        }
    }
}
#endif
