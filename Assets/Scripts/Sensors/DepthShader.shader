Shader "Custom/DepthShader"
{
    Properties {}

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
            // The DeclareDepthTexture.hlsl file contains utilities for 
            // sampling the Camera depth texture.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                // sample the source texture at the UV coordinates
                float depth = SAMPLE_DEPTH_TEXTURE(
                    _CameraDepthTexture, 
                    sampler_CameraDepthTexture, 
                    input.texcoord
                );

                // For testing, output the depth without normalization
                // This output is more clear to be seen in the view
                // return depth;

                // Convert perpendicular distance values to the plane of the camera,
                // to the actual distance depth values to the camera,
                // and normalize depth values to [0, 1] range by dividing far plane
                depth = Linear01Depth(depth, _ZBufferParams);

                // Set the color to black in the proximity 
                // to the near and far clipping plane.
                if(depth > 0.9999 || depth < 0.0001)
                    return 0;
                return depth;
            }
            ENDHLSL
        }
    }
}
