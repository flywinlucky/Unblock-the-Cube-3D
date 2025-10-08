Shader "Custom/TransparentArrowShell"
{
    Properties
    {
        _ArrowTex ("Arrow Texture (PNG, Points Up)", 2D) = "white" {}
        _MoveDirection ("Move Direction", Vector) = (0,1,0,0)

        // --- CONTROL MANUAL SEPARAT PENTRU FIECARE AXĂ ---
        _RotationOffsetY ("Rotation Top/Bottom (Y)", Range(-180, 180)) = 0
        _RotationOffsetX ("Rotation Right/Left (X)", Range(-180, 180)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 localNormal : TEXCOORD2;
            };

            sampler2D _ArrowTex;
            float4 _ArrowTex_ST;
            float3 _MoveDirection;
            float _RotationOffsetY, _RotationOffsetX;

            float2 rotateUV(float2 uv, float rotation) {
                uv -= 0.5;
                float s = sin(rotation);
                float c = cos(rotation);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                uv = mul(rotationMatrix, uv);
                uv += 0.5;
                return uv;
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ArrowTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.localNormal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fața "din față" (local Z+) și "din spate" (local Z-) devin transparente
                if (abs(i.localNormal.z) > 0.9)
                {
                    clip(-1); // Metodă sigură de a face pixelul invizibil
                }
                
                float3 moveDir = normalize(_MoveDirection);
                float3 normal = normalize(i.worldNormal);
                float2 uv = i.uv;
                float autoRotationAngle = 0;
                float manualRotationRad = 0;

                // Pasul 1: Calculăm rotația automată
                if (abs(normal.y) > 0.9) // Fețele de SUS sau JOS
                {
                    autoRotationAngle = atan2(moveDir.x, moveDir.z);
                    // Pasul 2: Preluăm rotația manuală DOAR pentru această axă
                    manualRotationRad = _RotationOffsetY * (3.14159 / 180.0);
                }
                else if (abs(normal.x) > 0.9) // Fețele din DREAPTA sau STÂNGA
                {
                    autoRotationAngle = atan2(moveDir.z * normal.x, moveDir.y);
                    // Pasul 2: Preluăm rotația manuală DOAR pentru această axă
                    manualRotationRad = _RotationOffsetX * (3.14159 / 180.0);
                }
                
                // Pasul 3: Combinăm rotația automată cu cea manuală
                float finalAngle = autoRotationAngle + manualRotationRad;
                uv = rotateUV(uv, finalAngle);
                
                fixed4 col = tex2D(_ArrowTex, uv);
                clip(col.a - 0.1); 
                return col;
            }
            ENDCG
        }
    }
}
