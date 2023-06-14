Shader "Custom/RedShaderURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
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
            // Define the texture and sampler state
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Pass data from Unity to the vertex shader
            // Need the vertex position and the UV coordinates
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Pass data from the vertex shader to the fragment shader
            // Need the UV coordinates for sampling the texture
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

            // The fragment shader
            float4 frag(v2f input) : SV_Target
            {
                // Sample the texture using the UV coordinates
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // Keep only the red channel and set the green and blue channels to 0
                color.g = 0;
                color.b = 0;
                return color;
            }
            ENDHLSL
        }
    }
}