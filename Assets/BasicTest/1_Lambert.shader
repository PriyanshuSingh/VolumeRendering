Shader "Unlit/1_Lambert"
{

    Properties{
        _Color("Color",Color) = (1.0,1.0,1.0,1.0)

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




            struct vertexOutput{
                float4 pos : SV_POSITION;
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
                return o;

            }


            float4 frag(vertexOutput i) : COLOR
            {


                float3 normal = normalize(UnityObjectToWorldNormal(i.normal));
                float x = max(0, dot(normal, _WorldSpaceLightPos0.xyz));


                return x*i.col;
            }
            ENDCG
        }

    }


}



