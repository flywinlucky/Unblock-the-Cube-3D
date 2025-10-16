using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenăm toate proprietățile serializate, EXCEPȚIE "blocks" (blocurile sunt gestionate în LevelEditorWindow)
        serializedObject.Update();
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (prop.name == "blocks") continue; // sărim lista raw de block-uri
            EditorGUILayout.PropertyField(prop, true);
        }
        serializedObject.ApplyModifiedProperties();

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