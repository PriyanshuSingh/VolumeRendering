Shader "VolumeRendering/RayShader"
{






    Subshader {
        //TODO see these parameters
       // ZTest Always Cull Off ZWrite Off
        Fog { Mode off }


        Tags {"RenderType" = "Volume"}
        Pass
        {
            CGPROGRAM

            	#include "UnityCG.cginc"
//            	#pragma target 3.0
//            	#pragma profileoption MaxLocalParams=1024
//            	#pragma profileoption NumInstructionSlots=4096
//           	#pragma profileoption NumMathInstructionSlots=4096

            #pragma vertex  VS
            #pragma fragment RayCastSimplePS








            uniform sampler3D _Volume;
            uniform sampler2D _BackTex;

            //light in local coordinates
            uniform float3 L = float3(1,1,1);



//TODO add support for these
            sampler2D sampler_FrontTex = sampler_state
            {
                Texture = <_FrontTex>;
                MinFilter = LINEAR;
                MagFilter = LINEAR;
                MipFilter = LINEAR;

                AddressU = Border;				// border sampling in U
                AddressV = Border;				// border sampling in V
                BorderColor = float4(0,0,0,0);	// outside of border should be black
            };

            sampler2D sampler_BackTex = sampler_state
            {
                Texture = <_BackTex>;
                MinFilter = LINEAR;
                MagFilter = LINEAR;
                MipFilter = LINEAR;

                AddressU = Border;				// border sampling in U
                AddressV = Border;				// border sampling in V
                BorderColor = float4(0,0,0,0);	// outside of border should be black
            };

            sampler3D  sampler_Volume = sampler_state
            {
                Texture = <_Volume>;
                MinFilter = LINEAR;
                MagFilter = LINEAR;
                MipFilter = LINEAR;

                AddressU = Border;				// border sampling in U
                AddressV = Border;				// border sampling in V
                AddressW = Border;
                BorderColor = float4(0,0,0,0);	// outside of border should be black
            };



            uniform sampler2D _transferF;

            struct VertexShaderInput
            {
                float4 Position : POSITION0;
                float2 texC		: TEXCOORD0;
            };

            struct VertexShaderOutput
            {
                float4 Position		: POSITION0;
                float3 texC			: TEXCOORD0;
                float4 pos			: TEXCOORD1;
            };



      

            VertexShaderOutput VS(VertexShaderInput input)
            {
                VertexShaderOutput output;

                output.Position = mul(UNITY_MATRIX_MVP, input.Position);
                output.texC = input.Position*0.5 + 0.5;
                output.pos = output.Position;

                return output;
            }





            //use uniforms to define these vars later
			#define Iterations 128
			#define StepSize 1.0f/128.0f



            float4 RayCastSimplePS(VertexShaderOutput input) : COLOR0
            {





            	//calculate projective texture coordinates
            	//used to project the front and back position textures onto the cube
            	float2 texC = input.pos.xy /= input.pos.w;
            	texC.x =  0.5f*texC.x + 0.5f;
            	texC.y =  0.5f*texC.y + 0.5f;

                //obtain front coordinates directly
				float3 front = input.texC;
                //back textures from sampling
                float3 back = tex2D(_BackTex, texC).xyz;

                float3 dir = normalize(back - front);
                float4 pos = float4(front, 0);

                float4 dst = float4(0, 0, 0, 0);
                float4 src = 0;
                float4 normal = 0;

                float value = 0;

            	float3 Step = dir * StepSize;

                for(int i = 0; i < Iterations; ++i)
                {
            		pos.w = 0;
            		//get normal
            		normal = tex3Dlod(_Volume, pos);
                    //copy greyscale value to src
            		src = (float4)normal.a;
            		normal.a = 0;
            		//TAG: TF BEGIN
            		//src.rgb = tex2Dlod(_transferF, float4(src.r, 0.5f, 0, 0)).rgb;
            		//TAG: TF END

            		src.a *= .1f; //reduce the alpha to have a more transparent result
            					  //this needs to be adjusted based on the step size
            					  //i.e. the more steps we take, the faster the alpha will grow



                    float s = max(dot(normal.xyz,normalize(pos-L)),0);
                    //diffuse shading + fake ambient lighting
                    //src.rgb = s * src.rgb;
                    //+ .1f * src.rgb;
            		//Front to back blending
            		//dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb;
            		//dst.a   = dst.a   + (1 - dst.a) * src.a;
            		src.rgb *= src.a;
            		dst = (1.0f - dst.a)*src + dst;

            		//break from the loop when alpha gets high enough
            		if(dst.a >= .95f)
            			break;

            		//advance the current position
            		pos.xyz += Step;

            		//break if the position is greater than <1, 1, 1>
            		if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f)
            			break;
                }

                return dst;
            }
            ENDCG
        }
    }
    Fallback off
}
