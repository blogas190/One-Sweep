using UnityEngine;
using System.Collections;

public class DirtSpot : MonoBehaviour
{
    private RenderTexture dirtMask;
    public Texture2D brushTexture;
    public Material dirtMaterial;
    public float cleanThreshold = 0.95f;
    public Shader brushBlendShader;

    [Header("Brush Settings")]
    public float setBrushWidth = 64f;
    public float setBrushHeight = 64f;
    private float lastBrushTime;
    public float brushInterval = 0.02f;

    [Header("UV Mapping Settings")]
    public bool flipUVX = false;
    public bool flipUVY = false;

    [Header("Performance Settings")]
    public float checkInterval = 0.5f;
    public int pixelSampleRate = 4;

    private Material brushBlendMaterial;
    private RenderTexture tempRT;
    protected float brushWidth;
    protected float brushHeight;

    private Texture2D persistentTexture;
    protected bool isDestroyed = false;
    protected float currentCleanPercentage = 0f;
    protected Bounds localBounds;
    protected MeshFilter meshFilter;

    protected float lastCheckTime;
    private bool isChecking = false;
    private float cleaningUntil;

    void Start()
    {
        dirtMask = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
        dirtMask.Create();
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = dirtMask;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = previous;

        Material matInstance = new Material(dirtMaterial);
        GetComponent<Renderer>().material = matInstance;
        matInstance.SetTexture("_MaskTex", dirtMask);

        brushBlendMaterial = new Material(brushBlendShader);
        tempRT = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);

        meshFilter = GetComponent<MeshFilter>();
        localBounds = (meshFilter != null && meshFilter.sharedMesh != null)
            ? meshFilter.sharedMesh.bounds
            : new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

        CalculateBrushSize();

        persistentTexture = new Texture2D(dirtMask.width, dirtMask.height, TextureFormat.RGBA32, false);

        if (CleaningProgressManager.Instance != null)
            CleaningProgressManager.Instance.RegisterDirtSpot(this);

        OnDirtSpotInitialized();
    }

    // Subclasses override this to run setup that needs the material instance
    // and RenderTexture to already exist (both created above in Start).
    protected virtual void OnDirtSpotInitialized() { }

    public void CleanAtWorldPos(Vector3 worldPos)
    {
        if (WorldPosToUV(worldPos, out Vector2 uv))
            DrawBrush(uv);
    }

    protected virtual void CalculateBrushSize()
    {
        brushWidth = setBrushWidth / (localBounds.size.x * transform.lossyScale.x);
        brushHeight = setBrushHeight / (localBounds.size.z * transform.lossyScale.z);
    }

    protected virtual bool WorldPosToUV(Vector3 worldPos, out Vector2 uv)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Vector3 boundsMin = localBounds.min;
        Vector3 boundsMax = localBounds.max;

        float uvX = Mathf.InverseLerp(boundsMin.x, boundsMax.x, localPos.x);
        float uvY = Mathf.InverseLerp(boundsMin.z, boundsMax.z, localPos.z);

        if (flipUVX) uvX = 1.0f - uvX;
        if (flipUVY) uvY = 1.0f - uvY;

        uv = new Vector2(uvX, uvY);
        return (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1);
    }

    void DrawBrush(Vector2 uv)
    {
        if (Time.time - lastBrushTime < brushInterval) return;
        lastBrushTime = Time.time;
        cleaningUntil = Time.time + 0.2f;

        Vector4 brushUV = new Vector4(uv.x, uv.y, brushWidth / dirtMask.width, brushHeight / dirtMask.height);
        brushBlendMaterial.SetTexture("_MainTex", dirtMask);
        brushBlendMaterial.SetTexture("_BrushTex", brushTexture);
        brushBlendMaterial.SetVector("_BrushUV", brushUV);

        Graphics.Blit(dirtMask, tempRT, brushBlendMaterial);
        Graphics.Blit(tempRT, dirtMask);
    }

    void Update()
    {
        // FIX: isChecking flag ensures only one coroutine runs at a time
        if (!isChecking && Time.time - lastCheckTime > checkInterval)
            StartCoroutine(CheckIfCleanedAsync());
    }

    IEnumerator CheckIfCleanedAsync()
    {
        if (isDestroyed) yield break;

        isChecking = true;           // lock
        lastCheckTime = Time.time;

        // GPU to CPU readback
        RenderTexture.active = dirtMask;
        persistentTexture.ReadPixels(new Rect(0, 0, dirtMask.width, dirtMask.height), 0, 0);
        persistentTexture.Apply();
        RenderTexture.active = null;

        yield return null;

        // FIX: GetPixels32 returns bytes instead of floats faster + less GC
        Color32[] pixels = persistentTexture.GetPixels32();
        int cleanCount = 0;
        int totalSamples = 0;

        for (int i = 0; i < pixels.Length; i += pixelSampleRate)
        {
            if (pixels[i].r > 229) // 0.9f * 255 ≈ 229
                cleanCount++;

            totalSamples++;

            if (totalSamples % 1000 == 0)
                yield return null;
        }

        float cleanPercent = (float)cleanCount / totalSamples;
        currentCleanPercentage = cleanPercent;

        if (CleaningProgressManager.Instance != null)
            CleaningProgressManager.Instance.UpdateDirtSpotProgress(this, cleanPercent);

        if (cleanPercent >= cleanThreshold && !isDestroyed)
        {
            isDestroyed = true;

            if (CleaningProgressManager.Instance != null)
                CleaningProgressManager.Instance.UnregisterDirtSpot(this);

            Destroy(gameObject);
        }

        isChecking = false;          // unlock
    }

    public float GetCleanPercentage() => currentCleanPercentage;

    void OnDestroy()
    {
        if (CleaningProgressManager.Instance != null)
            CleaningProgressManager.Instance.UnregisterDirtSpot(this);

        if (persistentTexture != null) Destroy(persistentTexture);
        if (tempRT != null) { tempRT.Release(); Destroy(tempRT); }
        if (dirtMask != null) { dirtMask.Release(); Destroy(dirtMask); }
    }

    public bool IsBeingCleaned()
    {
        return Time.time < cleaningUntil;
    }
}