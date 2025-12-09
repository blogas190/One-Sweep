using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(Transform))]
[CanEditMultipleObjects]
public class SetWorldSizeEditor : Editor
{
    private Editor defaultEditor;
    private Vector3 targetSize = Vector3.one;

    void OnEnable()
    {
        // Get Unity's built-in Transform editor
        defaultEditor = Editor.CreateEditor(targets, System.Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
    }

    void OnDisable()
    {
        if (defaultEditor != null)
        {
            DestroyImmediate(defaultEditor);
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw Unity's original Transform inspector (with Position/Rotation/Scale)
        if (defaultEditor != null)
        {
            defaultEditor.OnInspectorGUI();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);

        GUILayout.Label("Set World Size", EditorStyles.boldLabel);

        // Show current world size
        if (Selection.transforms.Length == 1)
        {
            Transform t = Selection.transforms[0];
            Renderer r = t.GetComponentInChildren<Renderer>();

            if (r != null)
            {
                Vector3 currentSize = r.bounds.size;
                EditorGUILayout.LabelField("Current World Size:", EditorStyles.boldLabel);

                GUI.enabled = false;
                EditorGUILayout.Vector3Field("", currentSize);
                GUI.enabled = true;

                GUILayout.Space(5);
            }
            else
            {
                EditorGUILayout.HelpBox("No Renderer found - cannot determine world size.", MessageType.Warning);
            }
        }
        else if (Selection.transforms.Length > 1)
        {
            EditorGUILayout.HelpBox($"{Selection.transforms.Length} objects selected. Select a single object to see current world size.", MessageType.Info);
        }

        targetSize = EditorGUILayout.Vector3Field("Target Size (meters)", targetSize);

        if (GUILayout.Button("Apply World Size"))
        {
            foreach (Transform t in Selection.transforms)
            {
                SetWorldSize(t, targetSize);
            }
        }

        EditorGUILayout.HelpBox("Sets the world-space size of the object by adjusting its scale.", MessageType.Info);
    }

    void SetWorldSize(Transform obj, Vector3 targetSize)
    {
        // Find any renderer (mesh or sprite)
        Renderer r = obj.GetComponentInChildren<Renderer>();
        if (r == null)
        {
            Debug.LogWarning($"No Renderer found on {obj.name}!");
            return;
        }

        Vector3 currentSize = r.bounds.size;
        Vector3 scale = obj.localScale;

        if (currentSize.x > 0) scale.x *= targetSize.x / currentSize.x;
        if (currentSize.y > 0) scale.y *= targetSize.y / currentSize.y;
        if (currentSize.z > 0) scale.z *= targetSize.z / currentSize.z;

        Undo.RecordObject(obj, "Set World Size");
        obj.localScale = scale;
    }
}