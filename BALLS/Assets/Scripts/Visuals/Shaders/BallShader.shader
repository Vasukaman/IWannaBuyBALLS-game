Shader "Custom/BallWithMetaballMerge"
{
    Properties
    {
        _Color ("Fill Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius", Range(0, 1)) = 0.45
        _Smoothness ("Edge Smoothness", Range(0, 1)) = 0.02
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 0.2)) = 0.05
        _MergeTargetPos ("Merge Target Position", Vector) = (0,0,0,0)
        _MergeTargetRadius ("Merge Target Radius", Float) = 0.2
        _MergeWeight ("Merge Weight", Range(0, 1)) = 0
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

            fixed4 _Color;
            float _Radius;
            float _Smoothness;
            fixed4 _OutlineColor;
            float _OutlineThickness;
            float4 _MergeTargetPos;
            float _MergeTargetRadius;
            float _MergeWeight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float smin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Current ball's center in UV space
                float2 center = float2(0.5, 0.5);
                
                // Calculate distance to current ball
                float d1 = length(i.uv - center) - _Radius;
                
                // Calculate distance to merge target (in this ball's UV space)
                float2 targetUV = float2(_MergeTargetPos.x, _MergeTargetPos.y);
                float d2 = length(i.uv - targetUV) - _MergeTargetRadius;
                
                // Combine distances with smooth min
                float mergedDist = smin(d1, d2, 0.1);
                
                // Blend between merged and original based on weight
                float dist = lerp(d1, mergedDist, _MergeWeight);
                
                // Outline mask
                float outerRadius = _Radius + _OutlineThickness;
                float outlineAlpha = smoothstep(outerRadius, outerRadius - _Smoothness, dist);
                
                // Inner fill mask
                float fillAlpha = smoothstep(_Radius, _Radius - _Smoothness, dist);
                
                // Combine colors
                fixed3 color = lerp(_OutlineColor.rgb, _Color.rgb, fillAlpha / max(outlineAlpha, 0.0001));
                
                return fixed4(color, outlineAlpha * _Color.a);
            }
            ENDCG
        }
    }
}