// Unity Shader for a 2D "Square Tunnel" effect.
// This shader creates a visual effect of infinitely zooming through a tunnel of squares.
// It's designed to be applied to a 2D quad and includes rotation and other effects.
Shader "Unlit/SquareTunnel"
{
    // Properties block to expose values to the Unity Inspector.
    // This allows for real-time tweaking of the effect without editing code.
    Properties
    {
        _Color1 ("Color 1", Color) = (1,0,0,1) // The first color of the squares (default red)
        _Color2 ("Color 2", Color) = (0,0,1,1) // The second color of the squares (default blue)
        _Speed ("Speed", Range(0.1, 10)) = 1.0   // Controls the speed of the inward/outward animation
        _Direction ("Direction", Range(-1, 1)) = 1.0 // Controls the direction of the animation (1 for inward, -1 for outward)
        _Frequency ("Frequency", Range(1, 50)) = 10.0 // Controls the number of squares visible at once
        _LineWidth ("Line Width", Range(0.01, 1.0)) = 0.5 // Controls the thickness of the colored lines
        _RotationSpeed ("Rotation Speed", Range(-10, 10)) = 1.0 // Controls the base speed of the tunnel's rotation
        _DistanceRotation ("Distance Rotation", Range(0, 50)) = 10.0 // How much rotation increases towards the center
        _Perspective ("Perspective", Range(0.1, 4.0)) = 1.5 // Controls the perspective effect of the lines
        _AberrationAmount ("Chromatic Aberration", Range(0, 0.1)) = 0.01 // Strength of the color separation
        _EdgeFade ("Edge Fade", Range(0.0, 0.5)) = 0.1 // Controls the softness of the outer edges
    }
    SubShader
    {
        // Set tags for transparency
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            // Enable alpha blending for transparency
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // Struct to define the data passed from the application to the vertex shader.
            struct appdata
            {
                float4 vertex : POSITION; // Vertex position
                float2 uv : TEXCOORD0;     // UV coordinates
            };

            // Struct to define the data passed from the vertex shader to the fragment shader.
            struct v2f
            {
                float2 uv : TEXCOORD0;     // UV coordinates
                UNITY_FOG_COORDS(1)        // Fog coordinates
                float4 vertex : SV_POSITION; // Clip space position
            };

            // Accessing the properties defined above.
            fixed4 _Color1;
            fixed4 _Color2;
            float _Speed;
            float _Direction;
            float _Frequency;
            float _LineWidth;
            float _RotationSpeed;
            float _DistanceRotation;
            float _Perspective;
            float _AberrationAmount;
            float _EdgeFade;

            // Vertex Shader:
            // Processes each vertex of the quad.
            v2f vert (appdata v)
            {
                v2f o;
                // Transform the vertex position from object space to clip space.
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Pass the UV coordinates to the fragment shader.
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // Helper function to calculate the color pattern for a given UV coordinate.
            // This encapsulates the core logic to avoid repetition for chromatic aberration.
            fixed4 calculateColor(float2 uv)
            {
                // Calculate distance before rotation
                float preRotationDist = max(abs(uv.x), abs(uv.y));

                // Apply Rotation based on distance
                float angle = _Time.y * _RotationSpeed + (0.5 - preRotationDist) * _DistanceRotation;
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                uv = mul(rotationMatrix, uv);

                // Calculate final distance and apply perspective
                float dist = max(abs(uv.x), abs(uv.y));
                // To make lines denser at the center, we need the frequency to increase.
                // We achieve this by modifying the frequency based on the inverse of the distance.
                float perspectiveFrequency = _Frequency / (pow(dist, _Perspective) + 0.1);


                // Create the repeating pattern
                float pattern = frac(dist * perspectiveFrequency - _Time.y * _Speed * _Direction);

                // Create sharp lines with controllable width
                float sharpPattern = step(1.0 - _LineWidth, pattern);

                // Mix the colors
                return lerp(_Color1, _Color2, sharpPattern);
            }

            // Fragment Shader:
            // Processes each pixel of the quad to determine its color.
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Center the UV coordinates
                float2 centeredUv = i.uv - 0.5;

                // 2. Chromatic Aberration Setup
                // Calculate the direction from the center for the color channel offset
                float2 aberrationDirection = normalize(centeredUv);
                // Calculate the base distance to determine aberration strength (stronger near center)
                float baseDist = max(abs(centeredUv.x), abs(centeredUv.y));
                float aberrationOffset = (0.5 - baseDist) * _AberrationAmount;

                // 3. Calculate color for each channel with offsets
                // We call our helper function for each color channel, slightly offsetting the UVs.
                fixed4 colR = calculateColor(centeredUv + aberrationDirection * aberrationOffset);
                fixed4 colG = calculateColor(centeredUv); // Green channel is the "true" position
                fixed4 colB = calculateColor(centeredUv - aberrationDirection * aberrationOffset);

                // 4. Combine Channels
                // We construct the final color by taking the R, G, and B components from our
                // three calculations. The alpha is taken from the central (green) sample.
                fixed4 finalCol = fixed4(colR.r, colG.g, colB.b, colG.a);

                // 5. Calculate Edge Fade
                // Use smoothstep to create a smooth transition from opaque to transparent at the edges.
                // It calculates a value from 1 (center) to 0 (edge) over the width defined by _EdgeFade.
                float edgeAlpha = smoothstep(0.5, 0.5 - _EdgeFade, baseDist);
                
                // 6. Apply Edge Fade
                // Multiply the final color's alpha by our fade value.
                finalCol.a *= edgeAlpha;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalCol);
                return finalCol;
            }
            ENDCG
        }
    }
}
