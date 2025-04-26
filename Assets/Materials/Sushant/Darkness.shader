Shader "Hidden/DarkVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 1)) = 0.1
        _Contrast ("Contrast", Range(0, 2)) = 1.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        
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
            
            sampler2D _MainTex;
            float _Brightness;
            float _Contrast;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply darkness with adjustable brightness
                col.rgb = (col.rgb - 0.5f) * _Contrast + 0.5f;
                col.rgb *= _Brightness;
                
                return col;
            }
            ENDCG
        }
    }
}