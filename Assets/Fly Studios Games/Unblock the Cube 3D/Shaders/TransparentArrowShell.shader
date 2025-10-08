Shader "Custom/TransparentArrowShell"
{
    Properties
    {
        _ArrowTex ("Arrow Texture (PNG)", 2D) = "white" {}
        _MoveDirection ("Move Direction", Vector) = (0,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull Off // Oprim Cull pentru ca fețele să fie vizibile din ambele părți
        Blend SrcAlpha OneMinusSrcAlpha // Activăm transparența

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
            };

            sampler2D _ArrowTex;
            float4 _ArrowTex_ST;
            float3 _MoveDirection;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ArrowTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 moveDir = normalize(_MoveDirection);
                float3 normal = normalize(i.worldNormal);

                float dotForward = dot(normal, moveDir);
                float dotBackward = dot(normal, -moveDir);

                // Dacă fața este cea de mișcare sau opusă ei...
                if (dotForward > 0.9 || dotBackward > 0.9)
                {
                    // ...o facem complet transparentă.
                    return fixed4(0, 0, 0, 0);
                }
                else
                {
                    // ...altfel, desenăm săgeata.
                    // Presupunem că săgeata din textură indică în sus.
                    // Aici putem adăuga logica de rotire a UV-urilor ca înainte dacă e necesar,
                    // sau putem lăsa săgețile să aibă o orientare standard pe fețele laterale.
                    // Pentru simplitate, o lăsăm standard.
                    return tex2D(_ArrowTex, i.uv);
                }
            }
            ENDCG
        }
    }
}