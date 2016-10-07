// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/1_Lambert"
{

    Properties{
        _Color("Color",Color) = (1.0,1.0,1.0,1.0)
        _Ambient("Ambient Contribution",Float) = 0.1

    }
    SubShader{




        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //#include "UnityCG.cginc"
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0


            //user defined vars
            uniform float4 _Color;
            uniform float _Ambient;






            struct vertexOutput{
                float4 pos : SV_POSITION;
                float3 wPos : TEXCOORD1;
                float3 normal : NORMAL;
                float4 col : COLOR;

            };


            //appdata_base included in UnityCG.cginc
            vertexOutput vert(appdata_base v){
                vertexOutput o;
                //multiply by MVP matrix
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.col = _Color;
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;

            }


            float4 frag(vertexOutput i) : COLOR
            {


                float3 lightDir = normalize(_WorldSpaceLightPos0);
                float3 normal = normalize(UnityObjectToWorldNormal(i.normal));
                float3 viewDir = normalize(_WorldSpaceCameraPos-i.wPos);
                float3 reflecVec = normalize(2*normal*dot(normal,lightDir)-lightDir);

                float diffuse = max(0, dot(normal,lightDir));
                float spec = 0;
                //check if light is on right side
                if(diffuse > 0){
                    spec = pow(max(dot(viewDir,reflecVec),0),16);
                }


                return i.col*(diffuse+spec+_Ambient);

            }
            ENDCG
        }

    }


}



