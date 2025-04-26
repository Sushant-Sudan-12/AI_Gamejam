Shader "Custom/EchoLocationPulse"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PulseRadius ("Pulse Radius", Float) = 0
        _PulseWidth ("Pulse Width", Float) = 1.5
        _PulseColor ("Pulse Color", Color) = (1, 1, 1, 1)
        _EnemyColor ("Enemy Color", Color) = (1, 0, 0, 1)
        _GridSize ("Grid Size", Float) = 0.5
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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float _PulseRadius;
            float _PulseWidth;
            float4 _PulseColor;
            float4 _EnemyColor;
            float3 _PlayerPosition;
            float4 _EnemyPositions[20];  // Max 20 enemies
            int _EnemyCount;
            float _GridSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // Calculate world position
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                
                return o;
            }
            
            // Function to create a tech pattern
            float techPattern(float3 pos) {
                float x = frac(pos.x / _GridSize) - 0.5;
                float z = frac(pos.z / _GridSize) - 0.5;
                float y = frac(pos.y / _GridSize) - 0.5;
                
                float squares = step(0.4, max(abs(x), max(abs(z), abs(y))));
                return squares;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get original scene color (will be black in your case)
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Get scene depth
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float linearDepth = LinearEyeDepth(depth);
                
                // Reconstruct world position from depth
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float3 worldPos = _WorldSpaceCameraPos + viewDir * linearDepth;
                
                // Calculate distance from player
                float distanceFromPlayer = distance(worldPos, _PlayerPosition);
                
                // Determine if point is within the pulse wave
                float innerEdge = _PulseRadius - _PulseWidth;
                float outerEdge = _PulseRadius;
                
                if (distanceFromPlayer > innerEdge && distanceFromPlayer < outerEdge)
                {
                    // Calculate intensity based on position in wave
                    float wavePos = 1.0 - (outerEdge - distanceFromPlayer) / _PulseWidth;
                    float intensity = sin(wavePos * 3.14159); // Gives a peak in the middle of the wave width
                    
                    // Check if this point is near an enemy
                    bool nearEnemy = false;
                    for (int e = 0; e < _EnemyCount; e++)
                    {
                        float distToEnemy = distance(worldPos, _EnemyPositions[e].xyz);
                        if (distToEnemy < 2.0) { // Adjust radius as needed
                            nearEnemy = true;
                            break;
                        }
                    }
                    
                    // Apply tech pattern
                    float pattern = techPattern(worldPos);
                    
                    // Choose color based on enemy proximity
                    float4 waveColor = nearEnemy ? _EnemyColor : _PulseColor;
                    
                    // Apply the wave color with tech pattern
                    col = lerp(col, waveColor * pattern, intensity * 0.8);
                }
                
                return col;
            }
            ENDCG
        }
    }
}