using UnityEngine;
using System.Collections;

public class VerticalDirtSpot : MonoBehaviour
{
    public RenderTexture dirtMask;
    public Texture2D brushTexture;
    public Material dirtMaterial;
    public float cleanThreshold = 0.95f;
    public Shader brushBlendShader;

    [Header("Brush Settings")]
    public float setBrushWidth = 2000f;
    public float setBrushHeight = 2000f;

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

    void Start()
    {
        dirtMask = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        dirtMask.Create();

        Material matInstance = new Material(dirtMaterial);
        GetComponent<Renderer>().material = matInstance;
        matInstance.SetTexture("_MaskTex", dirtMask);

        brushBlendMaterial = new Material(brushBlendShader);
        tempRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);

        brushHeight = setBrushHeight / transform.localScale.y;
        brushWidth = setBrushWidth / transform.localScale.x;

        persistentTexture = new Texture2D(dirtMask.width, dirtMask.height, TextureFormat.RGB24, false);

        // Register with the cleaning progress manager
        if (CleaningProgressManager.Instance != null)
        {
            //CleaningProgressManager.Instance.RegisterDirtSpot(this);
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
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Determine surface orientation by checking transform's up vector
        Vector3 surfaceNormal = transform.up.normalized;

        localPos += new Vector3(0.5f, 0.5f, 0);
        uv = new Vector2(localPos.x, localPos.y);

        return (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1);
    }

    void DrawBrush(Vector2 uv)
    {
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

    // Asynchronous version to spread the work across multiple frames
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

    // Public method to get current cleaning percentage for this dirt spot
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