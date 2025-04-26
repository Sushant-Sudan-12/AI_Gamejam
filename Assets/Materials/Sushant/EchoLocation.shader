Shader "Custom/EchoOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.01
        _OutlineIntensity ("Outline Intensity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        
        // First pass - outline
        Pass
        {
            ZWrite Off
            Cull Front
            
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
            };
            
            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                // Extrude along normal direction
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * _OutlineWidth;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 outlinePos = worldPos + float4(outlineOffset, 0);
                o.pos = mul(UNITY_MATRIX_VP, outlinePos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Apply intensity to outline color
                fixed4 col = _OutlineColor;
                col.a *= _OutlineIntensity;
                return col;
            }
            ENDCG
        }
        
        // Second pass - original object
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
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Just show the original texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}