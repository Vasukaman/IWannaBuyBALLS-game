Shader "Custom/DashedAndLined"
{
    Properties
    {
        [Header(Appearance)]
        _BackgroundColor("Background Color", Color) = (0.1, 0.1, 0.1, 0.5)

        [Header(Perimeter Dashes)]
        _DashColor ("Dash Color", Color) = (1, 1, 1, 1)
        _DashThickness ("Dash Thickness", Float) = 0.05
        _DashLength ("Dash Length", Float) = 0.2
        _GapLength ("Gap Length", Float) = 0.1
        _DashSpeed ("Dash Scroll Speed", Float) = 1.0

        [Header(Interior Lines)]
        _LineColor ("Line Color", Color) = (1, 1, 1, 0.2)
        _LineThickness ("Line Thickness", Float) = 0.02
        _LineSpacing ("Line Spacing", Float) = 0.2
        _LineAngle ("Line Angle", Range(0, 360)) = 0
        _LineScrollSpeed ("Line Scroll Speed (X,Y)", Vector) = (0.1, 0.1, 0, 0)

        // This is set by the C# script
        [HideInInspector] _ObjectScale ("Object Scale (Set from script)", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
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

            // Shader Properties
            fixed4 _BackgroundColor;
            fixed4 _DashColor;
            float _DashThickness;
            float _DashLength;
            float _GapLength;
            float _DashSpeed;

            fixed4 _LineColor;
            float _LineThickness;
            float _LineSpacing;
            float _LineAngle;
            float2 _LineScrollSpeed;

            float4 _ObjectScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Helper function to get a continuous coordinate (0-4) along the UV perimeter.
            float get_perimeter_uv_coord(float2 uv)
            {
                float2 d = abs(uv - 0.5);
                if (d.y > d.x) { // Closer to Top or Bottom edge
                    if (uv.y > 0.5) return 2.0 + (1.0 - uv.x); // Top: returns [2.0, 3.0]
                    else return uv.x;                         // Bottom: returns [0.0, 1.0]
                } else { // Closer to Left or Right edge
                    if (uv.x > 0.5) return 1.0 + uv.y;         // Right: returns [1.0, 2.0]
                    else return 3.0 + (1.0 - uv.y);            // Left: returns [3.0, 4.0]
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Start with the base background color.
                fixed4 final_color = _BackgroundColor;
                float2 uv = i.uv;
                float2 scale = _ObjectScale.xy;

                // 1. INTERIOR LINES //
                // All calculations are done in world-space units for consistency.
                float2 local_pos = (uv - 0.5) * scale;

                float angle_rad = _LineAngle * (3.14159 / 180.0);
                float s = sin(angle_rad);
                float c = cos(angle_rad);
                float2 perp_dir = float2(c, s); // Perpendicular to the lines

                // Project the local position onto the perpendicular vector.
                float projected_val = dot(local_pos, perp_dir);
                
                // Apply scrolling by offsetting the projected value
                float scroll_offset = dot(_LineScrollSpeed.xy * _Time.y, perp_dir);
                projected_val -= scroll_offset;

                // FIX: Add a large offset to the projected value. This ensures the input
                // to fmod() is always positive, which makes the pattern tile correctly
                // across the entire surface instead of just half.
                projected_val += (abs(scale.x) + abs(scale.y)) * 2.0;

                float total_line_width = _LineSpacing + _LineThickness;
                if (_LineSpacing > 0 && fmod(projected_val, total_line_width) < _LineThickness)
                {
                    // Blend the line color over the background.
                    final_color = lerp(final_color, _LineColor, _LineColor.a);
                }

                // 2. PERIMETER DASHED LINE //
                // Calculate world-space distance to the nearest edge
                float dist_x = min(uv.x, 1.0 - uv.x) * scale.x;
                float dist_y = min(uv.y, 1.0 - uv.y) * scale.y;
                float dist_to_edge = min(dist_x, dist_y);

                if (dist_to_edge < _DashThickness)
                {
                    // Get a UV-based perimeter coordinate (0-4)
                    float p_uv = get_perimeter_uv_coord(uv);
                    
                    // Convert the UV perimeter coordinate to a world-space perimeter coordinate
                    float p_world;
                    if (p_uv <= 1.0)      // Bottom edge
                        p_world = p_uv * scale.x;
                    else if (p_uv <= 2.0) // Right edge
                        p_world = scale.x + (p_uv - 1.0) * scale.y;
                    else if (p_uv <= 3.0) // Top edge
                        p_world = scale.x + scale.y + (3.0 - p_uv) * scale.x;
                    else                  // Left edge
                        p_world = scale.x + scale.y + scale.x + (4.0 - p_uv) * scale.y;

                    // Apply scrolling
                    p_world += _DashSpeed * _Time.y;

                    // Calculate dashes
                    float total_dash_pattern_len = _DashLength + _GapLength;
                    if (total_dash_pattern_len > 0 && fmod(p_world, total_dash_pattern_len) < _DashLength)
                    {
                        // Blend the dash color over the current result (background + lines)
                        final_color = lerp(final_color, _DashColor, _DashColor.a);
                    }
                }

                // Clip any pixels outside the quad (important for sprites with tight mesh)
                clip(uv - 0.0001);
                clip(1.0001 - uv);
                
                return final_color;
            }
            ENDCG
        }
    }
}
