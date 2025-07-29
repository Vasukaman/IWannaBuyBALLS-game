// Shader "Custom/LiquidGateWithXRay"
Shader "Custom/LiquidGateWithXRay"
{
    Properties
    {
        [Header(Gate Properties)]
        _Color          ("Line Color", Color) = (0.2,0.6,1,1)
        _LineWidth      ("Line Width", Range(0,0.5)) = 0.3
        _BulgeSize      ("Bulge Size", Range(-1,1)) = 0.2
        _BulgePos       ("Bulge Position (X)", Range(0,1)) = 0.5
        _BulgeWidth     ("Bulge Width", Range(0.01,1)) = 0.2
        _BulgeSmooth    ("Bulge Smoothness", Range(0.001,0.3)) = 0.05
        _TopColor       ("Top Fill Color", Color) = (0,0,0,0.5)
        _BottomColor    ("Bottom Fill Color", Color) = (0,0,0,0.5)
        _BulgeAngle     ("Bulge Angle", Range(-180,180)) = 0
        _NoiseAmp       ("Noise Amplitude", Range(0,0.2)) = 0.05

        //[Header(X-Ray Effect)]
        _XRayTopEnable      ("Enable For Top Zone", Float) = 1
        _XRayColorTop       ("X-Ray Base Top", Color) = (0, 0.1, 0.2, 0.8)
        _HighlightColorTop  ("X-Ray Highlight Top", Color) = (0.5, 1, 1, 1)

        _XRayBottomEnable   ("Enable For Bottom Zone", Float) = 1
        _XRayColorBottom    ("X-Ray Base Bottom", Color) = (0.2, 0, 0.1, 0.8)
        _HighlightColorBottom("X-Ray Highlight Bottom", Color) = (1, 0.5, 0.8, 1)

        _EdgeSmoothness ("X-Ray Edge Smoothness", Range(0.001, 0.05)) = 0.01
        _HighlightPower ("X-Ray Highlight Power", Range(1, 10)) = 4.0
    }

    SubShader
    {
        // Render this *after* the standard transparent objects (like the balls themselves)
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Gate uniforms
            fixed4 _Color, _TopColor, _BottomColor;
            float  _LineWidth, _BulgeSize, _BulgePos, _BulgeWidth, _BulgeSmooth;
            float  _BulgeAngle, _NoiseAmp;

            // X-Ray uniforms
            float  _XRayTopEnable, _XRayBottomEnable;
            fixed4 _XRayColorTop, _HighlightColorTop;
            fixed4 _XRayColorBottom, _HighlightColorBottom;
            float  _EdgeSmoothness, _HighlightPower;
            
            // Ball data (sent from script)
            #define MAX_BALLS 32 // This MUST match the const in the C# script
            uniform float4 _BallData[MAX_BALLS];    // xy=UV pos, z=UV radius
            uniform int    _BallCount;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float bulgeTilt(float2 p, float centre, float size, float width, float angle)
            {
                float2 dir = float2(cos(angle * 0.0174533), sin(angle * 0.0174533));
                float d = abs(dot(p - float2(centre, 0.5), dir)) / max(width, 0.001);
                float h = 1.0 - smoothstep(0.0, 1.0, d);
                return size * h * h * (3.0 - 2.0 * h);
            }

            // Signed Distance Function for a circle
            float sdCircle(float2 p, float2 center, float radius)
            {
                return length(p - center) - radius;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. CALCULATE GATE POSITION for this pixel
                float topEdge = 0.5 + _LineWidth * 0.5;
                float botEdge = 0.5 - _LineWidth * 0.5;
                float tilt    = bulgeTilt(i.uv, _BulgePos, _BulgeSize, _BulgeWidth, _BulgeAngle);
                float noise   = (sin(i.uv.x * 20 + _Time.y * 5) + sin(i.uv.x * 35 + _Time.y * 7)) * _NoiseAmp;
                float offset  = tilt + noise;
                topEdge += offset;
                botEdge += offset;

                // 2. CHECK FOR X-RAY BALLS
                for (int j = 0; j < _BallCount; j++)
                {
                    float2 ballUVCenter = _BallData[j].xy;
                    float  ballUVRadius = _BallData[j].z;
                    
                    float dist = sdCircle(i.uv, ballUVCenter, ballUVRadius);

                    // Is this pixel inside the current ball?
                    if (dist < _EdgeSmoothness) 
                    {
                        // Determine which zone the PIXEL (not the ball center) is in
                        bool isPixelInTopZone = i.uv.y > topEdge;
                        bool isPixelInBotZone = i.uv.y < botEdge;

                        // Check if X-Ray is enabled for the pixel's zone
                        bool applyTopXRay = _XRayTopEnable > 0.5 && isPixelInTopZone;
                        bool applyBotXRay = _XRayBottomEnable > 0.5 && isPixelInBotZone;

                        if (applyTopXRay || applyBotXRay)
                        {
                            // This pixel is part of a ball in an active X-Ray zone. Draw it.
                            fixed4 baseColor      = applyTopXRay ? _XRayColorTop : _XRayColorBottom;
                            fixed4 highlightColor = applyTopXRay ? _HighlightColorTop : _HighlightColorBottom;
                            
                            // Create a soft glow effect from the center outwards
                            float glow = 1.0 - saturate(abs(dist) / ballUVRadius);
                            fixed3 finalColor = lerp(baseColor.rgb, highlightColor.rgb, pow(glow, _HighlightPower));
                            
                            // Anti-alias the edge of the circle
                            float alpha = smoothstep(_EdgeSmoothness, -_EdgeSmoothness, dist);
                            return fixed4(finalColor, alpha * baseColor.a);
                        }
                        else
                        {
                            // This pixel is part of a ball, but NOT in an active X-Ray zone.
                            // Discard it, making it transparent so the original ball object can be seen.
                            discard;
                        }
                    }
                }

                // 3. RENDER GATE (This code only runs if the pixel was not discarded or returned)
                if (i.uv.y > topEdge) return _TopColor;
                if (i.uv.y < botEdge) return _BottomColor;

                float d = max(i.uv.y - topEdge, botEdge - i.uv.y);
                float alpha = smoothstep(_BulgeSmooth, -_BulgeSmooth, d);
                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}