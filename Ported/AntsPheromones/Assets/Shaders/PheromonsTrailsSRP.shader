Shader "Custom/PheromoneTrailsSRP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            //#pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"


            struct Attributes {
                float3 positionOS : POSITION; 
                float2 baseUV : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 baseUV : VAR_BASE_UV;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            CBUFFER_END


            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _MainTex_ST)
                UNITY_DOTS_INSTANCED_PROP(float4, _MainTex_TexelSize)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
            
            #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor)
            #define _MainTex_ST         UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _MainTex_ST)
            #define _MainTex_TexelSize  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _MainTex_TexelSize)
            #endif

            Varyings Vertex(Attributes input) {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
                output.positionCS = posnInputs.positionCS;
                float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                output.baseUV = input.baseUV * baseST.xy + baseST.zw;

                return output;
            }
            

            float4 Fragment(Varyings input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);
                
                float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.baseUV);
                float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
                float4 base = baseMap * baseColor;

                float2 offsets[3] = { float2(.75, -.5), float2(-.75, -.5), float2(0, .5) };
                float4 c = 0;
                for (int i = 0; i < 3; i++)
                {
                    c += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.baseUV + offsets[i] * _MainTex_TexelSize.xy);
                }
                base.rgb = sqrt(c.rgb / 3.f) * baseColor.rgb;
                base.a = c.a;

                return base;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
