Shader "Custom/DirtBlendDecal_URP"
{
    // Renders a PNG decal (graffiti, sticker, etc.) on a surface.
    // The _MaskTex RenderTexture is painted by the player brush at runtime.
    // Where the mask is white (cleaned), the decal is cleanly erased.
    // No vertex displacement — this is a flat 2D decal.

    Properties
    {
        _DecalTex        ("Decal (PNG)",              2D)             = "white" {}
        _MaskTex         ("Mask Texture (RenderTex)", 2D)             = "black" {}

        [HDR] _EmissionColor   ("Emission Color", Color)              = (0,0,0,1)
        _EmissionStrength      ("Emission Strength", Range(0,10))     = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_DecalTex); SAMPLER(sampler_DecalTex);
            TEXTURE2D(_MaskTex);  SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _DecalTex_ST;
                float4 _MaskTex_ST;
                half4  _EmissionColor;
                float  _EmissionStrength;
            CBUFFER_END

            // -----------------------------------------------------------------
            // Vert — flat decal, no displacement needed
            // -----------------------------------------------------------------
            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv          = TRANSFORM_TEX(input.uv, _DecalTex);
                return o;
            }

            // -----------------------------------------------------------------
            // Frag
            // -----------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                half4 decalColor = SAMPLE_TEXTURE2D(_DecalTex, sampler_DecalTex, input.uv);
                half  cleaned    = SAMPLE_TEXTURE2D(_MaskTex,  sampler_MaskTex,  input.uv).r;

                // Directly use the mask to erase the decal — cleaned pixels are gone
                float visibility = 1.0 - cleaned;

                clip(visibility - 0.001);

                half4 finalColor  = decalColor;
                finalColor.a     *= visibility;
                finalColor.rgb   += _EmissionColor.rgb * _EmissionStrength * visibility;

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
