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
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            HLSLPROGRAM
            // URP header
            // The Core.hlsl file contains definitions of frequently used 
            // HLSL macros and functions
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            // Source texture
            TEXTURE2D(_MainTex);
            // Source texture sampler
            SAMPLER(sampler_MainTex);

            // Far plane distance
            float _FarClipPlane;

            half4 frag (Varyings input) : SV_Target
            {
                // sample the source texture at the UV coordinates
                float depth = SAMPLE_TEXTURE2D(
                    _MainTex, sampler_MainTex, input.texcoord
                ).r;
                // scale the depth value by the far plane distance
                return depth * _FarClipPlane;

                return depth;
            }
            ENDHLSL
        }
    }
}
