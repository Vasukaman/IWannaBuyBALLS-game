// Filename: BallAppearance.shader
Shader "VFX/Ball Appearance"
{
    // TODO: [Organization] Many of these properties are driven by C# scripts. For properties
    // that are ALWAYS set by script, they can be removed from this block to declutter the
    // material inspector. They are kept here for debugging and clarity. - Gemini

    //SHUT UP ROBOT I HAVE HUMAN FINGERS I WANNA CHANGE SLIDERS WHEN TESTING.

    Properties
    {
        [Header(Main Ball Shape)]
        _Color ("Fill Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Radius ("UV-Space Radius", Range(0, 0.5)) = 0.45
        _OutlineThickness ("UV-Space Outline Thickness", Range(0, 0.2)) = 0.05
        _Smoothness ("Edge Smoothness", Range(0, 0.05)) = 0.01

        [Header(Orb Layers)]
        _OrbCount ("Tier 1 Orb Count", Int) = 3
        _OrbColor ("Tier 1 Orb Color", Color) = (1, 0.5, 0, 0.5)
        _OrbRadius ("Tier 1 Orb Radius", Range(0.001, 0.2)) = 0.04
        _OrbPathRadius ("Tier 1 Orb Path Radius", Range(0, 1)) = 0.3
        _OrbSpeed ("Tier 1 Orb Speed", Float) = 2.0
        _OrbCountTier2("Tier 2 Orb Count", Int) = 0
        _OrbColorTier2("Tier 2 Orb Color", Color) = (0.2, 0.8, 1, 0.7)
        _OrbRadiusTier2("Tier 2 Orb Radius", Range(0.001, 0.2)) = 0.08
        _OrbPathRadiusTier2("Orb Path Radius (Tier 2)", Range(0, 1)) = 0.15
        _OrbSpeedTier2("Tier 2 Orb Speed", Float) = -1.0

        [Header(Orb Path Line)]
        _ShowPathLine ("Show Path Line", Range(0, 1)) = 0
        _PathLineColor ("Path Line Color", Color) = (1, 1, 1, 0.2)
        _PathLineThickness("Path Line Thickness", Range(0.001, 0.1)) = 0.005

        [Header(Merge Effect)]
        _MergeTargetPos ("Merge Target Position (UV)", Vector) = (0,0,0,0)
        _MergeTargetRadius ("Merge Target Radius (UV)", Float) = 0.2
        _MergeWeight ("Merge Weight", Range(0, 1)) = 0

        [Header(Proximity Effect)]
        _ProximityBlendSharpness ("Proximity Blend Sharpness", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // --- Defines ---
            #define MAX_NEIGHBORS 10
            #define PI 3.14159265359

            // --- Structs ---
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0; // Renamed from localPos for convention
            };

            // --- Uniform Variables (set from C#) ---
            // Main Shape
            fixed4 _Color, _OutlineColor;
            float _Radius, _Smoothness, _OutlineThickness;

            // Orb Layers
            fixed4 _OrbColor, _OrbColorTier2, _PathLineColor;
            int _OrbCount, _OrbCountTier2;
            float _OrbRadius, _OrbPathRadius, _OrbSpeed;
            float _OrbRadiusTier2, _OrbPathRadiusTier2, _OrbSpeedTier2;
            float _ShowPathLine, _PathLineThickness;

            // Merge Effect
            float4 _MergeTargetPos;
            float _MergeTargetRadius, _MergeWeight;

            // Proximity Effect
            uniform float4 _ObjectWorldPos;
            uniform float _ObjectWorldRadius;
            uniform int _NeighborCount;
            uniform float4 _NeighborPositions[MAX_NEIGHBORS];
            uniform fixed4 _NeighborColors[MAX_NEIGHBORS];
            uniform float _ProximityBlendSharpness;

            // --- Vertex Shader ---
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Pass vertex position as UV coordinates (ranging from -0.5 to 0.5)
                o.uv = v.vertex.xy;
                return o;
            }

            // --- Helper Functions ---

            // Smooth Minimum: A function for blending two SDFs (signed distance fields) smoothly.
            // Used here for the metaball-like merge effect.
            float smin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }

            // Calculates the base shape of the ball, including the outline and merge effect.
            // Returns the signed distance from the edge.
            float getBaseShapeDistance(float2 uv)
            {
                float2 center = float2(0.0, 0.0);
                float baseDist = length(uv - center) - _Radius;

                // Apply merge effect if active
                float2 targetUV = _MergeTargetPos.xy - 0.5; // Convert from 0-1 range to -0.5 to 0.5
                float mergeTargetDist = length(uv - targetUV) - _MergeTargetRadius;
                float mergedDist = smin(baseDist, mergeTargetDist, 0.1);
                
                return lerp(baseDist, mergedDist, _MergeWeight);
            }

            // Draws a single layer of orbs and blends it with the incoming color.
            void drawOrbLayer(inout fixed3 finalColor, inout float totalOrbAlpha, float2 uv, int orbCount, float pathRadius, float orbRadius, float speed, fixed4 orbEffectColor)
            {
                if (orbCount <= 0) return;

                float2 center = float2(0.0, 0.0);
                for (int i = 0; i < orbCount; i++)
                {
                    float angle = ((float)i / (float)orbCount) * 2.0 * PI + _Time.y * speed;
                    float2 orbCenter = center + float2(cos(angle), sin(angle)) * pathRadius;
                    float orbDist = length(uv - orbCenter) - orbRadius;
                    float orbAlpha = smoothstep(_Smoothness, 0.0, orbDist);

                    // This logic ensures that overlapping orbs don't just add up,
                    // but the one with the highest alpha (closest to center) dominates.
                    if (orbAlpha > totalOrbAlpha)
                    {
                        totalOrbAlpha = orbAlpha;
                        finalColor = lerp(finalColor, orbEffectColor.rgb, totalOrbAlpha * orbEffectColor.a);
                    }
                }
            }

            // --- Fragment Shader ---
            fixed4 frag(v2f i) : SV_Target
            {
                // The UV here is in the range of -0.5 to 0.5, with (0,0) at the center.
                float2 uv = i.uv;

                // 1. --- CALCULATE BASE SHAPE AND OUTLINE ---
                float dist = getBaseShapeDistance(uv);
                float outlineAlpha = smoothstep(_Radius + _OutlineThickness, _Radius + _OutlineThickness - _Smoothness, dist);
                float fillAlpha = smoothstep(_Radius, _Radius - _Smoothness, dist);
                fixed3 finalColor = lerp(_OutlineColor.rgb, _Color.rgb, fillAlpha);

                // 2. --- APPLY PROXIMITY COLOR BLENDING ---
                // This logic remains complex because it operates in world space.
                for (int n = 0; n < _NeighborCount; n++)
                {
                    // This calculation is preserved exactly from the original to ensure scale is handled correctly.
                    float3 pixelWorldPos = _ObjectWorldPos.xyz + float3(i.uv.xy * _ObjectWorldRadius * 2.0, 0);
                    float3 neighborPos = _NeighborPositions[n].xyz;
                    float neighborRadius = _NeighborPositions[n].z;
                    float distToNeighbor = length(pixelWorldPos - neighborPos);
                    float normalizedDistance = saturate((distToNeighbor - neighborRadius) / _ObjectWorldRadius);
                    float influence = 1.0 - smoothstep(0.0, 1.0, normalizedDistance * _ProximityBlendSharpness);
                    finalColor = lerp(finalColor, _NeighborColors[n].rgb, influence * _NeighborColors[n].a);
                }

                // 3. --- DRAW ORB PATH LINE ---
                if (_ShowPathLine > 0.5)
                {
                    float pathDist = abs(length(uv) - _OrbPathRadius) - _PathLineThickness * 0.5;
                    float pathAlpha = smoothstep(_Smoothness * 0.5, 0.0, pathDist);
                    finalColor = lerp(finalColor, _PathLineColor.rgb, pathAlpha * _PathLineColor.a);
                }

                // 4. --- DRAW ORB LAYERS ---
                float totalOrbAlpha = 0.0;
                drawOrbLayer(finalColor, totalOrbAlpha, uv, _OrbCount, _OrbPathRadius, _OrbRadius, _OrbSpeed, _OrbColor);
                drawOrbLayer(finalColor, totalOrbAlpha, uv, _OrbCountTier2, _OrbPathRadiusTier2, _OrbRadiusTier2, _OrbSpeedTier2, _OrbColorTier2);
                
                // 5. --- FINAL COMPOSITION ---
                // The final alpha is determined by the main shape's outline.
                return fixed4(finalColor, outlineAlpha);
            }
            ENDCG
        }
    }
}