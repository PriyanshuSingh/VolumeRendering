Shader "VolumeRendering/MakeShader"
{







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

                    float left = tex3D(_Volume,pos+float3(-NormalDist,0,0)).a;
                    float right = tex3D(_Volume,pos+float3(NormalDist,0,0)).a;

                    float up = tex3D(_Volume,pos+float3(0,NormalDist,0)).a;
                    float down = tex3D(_Volume,pos+float3(0,-NormalDist,0)).a;

                    float fr = tex3D(_Volume,pos+float3(0,0,NormalDist)).a;
                    float ba = tex3D(_Volume,pos+float3(0,0,-NormalDist)).a;
                    normal.x = (right-left+1.0f)/2.0f;
                    normal.y = (up-down+1.0f)/2.0f;
                    normal.z = (fr-ba+1.0f)/2.0f;
//                    if(normal.x == 0.0f && normal.y == 0.0f && normal.z == 0.0f){
//                       normal = (float4)0;
//                    }
//                    else{
//                       normal = normalize(normal);
//                    }

            //float3 s = ;
            normal.a =  tex3D(_Volume,pos).a;
            return normal;
		}

	ENDCG

	Subshader
	{

	    Cull Off


        Pass
		{
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			ENDCG
		}
	}


}
