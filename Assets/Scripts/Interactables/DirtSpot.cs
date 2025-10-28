using UnityEngine;
using System.Collections;

public class DirtSpot : MonoBehaviour
{
    public RenderTexture dirtMask;
    public Texture2D brushTexture;
    public Material dirtMaterial;
    public float cleanThreshold = 0.95f;
    public Shader brushBlendShader;

    [Header("Brush Settings")]
    public float setBrushWidth = 64f;
    public float setBrushHeight = 64f;

    [Header("UV Mapping Settings")]
    public bool flipUVX = false;
    public bool flipUVY = false;

    [Header("Performance Settings")]
    public float checkInterval = 0.5f;
    public int pixelSampleRate = 4;

    private Material brushBlendMaterial;
    private RenderTexture tempRT;
    private float brushWidth;
    private float brushHeight;

    // Performance optimization variables
    private Texture2D persistentTexture;
    private float lastCheckTime;
    private bool isDestroyed = false;

    // Progress tracking
    private float currentCleanPercentage = 0f;

    // Mesh bounds for proper UV mapping
    private Bounds localBounds;
    private MeshFilter meshFilter;

    void Start()
    {
        dirtMask = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        dirtMask.Create();

        Material matInstance = new Material(dirtMaterial);
        GetComponent<Renderer>().material = matInstance;
        matInstance.SetTexture("_MaskTex", dirtMask);

        brushBlendMaterial = new Material(brushBlendShader);
        tempRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);

        // Get mesh bounds for proper UV calculation
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            localBounds = meshFilter.sharedMesh.bounds;
        }
        else
        {
            // Fallback to default bounds
            localBounds = new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));
        }

        // Calculate brush size based on world space (same as original)
        brushWidth = setBrushWidth / (localBounds.size.x * transform.localScale.x);
        brushHeight = setBrushHeight / (localBounds.size.z * transform.localScale.z);

        persistentTexture = new Texture2D(dirtMask.width, dirtMask.height, TextureFormat.RGB24, false);

        // Register with the cleaning progress manager
        if (CleaningProgressManager.Instance != null)
        {
            CleaningProgressManager.Instance.RegisterDirtSpot(this);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 contactPoint = other.ClosestPointOnBounds(transform.position);
            CleanAtWorldPos(contactPoint);
        }
    }

    public void CleanAtWorldPos(Vector3 worldPos)
    {
        Vector2 uv;
        if (WorldPosToUV(worldPos, out uv))
        {
            DrawBrush(uv);
        }
    }

    bool WorldPosToUV(Vector3 worldPos, out Vector2 uv)
    {
        // Transform world position to local space
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Get mesh bounds min and max
        Vector3 boundsMin = localBounds.min;
        Vector3 boundsMax = localBounds.max;

        // Map local position to UV space [0,1]
        float uvX = Mathf.InverseLerp(boundsMin.x, boundsMax.x, localPos.x);
        float uvY = Mathf.InverseLerp(boundsMin.z, boundsMax.z, localPos.z);

        // Apply flipping based on inspector settings
        if (flipUVX) uvX = 1.0f - uvX;
        if (flipUVY) uvY = 1.0f - uvY;

        uv = new Vector2(uvX, uvY);

        // Check if UV is within valid range
        return (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1);
    }

    void DrawBrush(Vector2 uv)
    {
        // Same calculation as the original working version
        Vector4 brushUV = new Vector4(uv.x, uv.y, brushWidth / dirtMask.width, brushHeight / dirtMask.height);
        brushBlendMaterial.SetTexture("_MainTex", dirtMask);
        brushBlendMaterial.SetTexture("_BrushTex", brushTexture);
        brushBlendMaterial.SetVector("_BrushUV", brushUV);

        Graphics.Blit(dirtMask, tempRT, brushBlendMaterial);
        Graphics.Blit(tempRT, dirtMask);

        // Check immediately after cleaning
        StartCoroutine(CheckIfCleanedAsync());
    }

    void Update()
    {
        // Periodically check cleanliness to update progress
        if (Time.time - lastCheckTime > checkInterval)
        {
            StartCoroutine(CheckIfCleanedAsync());
        }
    }

    IEnumerator CheckIfCleanedAsync()
    {
        if (isDestroyed) yield break;

        lastCheckTime = Time.time;

        RenderTexture.active = dirtMask;
        persistentTexture.ReadPixels(new Rect(0, 0, dirtMask.width, dirtMask.height), 0, 0);
        persistentTexture.Apply();
        RenderTexture.active = null;

        yield return null;

        Color[] pixels = persistentTexture.GetPixels();
        int cleanCount = 0;
        int totalSamples = 0;

        for (int i = 0; i < pixels.Length; i += pixelSampleRate)
        {
            if (pixels[i].r > 0.9f)
            {
                cleanCount++;
            }
            totalSamples++;

            if (totalSamples % 1000 == 0)
            {
                yield return null;
            }
        }

        float cleanPercent = (float)cleanCount / totalSamples;
        currentCleanPercentage = cleanPercent;

        // Update the global progress manager
        if (CleaningProgressManager.Instance != null)
        {
            CleaningProgressManager.Instance.UpdateDirtSpotProgress(this, cleanPercent);
        }

        // Destroy if fully cleaned
        if (cleanPercent >= cleanThreshold && !isDestroyed)
        {
            isDestroyed = true;

            // Unregister from progress manager before destroying
            if (CleaningProgressManager.Instance != null)
            {
                CleaningProgressManager.Instance.UnregisterDirtSpot(this);
            }

            Destroy(gameObject);
        }
    }

    public float GetCleanPercentage()
    {
        return currentCleanPercentage;
    }

    void OnDestroy()
    {
        // Unregister from progress manager
        if (CleaningProgressManager.Instance != null)
        {
            CleaningProgressManager.Instance.UnregisterDirtSpot(this);
        }

        // Clean up resources
        if (persistentTexture != null)
        {
            Destroy(persistentTexture);
        }

        if (tempRT != null)
        {
            tempRT.Release();
            Destroy(tempRT);
        }

        if (dirtMask != null)
        {
            dirtMask.Release();
            Destroy(dirtMask);
        }
    }
}