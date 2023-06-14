Shader "Custom/DepthShader"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP header
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            // Pass data from Unity to the vertex shader
            // Nneed the vertex position and the UV coordinates
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Pass data from the vertex shader to the fragment shader
            // Need the UV coordinates for the depth texture sampling.
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // The vertex shader
            v2f vert(appdata v)
            {
                v2f output;
                // Macros for handling instance and stereo rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Pass UV coordinates from vertex to fragment shader
                output.uv = v.uv;
                return output;
            }

            // Responsible for computing the color of the pixel
            half4 frag(v2f input) : SV_Target
            {
                // Sample the depth texture to get the depth value at the current pixel
                half depth = SAMPLE_DEPTH_TEXTURE(
                    _CameraDepthTexture, sampler_CameraDepthTexture, input.uv
                ).r;
                half grayscale = 1.0 - depth;
 
                return half4(depth, depth, depth, 1);
            }
            ENDHLSL
        }
    }

}
