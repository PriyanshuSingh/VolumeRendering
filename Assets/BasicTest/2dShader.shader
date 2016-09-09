Shader "Unlit/2dShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "" {}
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
                    float2 uv : TEXCOORD0;
                };


                ps_input vert (vs_input v)
                {
                    ps_input o;
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.uv = v.vertex.xy*0.5+0.5;
                    return o;
                }

                uniform sampler2D _MainTex;

                float4 frag (ps_input i) : COLOR
                {
                    return tex2D (_MainTex, i.uv);
                }


            ENDCG
		}
	}
}
