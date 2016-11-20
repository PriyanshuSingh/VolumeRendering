Shader "Unlit/TestShader"
{






    //         Properties
    // {
    //     _Tex ("Texture", 2D) = ""

    // }



	CGINCLUDE
		#include "UnityCG.cginc"

        uniform sampler3D _Volume;
        uniform float Depth;

        struct vin
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
		struct v2f {
			float4 pos : POSITION0;
			float2 localPos : TEXCOORD0;
		};

		v2f vert(vin v)
		{


			v2f o;
			o.pos =  mul(UNITY_MATRIX_MVP,v.vertex);
			o.localPos = v.uv;
			return o;
		}

		half4 frag(v2f i) : COLOR
		{

		            float4 normal = float4(0,0,0,0);

		            float NormalDist = 1/256.0f;

                    float3 pos = float3(i.localPos,Depth);

                    float left = tex3D(_Volume,pos+float4(-NormalDist,0,0,0)).a;
                    float right = tex3D(_Volume,pos+float4(NormalDist,0,0,0)).a;

                    float up = tex3D(_Volume,pos+float4(0,NormalDist,0,0)).a;
                    float down = tex3D(_Volume,pos+float4(0,-NormalDist,0,0)).a;

                    float fr = tex3D(_Volume,pos+float4(0,0,NormalDist,0)).a;
                    float ba = tex3D(_Volume,pos+float4(0,0,-NormalDist,0)).a;
                    normal.x = right-left;
                    normal.y = up-down;
                    normal.z = fr-ba;
                    if(normal.x == 0.0f && normal.y == 0.0f && normal.z == 0.0f){
                       normal = (float4)0;
                    }
                    else{
                       normal = normalize(normal);
                    }

            //float3 s = ;
            //float3 c =  (float3)tex3D(_Volume,s).a;
            return normal;
		}

	ENDCG

	Subshader
	{

	    Cull Off
	    //replacement of RenderType volume for this shader
		//Tags {"RenderType"="Volume"}
		//Fog { Mode off }

        Pass
		{
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			ENDCG
		}
	}


}
