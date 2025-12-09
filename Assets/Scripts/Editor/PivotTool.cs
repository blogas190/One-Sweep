using UnityEngine;
using UnityEditor;

public class PivotTool : EditorWindow
{
    private enum PivotPosition
    {
        Center,
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back,
        TopLeftFront,
        TopLeftBack,
        TopRightFront,
        TopRightBack,
        BottomLeftFront,
        BottomLeftBack,
        BottomRightFront,
        BottomRightBack
    }

    private PivotPosition pivotPos = PivotPosition.Center;
    private bool showPreview = true;
    private bool zeroRotation = true;

    [MenuItem("Tools/Pivot Tool")]
    public static void ShowWindow()
    {
        GetWindow<PivotTool>("Pivot Tool");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("Set Custom Pivot", EditorStyles.boldLabel);
        pivotPos = (PivotPosition)EditorGUILayout.EnumPopup("Pivot Position", pivotPos);
        showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);
        zeroRotation = EditorGUILayout.Toggle("Reset Pivot Rotation", zeroRotation);

        if (GUILayout.Button("Apply Pivot to Selected"))
        {
            foreach (GameObject obj in Selection.gameObjects)
                ApplyPivot(obj);
        }

        EditorGUILayout.HelpBox("This creates a parent object at the desired pivot point and re-parents the selected object under it.", MessageType.Info);

        if (showPreview && Selection.gameObjects.Length > 0)
        {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!showPreview || Selection.gameObjects.Length == 0)
            return;

        foreach (GameObject obj in Selection.gameObjects)
        {
            Vector3? pivotPoint = GetPivotPoint(obj);
            if (pivotPoint.HasValue)
            {
                // Draw preview sphere
                Handles.color = new Color(0, 1, 0, 0.8f);
                Handles.SphereHandleCap(0, pivotPoint.Value, Quaternion.identity, 0.1f, EventType.Repaint);

                // Draw label
                Handles.Label(pivotPoint.Value + Vector3.up * 0.15f, $"New Pivot: {pivotPos}", EditorStyles.whiteLabel);

                // Draw line from current position to new pivot
                Handles.color = new Color(1, 1, 0, 0.5f);
                Handles.DrawLine(obj.transform.position, pivotPoint.Value);

                // Draw coordinate axes at pivot point
                float axisLength = 0.3f;
                Handles.color = Color.red;
                Handles.DrawLine(pivotPoint.Value, pivotPoint.Value + obj.transform.right * axisLength);
                Handles.color = Color.green;
                Handles.DrawLine(pivotPoint.Value, pivotPoint.Value + obj.transform.up * axisLength);
                Handles.color = Color.blue;
                Handles.DrawLine(pivotPoint.Value, pivotPoint.Value + obj.transform.forward * axisLength);
            }
        }
    }

    Vector3? GetPivotPoint(GameObject obj)
    {
        // Check for MeshFilter or Renderer first
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        Renderer renderer = obj.GetComponent<Renderer>();

        if (meshFilter == null && renderer == null)
        {
            return null;
        }

        Bounds bounds;
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            bounds = meshFilter.sharedMesh.bounds;
        }
        else if (renderer != null)
        {
            // Use world bounds and convert to local space
            bounds = renderer.bounds;
            bounds.center = obj.transform.InverseTransformPoint(bounds.center);
            bounds.size = obj.transform.InverseTransformVector(bounds.size);
        }
        else
        {
            return null;
        }

        Vector3 targetPoint = bounds.center;

        // Compute pivot offset in local space
        switch (pivotPos)
        {
            case PivotPosition.Top: targetPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z); break;
            case PivotPosition.Bottom: targetPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z); break;
            case PivotPosition.Left: targetPoint = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z); break;
            case PivotPosition.Right: targetPoint = new Vector3(bounds.max.x, bounds.center.y, bounds.center.z); break;
            case PivotPosition.Front: targetPoint = new Vector3(bounds.center.x, bounds.center.y, bounds.max.z); break;
            case PivotPosition.Back: targetPoint = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z); break;
            case PivotPosition.TopLeftFront: targetPoint = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z); break;
            case PivotPosition.TopLeftBack: targetPoint = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z); break;
            case PivotPosition.TopRightFront: targetPoint = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z); break;
            case PivotPosition.TopRightBack: targetPoint = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z); break;
            case PivotPosition.BottomLeftFront: targetPoint = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z); break;
            case PivotPosition.BottomLeftBack: targetPoint = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z); break;
            case PivotPosition.BottomRightFront: targetPoint = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z); break;
            case PivotPosition.BottomRightBack: targetPoint = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z); break;
        }

        // Convert target point from local to world space
        return obj.transform.TransformPoint(targetPoint);
    }

    void ApplyPivot(GameObject obj)
    {
        Vector3? pivotPoint = GetPivotPoint(obj);

        if (!pivotPoint.HasValue)
        {
            Debug.LogError($"No MeshFilter or Renderer found on {obj.name}. Cannot determine bounds for pivot.");
            return;
        }

        Vector3 worldPivotPoint = pivotPoint.Value;

        // Store original parent and sibling index
        Transform originalParent = obj.transform.parent;
        int siblingIndex = obj.transform.GetSiblingIndex();

        // Create parent pivot object
        GameObject pivotParent = new GameObject(obj.name + "_Pivot");
        Undo.RegisterCreatedObjectUndo(pivotParent, "Create Pivot Parent");

        // Set pivot parent transform
        pivotParent.transform.position = worldPivotPoint;
        if (!zeroRotation)
        {
            pivotParent.transform.rotation = obj.transform.rotation;
        }    
        else
        {
            pivotParent.transform.rotation = new Quaternion(0,0,0,1);
        }
        pivotParent.transform.localScale = Vector3.one;

        // Set pivot parent's parent to original parent
        pivotParent.transform.SetParent(originalParent);
        pivotParent.transform.SetSiblingIndex(siblingIndex);

        // Re-parent the object under the pivot
        Undo.SetTransformParent(obj.transform, pivotParent.transform, "Reparent to Pivot");

        Debug.Log($"Pivot parent created for {obj.name} at {pivotPos}");
    }
}