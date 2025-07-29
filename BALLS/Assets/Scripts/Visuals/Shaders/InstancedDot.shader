// Filename: InstancedDotShader.shader
Shader "Custom/InstancedDotShader"
{
    Properties {} // No properties needed here
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha One 
        ZWrite Off Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            // NEW: A global property for the wave speed, set from C# once per frame
            float _GlobalWaveSpeed;

            UNITY_INSTANCING_BUFFER_START(Props)
                // We now pass start and end values to let the shader do the lerping
                UNITY_DEFINE_INSTANCED_PROP(float4, _Data)         // x: startX, y: startY, z: spawnTime, w: lifetime
                UNITY_DEFINE_INSTANCED_PROP(float4, _Target)       // x: targetX, y: targetY
                UNITY_DEFINE_INSTANCED_PROP(float4, _StartParams)  // x: startAmp, y: startFreq, z: startScale
                UNITY_DEFINE_INSTANCED_PROP(float4, _EndParams)    // x: endAmp,   y: endFreq,   z: endScale
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);

                float4 data = UNITY_ACCESS_INSTANCED_PROP(Props, _Data);
                float4 target = UNITY_ACCESS_INSTANCED_PROP(Props, _Target);
                float4 startParams = UNITY_ACCESS_INSTANCED_PROP(Props, _StartParams);
                float4 endParams = UNITY_ACCESS_INSTANCED_PROP(Props, _EndParams);
                o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                // Calculate the dot's lifetime progress (0 to 1)
                float progress = saturate((_Time.y - data.z) / data.w);
                
                // Lerp all parameters based on progress
                float currentAmplitude = lerp(startParams.x, endParams.x, progress);
                float currentFrequency = lerp(startParams.y, endParams.y, progress);
                float currentScale = lerp(startParams.z, endParams.z, progress);
                
                // Base position and direction
                float3 startPos = float3(data.xy, 0);
                float3 endPos = float3(target.xy, 0);
                float3 basePos = lerp(startPos, endPos, progress);

                // Wave calculation using the lerped parameters and global wave speed
                float3 pathDir = normalize(endPos - startPos);
                float3 perpendicular = float3(-pathDir.y, pathDir.x, 0);
                float waveOffsetVal = currentAmplitude * sin(progress * currentFrequency * 6.28318 + _Time.y * _GlobalWaveSpeed);
                
                float3 finalPos = basePos + perpendicular * waveOffsetVal;
                
                o.vertex = UnityObjectToClipPos(float4(finalPos, 1.0));
                o.vertex.xy += v.vertex.xy * currentScale;
                
                o.uv = v.texcoord;
                // Fade out the overall alpha as the dot dies
                o.color.a *= (1.0 - progress); 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.uv, float2(0.5, 0.5));
                float circle = smoothstep(0.5, 0.48, dist);
                i.color.a *= circle;
                return i.color;
            }
            ENDCG
        }
    }
}