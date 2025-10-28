Shader "Custom/DirtBlend_URP"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 0
        _CircleDensity ("Circle Density", Range(1, 20)) = 5
        _CircleSize ("Circle Size", Range(0.1, 2.0)) = 0.5
        _RandomSeed ("Random Seed", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBitangent : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
                half4 _EmissionColor;
                float _EmissionStrength;
                float _CircleDensity;
                float _CircleSize;
                float _RandomSeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                
                // Calculate tangent and bitangent for triplanar mapping
                float3 worldTangent = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.worldTangent = worldTangent;
                output.worldBitangent = cross(output.worldNormal, worldTangent) * input.tangentOS.w;
                
                return output;
            }

            // Simple hash function for pseudo-random numbers with seed support
            float hash(float2 p)
            {
                p += _RandomSeed;
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }
            
            // Generate dirt puddle pattern using world space coordinates
            float generateDirtPattern(float2 uv, float2 worldUV)
            {
                // Create border fade - circles won't spawn near edges (in UV space)
                float borderFade = 1.0;
                float edgeDistance = 0.15;
                
                float distFromLeft = smoothstep(0.0, edgeDistance, uv.x);
                float distFromRight = smoothstep(0.0, edgeDistance, 1.0 - uv.x);
                float distFromBottom = smoothstep(0.0, edgeDistance, uv.y);
                float distFromTop = smoothstep(0.0, edgeDistance, 1.0 - uv.y);
                
                borderFade = distFromLeft * distFromRight * distFromBottom * distFromTop;
                
                // Use world position scaled by density for consistent circle count
                float2 scaledUV = worldUV * _CircleDensity;
                float2 gridID = floor(scaledUV);
                float2 gridUV = frac(scaledUV);
                
                float minDist = 1.0;
                
                // Check neighboring cells for circle centers
                for(int y = -1; y <= 1; y++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cellID = gridID + neighbor;
                        
                        float2 randomOffset = float2(
                            hash(cellID),
                            hash(cellID + 100.0)
                        );
                        
                        float randomRadius = _CircleSize * (0.6 + hash(cellID + 200.0) * 0.8);
                        float2 circlePos = neighbor + randomOffset;
                        float dist = length(gridUV - circlePos);
                        
                        float edgeSoftness = 0.1;
                        float circle = smoothstep(randomRadius + edgeSoftness, randomRadius - edgeSoftness, dist);
                        
                        minDist = min(minDist, 1.0 - circle);
                    }
                }
                
                return (1.0 - minDist) * borderFade;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample the base texture
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Normalize the world normal
                float3 worldNormal = normalize(input.worldNormal);
                float3 absNormal = abs(worldNormal);
                
                // Determine which plane to project onto based on the dominant normal axis
                float2 worldUV;
                
                // Find the dominant axis and use appropriate world space coordinates
                if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
                {
                    // Horizontal surface (floor/ceiling) - use XZ plane
                    worldUV = input.worldPos.xz;
                }
                else if (absNormal.x > absNormal.z)
                {
                    // Vertical surface facing X - use YZ plane
                    worldUV = input.worldPos.yz;
                }
                else
                {
                    // Vertical surface facing Z - use XY plane
                    worldUV = input.worldPos.xy;
                }
                
                // Generate procedural dirt puddle pattern using appropriate world coordinates
                float dirtPattern = generateDirtPattern(input.uv, worldUV);
                
                // Sample the mask texture
                half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).r;
                
                // Combine procedural pattern with mask
                float finalMask = dirtPattern * (1.0 - mask);
                
                // Apply mask to alpha channel for transparency
                half4 finalColor = baseColor;
                finalColor.a = finalMask;
                
                // Add emission
                half3 emission = _EmissionColor.rgb * _EmissionStrength * finalMask;
                finalColor.rgb += emission;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}