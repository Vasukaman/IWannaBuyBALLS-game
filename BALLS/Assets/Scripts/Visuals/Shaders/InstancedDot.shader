Shader "Custom/DotShader"
{
    Properties
    {
        _MainColor ("Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
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
            
            fixed4 _MainColor;
            float _GlowIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Circle with glow
                float circle = smoothstep(0.5, 0.45, dist);
                float glow = pow(1.0 - saturate(dist * 2.0), 2) * _GlowIntensity;
                
                fixed4 col = _MainColor;
                col.a = circle + glow;
                return col;
            }
            ENDCG
        }
    }
}