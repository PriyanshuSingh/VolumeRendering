Shader "Unlit/3Dshader"
{
	Properties
	{
		_Volume ("Texture", 3D) = "" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct vs_input {
                float4 vertex : POSITION;
            };

            struct ps_input {
                float4 pos : SV_POSITION;
                float3 uv : TEXCOORD0;
            };


            ps_input vert (vs_input v)
            {
                ps_input o;
                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.vertex.xyz*0.5+0.5;
                return o;
            }

            uniform sampler3D _Volume;

            float4 frag (ps_input i) : COLOR
            {
                return tex3D (_Volume, i.uv);
            }


			ENDCG
		}
	}
}
