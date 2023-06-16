Shader "Custom/DepthShader"
{
    Properties {}

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
            // The DeclareDepthTexture.hlsl file contains utilities for 
            // sampling the Camera depth texture.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                // The vertex positions in object space.
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                // The vertex position in screen space
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // Compute coordinates w.r.t. to Screen coords
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // Obtain raw depth value
                float rawDepth = SampleSceneDepth(input.screenPos.xy / input.screenPos.w);
                // Normalize depth value into [0, 1] by dividing far plane value
                float depth = Linear01Depth(rawDepth, _ZBufferParams);
                // Black => far; White => close 
                depth = 1.0f - depth;

                return half4(depth, depth, depth, 1);
            }
            ENDHLSL
        }
    }
}
