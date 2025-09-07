using UnityEngine;

public class DirtSpot : MonoBehaviour
{

    public RenderTexture dirtMask;
    public Texture2D brushTexture;
    public Material dirtMaterial;
    public float cleanThreshold = 0.95f;
    public Shader brushBlendShader;
    Material brushBlendMaterial;
    RenderTexture tempRT;
    [Header("Brush Settings")]
    public float setBrushWidth = 64f;
    public float setBrushHeight = 64f;

    private float brushWidth;
    private float brushHeight;


    void Start()
    {
        dirtMask = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        dirtMask.Create();

        Material matInstance = new Material(dirtMaterial);
        GetComponent<Renderer>().material = matInstance;
        matInstance.SetTexture("_MaskTex", dirtMask);

        brushBlendMaterial = new Material(brushBlendShader);
        //wouldn't this need to be later changed to a scalable variable? (the dimensions)
        tempRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);

        brushHeight = setBrushHeight / transform.localScale.y;
        brushWidth = setBrushWidth / transform.localScale.x;
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
        Vector3 localPos = transform.InverseTransformPoint(worldPos) + new Vector3(0.5f, 0, 0.5f);
        uv = new Vector2(localPos.x, localPos.z);

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
    }

    void Update()
    {
        CheckIfCleaned();
    }

    public void CheckIfCleaned()
    {
        Texture2D temp = new Texture2D(dirtMask.width, dirtMask.height, TextureFormat.RGB24, false);
        RenderTexture.active = dirtMask;
        temp.ReadPixels(new Rect(0, 0, dirtMask.width, dirtMask.height), 0, 0);
        temp.Apply();
        RenderTexture.active = null;

        Color[] pixels = temp.GetPixels();
        int cleanCount = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 0.9f)
            {
                cleanCount++;
            }
        }

        float cleanPercent = (float)cleanCount / pixels.Length;

        if (cleanPercent >= cleanThreshold)
        {
            Destroy(gameObject);
        }

        Destroy(temp);
    }
}

