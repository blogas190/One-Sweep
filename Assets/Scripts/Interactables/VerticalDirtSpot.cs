using UnityEngine;

public class VerticalDirtSpot : DirtSpot
{
    protected override bool WorldPosToUV(Vector3 worldPos, out Vector2 uv)
    {
        // Convert world to local
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Vertical surfaces use X (width) and Y (height)
        float uvX = Mathf.InverseLerp(localBounds.min.x, localBounds.max.x, localPos.x);
        float uvY = Mathf.InverseLerp(localBounds.min.y, localBounds.max.y, localPos.y);

        // Apply flips inherited from base class
        if (flipUVX) uvX = 1f - uvX;
        if (flipUVY) uvY = 1f - uvY;

        uv = new Vector2(uvX, uvY);

        return uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;
    }
}