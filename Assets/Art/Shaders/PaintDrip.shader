Shader "Custom/PaintDrip"
{
    Properties
    {
        _PaintColor    ("Paint Color",    Color)        = (1, 0, 0, 1)
        _LineCount     ("Line Count",     Float)        = 6
        _LineWidth     ("Line Width",     Float)        = 0.02
        _MinLength     ("Min Length",     Float)        = 0.3
        _MaxLength     ("Max Length",     Float)        = 0.9
        _TaperStrength ("Taper Strength", Range(0, 1)) = 1.0
        _Seed          ("Seed",           Float)        = 1.0
        _FillAmount    ("Fill Amount",    Float)        = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            float4 _PaintColor;
            float  _LineCount;
            float  _LineWidth;
            float  _MinLength;
            float  _MaxLength;
            float  _TaperStrength;
            float  _Seed;
            float  _FillAmount;
            float4 _ClipRect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 color    : COLOR;
            };

            float hash(float a, float b)
            {
                return frac(sin(dot(float2(a, b), float2(127.1, 311.7))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex;
                o.uv       = v.uv;
                o.color    = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv    = i.uv;
                float  alpha = 0.0;

                for (int d = 0; d < (int)_LineCount; d++)
                {
                    float lineX      = hash(d, _Seed);
                    float lineLength = lerp(_MinLength, _MaxLength, hash(d, _Seed + 1.0));
                    float lineWidth  = _LineWidth * (hash(d, _Seed + 2.0) * 0.5 + 0.75);

                    if (lineX > _FillAmount) continue;

                    // taper: wide at top, narrow at bottom
                    float taper        = 1.0 - (uv.y / lineLength) * _TaperStrength;
                    float taperedWidth = lineWidth * max(taper, 0.01);

                    float distX    = abs(uv.x - lineX);
                    float inLine   = distX < taperedWidth && uv.y < lineLength;
                    float edgeFade = 1.0 - smoothstep(taperedWidth * 0.5, taperedWidth, distX);

                    // round tip at bottom using original lineWidth so it doesn't vanish
                    float tipY    = lineLength;
                    float tipDist = length(float2((uv.x - lineX) / lineWidth, (uv.y - tipY) * 3.0));
                    float tip     = 1.0 - smoothstep(0.8, 1.0, tipDist);

                    float lineMask = (inLine ? edgeFade : 0.0) + tip;
                    alpha = max(alpha, saturate(lineMask));
                }

                fixed4 col = float4(_PaintColor.rgb, alpha * _PaintColor.a);
                col.a     *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                col        *= i.color;

                clip(col.a - 0.01);
                return col;
            }
            ENDCG
        }
    }
}