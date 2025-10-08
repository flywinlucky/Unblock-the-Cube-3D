Shader "Custom/TransparentArrowShell"
{
    Properties
    {
        _ArrowTex ("Arrow Texture (PNG, Points Up)", 2D) = "white" {}
        _MoveDirection ("Move Direction", Vector) = (0,1,0,0)
        // NOU: Setare pentru a roti manual săgeata, dacă este necesar.
        _ManualRotationOffset ("Manual Rotation (Degrees)", Range(-180, 180)) = 0
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
            float _ManualRotationOffset; // Variabila pentru rotația manuală

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
                // MODIFICAT: Verificăm acum axa Z, conform culorii Albastru (+Z)
                if (abs(i.localNormal.z) > 0.9)
                {
                    // Fața "din față" (Albastru) și "din spate" (Galben) devin transparente.
                    return fixed4(0, 0, 0, 0);
                }
                else
                {
                    float3 moveDir = normalize(_MoveDirection);
                    float3 normal = normalize(i.worldNormal);
                    float2 uv = i.uv;
                    float rotationAngle = 0;

                    if (abs(normal.y) > 0.9) // Fața de SUS (Verde) sau JOS (Magenta)
                    {
                        rotationAngle = atan2(moveDir.x, moveDir.z);
                    }
                    else if (abs(normal.x) > 0.9) // Fața din DREAPTA (Roșu) sau STÂNGA (Cyan)
                    {
                        rotationAngle = atan2(moveDir.z * normal.x, moveDir.y);
                    }
                    
                    // NOU: Adăugăm rotația manuală la cea calculată automat.
                    // Convertim gradele din Inspector în radiani pentru calcul.
                    float manualRotationRad = _ManualRotationOffset * (3.14159 / 180.0);
                    float finalAngle = rotationAngle + manualRotationRad;

                    uv = rotateUV(uv, finalAngle);
                    
                    fixed4 col = tex2D(_ArrowTex, uv);
                    clip(col.a - 0.1); 
                    
                    return col;
                }
            }
            ENDCG
        }
    }
}