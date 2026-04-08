using UnityEngine;

/// <summary>
/// Drop-in replacement for DirtSpot that displays a PNG decal (graffiti,
/// sticker, mud splat…) and erases it with a bubble-pop dissolve edge as
/// the player brushes over it.
///
/// Setup
///   1. Add this component to a quad placed over the dirty surface.
///   2. Assign a material that uses the DirtBlendDecalBubbles_URP shader.
///   3. Drag your PNG into the Decal Texture slot.
///   4. All brush painting, clean-percentage tracking and progress manager
///      registration is handled by the parent DirtSpot — nothing else needed.
///
/// Note: DirtSpot.cs must expose the protected virtual OnDirtSpotInitialized()
/// hook (added alongside this file) so we can set the decal after the material
/// instance has been created by the parent's Start().
/// </summary>
public class DecalDirtSpot : DirtSpot
{
    [Header("Decal")]
    [Tooltip("The PNG to display as dirt. Transparent areas are already " +
             "invisible; the player brush erases the rest with a bubble edge.")]
    public Texture2D decalTexture;

    protected override void OnDirtSpotInitialized()
    {
        if (decalTexture == null)
        {
            Debug.LogWarning($"[DecalDirtSpot] '{name}' has no decal texture assigned.", this);
            return;
        }

        // The parent's Start() has already cloned the shared material into a
        // per-instance material, so writing here is safe and won't affect
        // other objects using the same base material.
        GetComponent<Renderer>().material.SetTexture("_DecalTex", decalTexture);
    }
}
