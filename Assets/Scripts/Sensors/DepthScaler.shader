// A shader used to scale the 
// normalized depth values of a render texture
// back to actual depth distances
Shader "Custom/DepthScaler"
{
    Properties
    {
        // the source texture
        _MainTex ("Texture", 2D) = "white" {}
        // the far plane distance
        _FarClipPlane ("FarClipPlane", Range(0, 1000)) = 100
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP header
            // The Core.hlsl file contains definitions of frequently used 
            // HLSL macros and functions
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                // Vertex position in object space
                float4 positionOS : POSITION;
                // UV coordinates
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                // Vertex position in screen space
                float4 positionCS : SV_POSITION;
                // UV coordinates
                float2 uv : TEXCOORD0;
            };

            // Source texture
            TEXTURE2D(_MainTex);
            // Source texture sampler
            SAMPLER(sampler_MainTex);

            // Far plane distance
            float _FarClipPlane;

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Transform the vertex position to clip space and 
                // pass it to the fragment function
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // Pass the UV coordinates to the fragment function
                output.uv = input.uv;

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // sample the source texture at the UV coordinates
                float depth = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).r;
                // scale the depth value by the far plane distance
                return depth * _FarClipPlane;

                return depth;
            }
            ENDHLSL
        }
    }
}
