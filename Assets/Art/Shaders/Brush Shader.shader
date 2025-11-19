Shader "Custom/BrushBlend"
{
    Properties
    {
        _MainTex ("CurrentMask", 2D) = "white" {}
        _BrushTex ("Brush", 2D) = "white" {}
        _BrushUV ("BrushUV", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BrushTex;
            float4 _BrushUV;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                float mask = tex2D(_MainTex, uv).r;

                float2 brushUV = (uv - _BrushUV.xy) / _BrushUV.zw + 0.5;

                float brushAlpha = 0;
                if (brushUV.x >= 0 && brushUV.x <= 1 && brushUV.y >= 0 && brushUV.y <= 1)
                {
                    brushAlpha = tex2D(_BrushTex, brushUV).a;
                }

                float newMask = max(mask, brushAlpha);

                return fixed4(newMask, newMask, newMask, 1);
            }
            ENDCG
        }
    }
}
