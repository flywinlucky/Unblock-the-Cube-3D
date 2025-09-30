using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Redesenăm interfața default. Câmpurile care nu mai există în LevelData vor dispărea automat.
        base.DrawDefaultInspector();

        EditorGUILayout.Space();

        LevelData levelData = (LevelData)target;
        if (GUILayout.Button("Generate Level", GUILayout.Height(40)))
        {
            levelData.Generate();
            EditorUtility.SetDirty(levelData);
            Debug.Log("Level generated successfully with " + levelData.GetBlocks().Count + " blocks!");
        }
    }
}