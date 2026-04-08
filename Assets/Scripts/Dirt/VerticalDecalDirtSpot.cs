using UnityEngine;

/// <summary>
/// Vertical variant of DecalDirtSpot — use this for decals placed on walls,
/// doors, or any surface whose dominant axis is Y (height) rather than Z (depth).
///
/// Inherits from VerticalDirtSpot so UV mapping and brush-size calculation
/// both use X/Y instead of X/Z. The decal texture is set the same way as
/// DecalDirtSpot via OnDirtSpotInitialized().
///
/// Setup is identical to DecalDirtSpot — just swap the component.
/// </summary>
public class VerticalDecalDirtSpot : VerticalDirtSpot
{
    [Header("Decal")]
    [Tooltip("The PNG to display as dirt on a vertical surface (graffiti on a wall, etc.). " +
             "Transparent areas are already invisible; the player brush erases the rest.")]
    public Texture2D decalTexture;

    protected override void OnDirtSpotInitialized()
    {
        if (decalTexture == null)
        {
            Debug.LogWarning($"[VerticalDecalDirtSpot] '{name}' has no decal texture assigned.", this);
            return;
        }

        GetComponent<Renderer>().material.SetTexture("_DecalTex", decalTexture);
    }
}
