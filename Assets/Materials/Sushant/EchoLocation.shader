Shader "Custom/BadassEchoOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.05
        _OutlineIntensity ("Outline Intensity", Range(0, 3)) = 1.0
        _GlowPower ("Glow Power", Range(1, 10)) = 2.5
        _EdgeSharpness ("Edge Sharpness", Range(1, 10)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent+100" }
        
        // First pass - main object with modified lighting
        Pass
        {
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Add subtle edge highlight
                float edge = 1.0 - saturate(dot(i.viewDir, i.worldNormal));
                edge = pow(edge, 2.0) * 0.5 * _OutlineIntensity;
                
                // Blend with outline color
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, edge * 0.3);
                
                return col;
            }
            ENDCG
        }
        
        // Second pass - outer glow outline
        Pass
        {
            ZWrite Off
            Cull Front
            Blend SrcAlpha One // Additive blending for glow effect
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineIntensity;
            float _GlowPower;
            float _EdgeSharpness;
            
            v2f vert (appdata v)
            {
                v2f o;
                // Extrude along normal direction for outline
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * _OutlineWidth;
                float4 extrudedPos = UnityObjectToClipPos(v.vertex + float4(outlineOffset, 0));
                o.pos = extrudedPos;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Get dynamic outline color with glow effect
                fixed4 outlineCol = _OutlineColor;
                
                // Apply intensity - glow effect!
                outlineCol.rgb *= _OutlineIntensity * _GlowPower;
                
                // Apply edge sharpness
                outlineCol.a = pow(outlineCol.a * _OutlineIntensity, _EdgeSharpness);
                
                return outlineCol;
            }
            ENDCG
        }
    }
}