// To use this shader:
// 1. Create a new Shader in Unity (Create -> Shader -> Unlit Shader).
// 2. Name it something like "AdvancedVortex".
// 3. Copy and paste all of this code into the file.
// 4. Create a Material from this shader (Right-click the shader -> Create -> Material).
// 5. Assign the new Material to a Quad or Sprite in your scene.
// 6. Adjust the many properties on the Material in the Inspector to create your effect.

Shader "Custom/AdvancedVortex"
{
    Properties
    {
        [Header(Main Settings)]
        // NOTE: Radius is now relative to screen height. 0.5 means a radius half the screen's height.
        _Radius ("Effect Radius", Range(0.01, 1.0)) = 0.25
        _Center ("Vortex Center (UV Coords)", Vector) = (0.5, 0.5, 0, 0)

        [Header(Distortion Effects)]
        _PinchStrength ("Pinch Strength", Range(-1, 1)) = 0.2
        _TwirlStrength ("Twirl Strength", Range(-50, 50)) = 10.0

        [Header(Visual Effects)]
        _AberrationStrength ("Chromatic Aberration", Range(0, 0.1)) = 0.01
        _PixelationStrength ("Pixelation Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        GrabPass { "_GrabTexture" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 grabScreenPos : TEXCOORD0;
                float2 localUV : TEXCOORD1;
                float4 centerScreenPos : TEXCOORD2;
            };

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
            float _Radius;
            float2 _Center;
            float _PinchStrength;
            float _TwirlStrength;
            float _AberrationStrength;
            float _PixelationStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabScreenPos = ComputeGrabScreenPos(o.vertex);
                o.localUV = v.uv;
                
                // Correctly map the UV center coordinate to the object's local position.
                // A default Quad's local space is -0.5 to 0.5, so we must offset the 0-1 UV coordinate.
                float4 centerObjectPos = float4(_Center.x - 0.5, _Center.y - 0.5, 0.0, 1.0);
                o.centerScreenPos = ComputeScreenPos(UnityObjectToClipPos(centerObjectPos));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- 1. CALCULATE EFFECT INTENSITY IN SCREEN SPACE ---
                float aspect = _GrabTexture_TexelSize.w / _GrabTexture_TexelSize.z; // height / width

                float2 screenUV = i.grabScreenPos.xy / i.grabScreenPos.w;
                float2 centerScreenUV = i.centerScreenPos.xy / i.centerScreenPos.w;

                float2 screenDirection = screenUV - centerScreenUV;
                
                screenDirection.x *= aspect; 
                
                float distance = length(screenDirection);
                float falloff = 1.0 - saturate(distance / _Radius);
                falloff = pow(falloff, 2);

                if (falloff <= 0.001)
                {
                    return tex2Dproj(_GrabTexture, i.grabScreenPos);
                }

                // --- 2. CALCULATE GEOMETRIC DISTORTION OFFSET ---
                float newLength = max(0.0, distance - falloff * _PinchStrength * _Radius);
                float2 pinchedDirection = normalize(screenDirection) * newLength;

                float angle = falloff * _TwirlStrength;
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                float2 finalDirection = mul(rotationMatrix, pinchedDirection);
                
                float2 totalOffset = finalDirection - screenDirection;
                totalOffset.x /= aspect; // Un-correct aspect ratio for application.

                // --- 3. APPLY DISTORTIONS AND VISUAL EFFECTS ---
                float2 distortedUV = screenUV + totalOffset;

                float pixelBlockCount = lerp(256.0, 32.0, falloff * _PixelationStrength);
                float2 pixelatedUV = floor(distortedUV * pixelBlockCount) / pixelBlockCount;
                float2 finalUV = lerp(distortedUV, pixelatedUV, falloff * _PixelationStrength);

                // --- 4. APPLY CHROMATIC ABERRATION & SAMPLE TEXTURE ---
                float2 aberrationDirection = normalize(i.localUV - _Center);
                float aberrationAmount = falloff * _AberrationStrength;
                float2 aberrationOffset = aberrationDirection * aberrationAmount;
                
                // The aberration offset also needs to be corrected for aspect ratio
                aberrationOffset.x /= aspect;

                float2 uv_r = finalUV + aberrationOffset;
                float2 uv_g = finalUV;
                float2 uv_b = finalUV - aberrationOffset;

                // Final clamp to prevent any possible artifacting at extreme strengths.
                float4 screenPosR = i.grabScreenPos;
                screenPosR.xy = saturate(uv_r) * screenPosR.w;

                float4 screenPosG = i.grabScreenPos;
                screenPosG.xy = saturate(uv_g) * screenPosG.w;

                float4 screenPosB = i.grabScreenPos;
                screenPosB.xy = saturate(uv_b) * screenPosB.w;

                fixed r = tex2Dproj(_GrabTexture, screenPosR).r;
                fixed g = tex2Dproj(_GrabTexture, screenPosG).g;
                fixed b = tex2Dproj(_GrabTexture, screenPosB).b;

                return fixed4(r, g, b, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
