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

                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                // The vertex position in screen space
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float4 viewDir : TEXCOORD2;
                
                float3 viewDirVec: TEXCOORD3;

                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // Compute coordinates w.r.t. to Screen coords
                output.screenPos = ComputeScreenPos(output.positionCS);
                

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.viewDirVec = _WorldSpaceCameraPos - positionWS;

                // Compute view direction
                output.viewDir = mul (unity_CameraInvProjection, float4 (input.uv, 1.0, 1.0));

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // Sample the depth from the Camera depth texture with the 
                // UV coordinates => 
                // divide the pixel location by the render target resolution
                float depth = SampleSceneDepth(
                    input.positionCS.xy / _ScaledScreenParams.xy
                );

                // Convert perpendicular distance values to the plane of the camera,
                // to the actual distance depth values to the camera,
                // and normalize depth values to [0, 1] range by dividing far plane
                depth = Linear01Depth(depth, _ZBufferParams);

                // Set the color to black in the proximity to the far clipping plane.
                if(depth > 0.9999)
                    return 0;

                return depth;
            }
            ENDHLSL
        }
    }
}
