using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (target == null) return;
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
        if (levelData == null) return;

        // NOU: Afișăm dimensiunile personalizate ale grilei
        EditorGUILayout.LabelField("Custom Grid Dimensions", EditorStyles.boldLabel);
        levelData.customGridLength = EditorGUILayout.IntSlider("Grid Length", levelData.customGridLength, 2, 40);
        levelData.customGridHeight = EditorGUILayout.IntSlider("Grid Height", levelData.customGridHeight, 2, 40);
    }
}