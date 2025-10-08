Shader "Custom/Debug_FaceNormals"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL; // Normala în spațiul local
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 localNormal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Trimitem normala locală direct la fragment shader
                o.localNormal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalizăm pentru siguranță
                float3 normal = normalize(i.localNormal);

                // Verificăm pe ce axă este orientată normala și returnăm o culoare specifică
                if (normal.z > 0.9) return fixed4(0, 0, 1, 1);   // Față (+Z) = Albastru
                if (normal.z < -0.9) return fixed4(1, 1, 0, 1);  // Spate (-Z) = Galben
                
                if (normal.y > 0.9) return fixed4(0, 1, 0, 1);   // Sus (+Y) = Verde
                if (normal.y < -0.9) return fixed4(1, 0, 1, 1);  // Jos (-Y) = Magenta
                
                if (normal.x > 0.9) return fixed4(1, 0, 0, 1);   // Dreapta (+X) = Roșu
                if (normal.x < -0.9) return fixed4(0, 1, 1, 1);  // Stânga (-X) = Cyan

                return fixed4(0, 0, 0, 1); // Negru dacă apare o eroare
            }
            ENDCG
        }
    }
}