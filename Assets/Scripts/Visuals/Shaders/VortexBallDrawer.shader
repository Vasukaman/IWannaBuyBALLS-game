// Shader "Custom/VortexBallDrawer"
Shader "Custom/VortexBallDrawer"
{
    Properties
    {
        [Header(Vortex Ball Properties)]
        _BallOutlineColor ("Ball Outline Color", Color) = (1,1,1,1)
        _BallOutlineWidth ("Ball Outline Width", Range(0.0, 0.5)) = 0.1
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.1)) = 0.01
        
        [Header(Distortion)]
        _PinchPower ("Pinch Power", Range(0, 5)) = 2.0
        _ScalePower ("Scale Power", Range(0, 5)) = 1.5
    }

    SubShader
    {
        // Render after standard transparent objects
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Vortex Ball uniforms
            fixed4 _BallOutlineColor;
            float _BallOutlineWidth, _EdgeSmoothness;
            float _PinchPower, _ScalePower;

            // Ball data from script
            #define MAX_BALLS 32
            uniform float4 _BallData[MAX_BALLS];    // xy=UV pos, z=UV radius
            uniform float4 _BallColors[MAX_BALLS];  // Original color of the ball
            uniform int _BallCount;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Signed Distance Function for a circle
            float sdCircle(float2 p, float2 center, float radius)
            {
                return length(p - center) - radius;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 finalColor = fixed4(0,0,0,0);
                
                // Center the pixel's UV coordinate (0,0 is now the center)
                float2 p = i.uv - 0.5;

                // Loop through all balls sent from the script
                for (int j = 0; j < _BallCount; j++)
                {
                    // Get ball data, converting its center to our (-0.5, 0.5) space
                    float2 ballUVCenter = _BallData[j].xy - 0.5;
                    float ballUVRadius = _BallData[j].z;
                    fixed4 ballFillColor = _BallColors[j];

                    // 1. DISTORTION
                    // Get direction and distance from the vortex center (0,0) to the ball's center
                    float distFromCenter = length(ballUVCenter);
                    float2 dirToCenter = normalize(ballUVCenter);

                    // Scale the ball's radius down as it gets closer to the center
                    float scaleFactor = pow(distFromCenter, _ScalePower);
                    float scaledRadius = ballUVRadius * scaleFactor;

                    // Pinch the ball's position towards the center
                    float pinchOffset = pow(1.0 - saturate(distFromCenter * 2.0), _PinchPower) * ballUVRadius;
                    float2 pinchedCenter = ballUVCenter - dirToCenter * pinchOffset;
                    
                    // 2. DRAWING
                    // Calculate Signed Distance from the pixel to the distorted circle
                    float distToCircle = sdCircle(p, pinchedCenter, scaledRadius);

                    // Is the pixel inside this ball?
                    if (distToCircle < _EdgeSmoothness)
                    {
                        // Determine fill vs outline
                        // 'outline' is a value from 0 (at the edge) to 1 (at the outline's inner border)
                        float outline = smoothstep(-_BallOutlineWidth * scaledRadius, 0, distToCircle);
                        fixed3 color = lerp(ballFillColor.rgb, _BallOutlineColor.rgb, outline);

                        // Anti-alias the outer edge of the circle
                        float alpha = smoothstep(_EdgeSmoothness, -_EdgeSmoothness, distToCircle);

                        // Blend this ball's color with any previous balls drawn on this pixel
                        finalColor.rgb = lerp(finalColor.rgb, color, alpha);
                        finalColor.a = max(finalColor.a, alpha);
                    }
                }
                
                // If this pixel wasn't part of any ball, make it fully transparent.
                if (finalColor.a <= 0)
                {
                    discard;
                }

                return finalColor;
            }
            ENDCG
        }
    }
}