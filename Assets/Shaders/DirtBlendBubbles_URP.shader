Shader "Custom/DirtBlendBubbles_URP"
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
        _DomeHeight ("Dome Height (Shading)", Range(0, 1)) = 0.3
        _DomeSpecular ("Dome Specular", Range(0, 1)) = 0.5
        _DomeSmoothness ("Dome Smoothness", Range(0, 1)) = 0.6
        // Property to control the actual physical bubble height
        _DisplacementHeight ("Displacement Height (Meters)", Range(0, 0.1)) = 0.01
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
            ZWrite On // Keep ZWrite On for displaced geometry
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float2 uv : TEXCOORD0;                 // For texture sampling
                float3 worldPos : TEXCOORD1;           // Displaced world position (for lighting)
                float3 worldNormal : TEXCOORD2;        // Original normal
                float3 worldTangent : TEXCOORD3;       // Original tangent
                float3 worldBitangent : TEXCOORD4;     // Original bitangent
                float  bubbleDisplacement : TEXCOORD5; // ONLY for vertex displacement amount
                float2 meshUV : TEXCOORD6;             // NEW: Original, untransformed mesh UV
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
                float _DomeHeight;
                float _DomeSpecular;
                float _DomeSmoothness;
                float _DisplacementHeight;
            CBUFFER_END

            float hash(float2 p)
            {
                p += _RandomSeed;
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }
            
            struct DirtResult
            {
                float pattern;
                float3 domeNormal;
                float displacementFactor; 
            };
            
            // MODIFIED: This function now uses UV coordinates instead of world position
            // This locks the pattern to the quad's UVs and allows clipping.
            DirtResult generateDirtPattern(float2 uv, float3 worldNormal, float3 worldTangent, float3 worldBitangent)
            {
                DirtResult result;
                result.pattern = 0.0;
                result.domeNormal = worldNormal;
                result.displacementFactor = 0.0;
                
                // Use the quad's UV coordinates as the base position
                float2 surfacePos = uv;
                
                float2 scaledUV = surfacePos * _CircleDensity;
                float2 gridID = floor(scaledUV);
                float2 gridUV = frac(scaledUV);
                
                float minDist = 1.0;
                float2 closestCircleOffset = float2(0, 0);
                float closestRadius = 0.0;
                bool foundCircle = false;
                
                // Iterates over neighboring grid cells to find the closest circle center
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
                        
                        // --- NEW CHECK to see if bubble is fully inside ---
                        // 1. Calculate the circle center's position in the [0, 1] UV space
                        float2 circleCenterUV = (cellID + randomOffset) / _CircleDensity;
                        
                        // 2. Calculate the radius in [0, 1] UV space
                        // Note: randomRadius is in grid space (approx 0.0-1.0), so divide by density
                        float radiusInUVSpace = randomRadius / _CircleDensity;
                        
                        // 3. Check if the full circle (center + radius) is inside the 0.0 to 1.0 range
                        bool isFullyInside = 
                            circleCenterUV.x - radiusInUVSpace > 0.0 &&
                            circleCenterUV.x + radiusInUVSpace < 1.0 &&
                            circleCenterUV.y - radiusInUVSpace > 0.0 &&
                            circleCenterUV.y + radiusInUVSpace < 1.0;
                        // ------------------------------------------------
                        
                        float edgeSoftness = 0.1;
                        float circle = smoothstep(randomRadius + edgeSoftness, randomRadius - edgeSoftness, dist);
                        
                        // This pixel is part of a circle, AND that circle is fully inside the quad
                        if (circle > 0.0 && dist < minDist && isFullyInside)
                        {
                            minDist = dist;
                            closestCircleOffset = gridUV - circlePos;
                            closestRadius = randomRadius;
                            foundCircle = true;
                        }
                    }
                }
                
                // We now have the closest circle *that is fully inside the quad*
                if (foundCircle)
                {
                    // Recalculate pattern based *only* on the found circle
                    result.pattern = smoothstep(closestRadius + 0.1, closestRadius - 0.1, minDist);
                    
                    float normalizedDist = minDist / closestRadius;
                    // Calculate the parabolic height (0 at edge, 1 at center)
                    float height = sqrt(max(0.0, 1.0 - normalizedDist * normalizedDist));
                    
                    // Assign the height (0 to 1) for both displacement and alpha mask
                    result.displacementFactor = height * result.pattern;
                    
                    // Calculate the shading normal (using _DomeHeight for the normal map effect)
                    float3 tangentNormal = normalize(float3(
                        -closestCircleOffset.x / closestRadius,
                        -closestCircleOffset.y / closestRadius,
                        height * _DomeHeight
                    ));
                    
                    result.domeNormal = normalize(
                        tangentNormal.x * worldTangent +
                        tangentNormal.y * worldBitangent +
                        tangentNormal.z * worldNormal
                    );
                }
                // If no circle is found (or the closest one is outside), 
                // result.pattern and result.displacementFactor remain 0.0
                
                return result;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 1. Calculate basis vectors for world space
                float3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
                float3 worldTangent = TransformObjectToWorldDir(input.tangentOS.xyz);
                float3 worldBitangent = cross(worldNormal, worldTangent) * input.tangentOS.w;
                
                // --- PASS ORIGINAL DATA TO FRAGMENT ---
                output.worldNormal = worldNormal;
                output.worldTangent = worldTangent;
                output.worldBitangent = worldBitangent;
                output.meshUV = input.uv; // Pass raw mesh UV
                // ----------------------------------------

                // 2. Calculate displacement factor at the vertex using its mesh UV
                DirtResult vert_result = generateDirtPattern(
                    input.uv, // Use mesh UV
                    worldNormal,
                    worldTangent, 
                    worldBitangent
                );
                
                // 3. Apply Vertex Displacement
                float displacement = vert_result.displacementFactor * _DisplacementHeight;
                input.positionOS.xyz += input.normalOS * displacement;
                
                // 4. Populate Varyings struct
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex); // For main texture
                
                // Pass the *displaced* world position (for lighting)
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                output.bubbleDisplacement = vert_result.displacementFactor; 
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Re-calculate the high-resolution pattern using the interpolated mesh UV
                DirtResult frag_result = generateDirtPattern(
                    input.meshUV, // USE THE STABLE MESH UV
                    input.worldNormal, 
                    input.worldTangent, 
                    input.worldBitangent
                );
                
                // Use input.uv for texture sampling (as it has _MainTex_ST applied)
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).r;
                
                // Use the new, pixel-accurate displacementFactor for the final alpha mask
                float finalMask = frag_result.displacementFactor * (1.0 - mask);
                
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                // Use the DISPLACED worldPos for view direction (correct perspective)
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.worldPos); 
                
                // Use the pixel-accurate domeNormal for lighting
                float3 domeNormal = frag_result.domeNormal;

                float NdotL = saturate(dot(domeNormal, lightDir));
                float3 diffuse = mainLight.color * NdotL;
                
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(domeNormal, halfDir));
                float specPower = exp2(10 * _DomeSmoothness + 1);
                float3 specular = mainLight.color * pow(NdotH, specPower) * _DomeSpecular;
                
                half3 litColor = baseColor.rgb * (diffuse + 0.3);
                litColor += specular * finalMask;
                
                half4 finalColor = half4(litColor, finalMask);
                
                half3 emission = _EmissionColor.rgb * _EmissionStrength * finalMask;
                finalColor.rgb += emission;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

