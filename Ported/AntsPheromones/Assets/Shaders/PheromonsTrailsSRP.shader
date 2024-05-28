Shader "Custom/PheromoneTrailsSRP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"


            struct Attributes {
                float3 positionOS : POSITION; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            

            struct Interpolators {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END


            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
            
            #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor)
            #endif

            Interpolators Vertex(Attributes input) {
                Interpolators output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
                output.positionCS = posnInputs.positionCS;
            
                return output;
            }
            

            float4 Fragment(Interpolators input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);
                return _BaseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
