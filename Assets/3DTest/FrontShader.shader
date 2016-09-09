﻿Shader "VolumeRendering/FrontShader"
{

	CGINCLUDE
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : POSITION;
			float3 localPos : TEXCOORD0;
		};

		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.localPos = v.vertex.xyz*0.5 + 0.5;
			return o;
		}

		half4 frag(v2f i) : COLOR
		{
			return float4(i.localPos, 1);
		}

	ENDCG

	Subshader
	{
	    //replacement of RenderType volume for this shader
		Tags {"RenderType"="Volume"}
		Fog { Mode off }

        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
    //no fallback mechanism
	Fallback Off
}


