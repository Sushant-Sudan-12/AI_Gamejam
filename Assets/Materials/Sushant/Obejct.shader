Shader "Custom/WireframeGlow"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0,0,0,1)
        _WireColor ("Wire Color", Color) = (0,0,1,1)
        _WireThickness ("Wire Thickness", Range(0, 0.1)) = 0.01
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 4
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2g
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 dist : TEXCOORD1;
            };
            
            float4 _MainColor;
            float4 _WireColor;
            float _WireThickness;
            float _GlowIntensity;
            
            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream)
            {
                float2 p0 = i[0].vertex.xy / i[0].vertex.w;
                float2 p1 = i[1].vertex.xy / i[1].vertex.w;
                float2 p2 = i[2].vertex.xy / i[2].vertex.w;
                
                float2 v0 = p2 - p1;
                float2 v1 = p0 - p2;
                float2 v2 = p1 - p0;
                
                // Compute the area of the triangle
                float area = abs(v1.x * v2.y - v1.y * v2.x);
                
                g2f o;
                o.vertex = i[0].vertex;
                o.uv = i[0].uv;
                o.dist = float3(area / length(v0), 0, 0);
                triStream.Append(o);
                
                o.vertex = i[1].vertex;
                o.uv = i[1].uv;
                o.dist = float3(0, area / length(v1), 0);
                triStream.Append(o);
                
                o.vertex = i[2].vertex;
                o.uv = i[2].uv;
                o.dist = float3(0, 0, area / length(v2));
                triStream.Append(o);
            }
            
            fixed4 frag (g2f i) : SV_Target
            {
                // Find the minimum distance to the edge
                float minDist = min(min(i.dist.x, i.dist.y), i.dist.z);
                
                // Apply thickness and smoothing
                float intensity = exp2(-4 * minDist / _WireThickness);
                
                // Mix the wire color with the main color based on intensity
                float4 finalColor = lerp(_MainColor, _WireColor * _GlowIntensity, intensity);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}