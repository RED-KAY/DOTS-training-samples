Shader "GridColonies/SectorShader"
{
    Properties
    {
        _MyArr ("Tex", 2DArray) = "" {}
        _Color ("Base Colour", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            // texture arrays are not available everywhere,
            // only compile shader on platforms where they are
            #pragma require 2darray
            // #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
 
            struct VertexInput
            {
                float4 pos : POSITION0;
                float4 tex : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
 
            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };
 
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
        
            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _Color)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
            
            #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _Color)
            #endif
 
            v2f vert (VertexInput v)
            {
                v2f o;
 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
 
                o.vertex = mul(UNITY_MATRIX_MVP, v.pos);
               
                // Texture uvs
                o.uv.xy = v.tex.xy;
 
                // Texture array index
                o.uv.z = v.tex.z;
 
                // Height for tiling the texture
                o.uv.y *= v.tex.w;
 
                return o;
            }
           
            TEXTURE2D_ARRAY(_MyArr);
            SAMPLER(sampler_MyArr);
 
            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
 
                return SAMPLE_TEXTURE2D_ARRAY(_MyArr, sampler_MyArr, i.uv.xy, i.uv.z) * _Color;
            }
            ENDHLSL
        }
    }
}