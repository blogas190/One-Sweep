#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Mover))]
public class MoverEditor : Editor
{
    // Movement Settings
    SerializedProperty movementType;
    SerializedProperty speed;
    SerializedProperty autoStart;
    SerializedProperty loop;
    SerializedProperty faceMovementDirection;
    SerializedProperty rotationSpeed;

    // Wait Time Settings
    SerializedProperty useGlobalWaitTime;
    SerializedProperty globalWaitTime;

    // Straight
    SerializedProperty startPoint;
    SerializedProperty endPoint;

    // Circular
    SerializedProperty center;
    SerializedProperty radius;
    SerializedProperty clockwise;
    SerializedProperty plane;
    SerializedProperty startAngle;

    // Follow Points
    SerializedProperty waypoints;

    // Debug
    SerializedProperty showPath;
    SerializedProperty pathColor;

    void OnEnable()
    {
        movementType = serializedObject.FindProperty("movementType");
        speed = serializedObject.FindProperty("speed");
        autoStart = serializedObject.FindProperty("autoStart");
        loop = serializedObject.FindProperty("loop");
        faceMovementDirection = serializedObject.FindProperty("faceMovementDirection");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");

        useGlobalWaitTime = serializedObject.FindProperty("useGlobalWaitTime");
        globalWaitTime = serializedObject.FindProperty("globalWaitTime");

        startPoint = serializedObject.FindProperty("startPoint");
        endPoint = serializedObject.FindProperty("endPoint");

        center = serializedObject.FindProperty("center");
        radius = serializedObject.FindProperty("radius");
        clockwise = serializedObject.FindProperty("clockwise");
        plane = serializedObject.FindProperty("plane");
        startAngle = serializedObject.FindProperty("startAngle");

        waypoints = serializedObject.FindProperty("waypoints");

        showPath = serializedObject.FindProperty("showPath");
        pathColor = serializedObject.FindProperty("pathColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Mover.MovementType selectedType = (Mover.MovementType)movementType.enumValueIndex;

        // Movement Settings 
        //EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(movementType);
        EditorGUILayout.PropertyField(speed);
        EditorGUILayout.PropertyField(autoStart);
        EditorGUILayout.PropertyField(loop);
        EditorGUILayout.PropertyField(faceMovementDirection);

        // rotationSpeed only makes sense when facing is on
        if (faceMovementDirection.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(rotationSpeed);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(6);

        // Wait Time Settings
        // FollowPoints uses per-waypoint wait times, so global wait is irrelevant
        if (selectedType != Mover.MovementType.FollowPoints)
        {
            //EditorGUILayout.LabelField("Wait Time Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useGlobalWaitTime);
            if (useGlobalWaitTime.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(globalWaitTime);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(6);
        }

        // Type-specific Settings
        switch (selectedType)
        {
            case Mover.MovementType.Straight:
                //EditorGUILayout.LabelField("Straight Movement", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(startPoint);
                EditorGUILayout.PropertyField(endPoint);
                break;

            case Mover.MovementType.Circular:
                //EditorGUILayout.LabelField("Circular Movement", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(center);
                EditorGUILayout.PropertyField(radius);
                EditorGUILayout.PropertyField(clockwise);
                EditorGUILayout.PropertyField(plane);
                EditorGUILayout.PropertyField(startAngle);
                break;

            case Mover.MovementType.FollowPoints:
                //EditorGUILayout.LabelField("Point Following", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(waypoints, includeChildren: true);
                break;
        }

        EditorGUILayout.Space(6);

        // Debug 
        //EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showPath);
        if (showPath.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(pathColor);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif