Shader "Custom/BallWithInternalOrbs"
{
    Properties
    {
        [Header(Main Ball)]
        _Color ("Fill Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius", Range(0, 1)) = 0.45
        _Smoothness ("Edge Smoothness", Range(0, 0.05)) = 0.01
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 0.2)) = 0.05

        [Header(Internal Orbs)]
        _OrbCount ("Orb Count", Int) = 3
        _OrbColor ("Orb Color", Color) = (1, 0.5, 0, 0.5)
        _OrbRadius ("Orb Radius", Range(0.001, 0.2)) = 0.04
        _OrbPathRadius ("Orb Path Radius", Range(0, 1)) = 0.3
        _OrbSpeed ("Orb Speed", Float) = 2.0

        [Header(Orb Path Line)]
        _ShowPathLine ("Show Path Line", Range(0, 1)) = 0
        _PathLineColor ("Path Line Color", Color) = (1, 1, 1, 0.2)
        _PathLineThickness("Path Line Thickness", Range(0.001, 0.1)) = 0.005

        [Header(Metaball Merge Effect)]
        _MergeTargetPos ("Merge Target Position (UV)", Vector) = (0,0,0,0)
        _MergeTargetRadius ("Merge Target Radius", Float) = 0.2
        _MergeWeight ("Merge Weight", Range(0, 1)) = 0

        [Header(Proximity Effect)]
        _ProximityBlendSharpness ("Proximity Blend Sharpness", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_NEIGHBORS 10

            // --- Properties ---
            fixed4 _Color, _OutlineColor, _OrbColor, _PathLineColor;
            float _Radius, _Smoothness, _OutlineThickness, _OrbRadius, _OrbPathRadius, _OrbSpeed;
            float _ShowPathLine, _PathLineThickness, _MergeTargetRadius, _MergeWeight;
            int _OrbCount;
            float4 _MergeTargetPos;

            // --- This Ball's Properties (NEW) ---
            uniform float4 _ObjectWorldPos;
            uniform float _ObjectWorldRadius;

            // --- Neighbor Properties ---
            uniform int _NeighborCount;
            uniform float4 _NeighborPositions[MAX_NEIGHBORS];
            uniform fixed4 _NeighborColors[MAX_NEIGHBORS];
            uniform float _ProximityBlendSharpness;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 localPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xy;
                // We no longer pass the rotating worldPos
                return o;
            }

            float smin(float a, float b, float k) {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }

            float applyMergeEffect(float originalDist, float2 uv) {
                float2 targetUV = _MergeTargetPos.xy;
                float d2 = length(uv - targetUV) - _MergeTargetRadius;
                float mergedDist = smin(originalDist, d2, 0.1);
                return lerp(originalDist, mergedDist, _MergeWeight);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.localPos + 0.5;
                float2 center = float2(0.5, 0.5);
                
                // 1. --- BASE SHAPE & COLOR ---
                float d1 = length(uv - center) - _Radius;
                float dist = applyMergeEffect(d1, uv);
                float outlineAlpha = smoothstep(_Radius + _OutlineThickness, _Radius + _OutlineThickness - _Smoothness, dist);
                float fillAlpha = smoothstep(_Radius, _Radius - _Smoothness, dist);
                fixed3 finalColor = lerp(_OutlineColor.rgb, _Color.rgb, fillAlpha);

                // 2. --- PROXIMITY COLOR BLENDING (CORRECTED) ---
                for (int n = 0; n < _NeighborCount; n++)
                {
                    // Reconstruct this pixel's world position without rotation
                    float3 pixelWorldPos = _ObjectWorldPos.xyz + float3(i.localPos.xy * _ObjectWorldRadius * 2.0, 0);

                    float3 neighborPos = _NeighborPositions[n].xyz;
                    float neighborRadius = _NeighborPositions[n].z;
                    
                    float distToNeighbor = length(pixelWorldPos - neighborPos);
                    
                    // Normalize distance based on this ball's world radius
                    float normalizedDistance = saturate((distToNeighbor - neighborRadius) / _ObjectWorldRadius);
                    
                    float influence = 1.0 - smoothstep(0.0, 1.0, normalizedDistance * _ProximityBlendSharpness);
                    
                    finalColor = lerp(finalColor, _NeighborColors[n].rgb, influence * _NeighborColors[n].a);
                }

                // 3. --- ORB PATH LINE ---
                if (_ShowPathLine > 0.5) {
                    float pathDist = abs(length(uv - center) - _OrbPathRadius) - _PathLineThickness * 0.5;
                    float pathAlpha = smoothstep(_Smoothness * 0.5, 0.0, pathDist);
                    finalColor = lerp(finalColor, _PathLineColor.rgb, pathAlpha * _PathLineColor.a);
                }

                // 4. --- INTERNAL ORBS ---
                float totalOrbAlpha = 0.0;
                for (int j = 0; j < _OrbCount; j++) {
                    float angle = ((float)j / (float)_OrbCount) * 2.0 * 3.14159 + _Time.y * _OrbSpeed;
                    float2 orbCenter = center + float2(cos(angle), sin(angle)) * _OrbPathRadius;
                    float orbDist = length(uv - orbCenter) - _OrbRadius;
                    float orbAlpha = smoothstep(_Smoothness, 0.0, orbDist);
                    totalOrbAlpha = max(totalOrbAlpha, orbAlpha);
                }
                finalColor = lerp(finalColor, _OrbColor.rgb, totalOrbAlpha * _OrbColor.a);
                
                // 5. --- FINAL COMPOSITION ---
                return fixed4(finalColor, outlineAlpha);
            }
            ENDCG
        }
    }
}