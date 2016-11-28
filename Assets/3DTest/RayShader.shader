// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

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

            #pragma vertex  VS
            #pragma fragment RayCastPS

            //functions
            float4 composite(float4 dst,float4 src);
            float getPhongFactor(float4 normal,float3 wPos);
            float4 composeFrontToBack(float3 front,float3 back,float3 wPos);
            float4 composeBackToFront(float3 front,float3 back,float3 wPos);









            uniform sampler3D _Volume;
            uniform sampler2D _BackTex;

            //Directional Light in world coordinates
            uniform float3 L;
            uniform sampler2D _transferF;

            struct VertexShaderInput
            {
                float4 Position : POSITION0;
                float2 texC     : TEXCOORD0;
            };

            struct VertexShaderOutput
            {
                float4 Position     : POSITION0;
                float3 texC         : TEXCOORD0;
                float4 pos          : TEXCOORD1;
                float3 wPos         : TEXCOORD2;


            };



            VertexShaderOutput VS(VertexShaderInput input)
            {
                VertexShaderOutput output;

                output.Position = mul(UNITY_MATRIX_MVP, input.Position);
                output.texC = input.Position*0.5 + 0.5;
                output.pos = output.Position;
                output.wPos  = mul(unity_ObjectToWorld,input.Position);
                return output;
            }





            uniform float iterations;
            uniform float baseIterations;
            uniform float alphaThreshold;
            //0 for front to back,1 for back to front
            uniform int compositingType;



            float4 RayCastPS(VertexShaderOutput input) : COLOR0
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



                if(compositingType == 0){
                    return composeFrontToBack(front,back,input.wPos);
                }
                else{
                    return composeBackToFront(front,back,input.wPos);
                }



            }

            float getPhongFactor(float4 normal,float3 wPos){



                float3 lightDir = L;
                float3 reflecVec = normalize(2*normal*dot(normal,lightDir)-lightDir);
                float3 viewDir = normalize(_WorldSpaceCameraPos-wPos);

                float diffref = max(0, dot(normal,lightDir));
                float specref = 0;
                //check if light is on right side
                if(diffref > 0){
                   specref = pow(max(dot(viewDir,reflecVec),0),1);
                }

                float ambientreff = 0.4f;
                return specref+diffref+ambientreff;


            }
            float4 composeFrontToBack(float3 front,float3 back,float3 wPos){

                float3 dir = normalize(back - front);
                float stepSize = 1/iterations;
                float3 Step = dir * stepSize;
                float4 pos = float4(front, 0);

                float4 dst = float4(0, 0, 0, 0);
                float4 src = 0;
                float4 normal = 0;

                float value = 0;
                for(int i = 0; i < iterations; ++i)
                {
                    pos.w = 0;
                    //get normal
                    normal = tex3Dlod(_Volume, pos);

                    //copy iso value to src
                    src = (float4)normal.a;
                    normal.a = 0;

                    src = tex2Dlod(_transferF, float4(src.r, 0.5f, 0, 0));


                    //opacity correction
                    src.a = 1 - pow((1 - src.a),baseIterations/iterations);




                    //convert normals back to -1 to 1 range
                    normal *= 2.0f;normal -= 1.0f; normal.a = 0;

                    if(normal.x == 0.0f && normal.y == 0.0f && normal.z == 0.0f){
                       normal = (float4)0;

                    }
                    else{

                       normal = normalize(normal);
                    }







                    src.rgb*=getPhongFactor(normal,wPos);
                    //alpha premultipied colors
                    src.rgb *= src.a;




                    dst = (1.0f - dst.a)*src + dst;

                    //break from the loop when alpha gets high enough(early ray termination)
                    if(dst.a >= alphaThreshold)
                        break;

                    //advance the current position
                    pos.xyz += Step;
                    //break if the position is out of volume
                    if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f || pos.x < 0.0f || pos.y < 0.0f || pos.z < 0.0f)
                        break;
                }

                return dst;
            }

             float4 composeBackToFront(float3 front,float3 back,float3 wPos){

                 float3 dir = normalize(front-back);
                 float stepSize = 1/iterations;
                 float3 Step = dir * stepSize;
                 float4 pos = float4(back, 0);

                 float4 dst = float4(0, 0, 0, 0);
                 float4 src = 0;
                 float4 normal = 0;

                 float value = 0;
                 for(int i = 0; i < iterations; ++i)
                 {
                     pos.w = 0;
                     //get normal
                     normal = tex3Dlod(_Volume, pos);

                     //copy iso value to src
                     src = (float4)normal.a;
                     normal.a = 0;

                     src = tex2Dlod(_transferF, float4(src.r, 0.5f, 0, 0));


                     //opacity correction
                     src.a = 1 - pow((1 - src.a),baseIterations/iterations);




                     //convert normals back to -1 to 1 range
                     normal *= 2.0f;normal -= 1.0f; normal.a = 0;

                     if(normal.x == 0.0f && normal.y == 0.0f && normal.z == 0.0f){
                        normal = (float4)0;

                     }
                     else{

                        normal = normalize(normal);
                     }







                     src.rgb*=getPhongFactor(normal,wPos);
                     //alpha premultipied colors
                     src.rgb *= src.a;




                     dst = (1.0f - src.a)*dst + src;

                     //advance the current position
                     pos.xyz += Step;

                     //break if the position is out of volume
                     if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f || pos.x < 0.0f || pos.y < 0.0f || pos.z < 0.0f)
                         break;;
                 }

                 return dst;




            }






            ENDCG
        }
    }
    Fallback off
}
