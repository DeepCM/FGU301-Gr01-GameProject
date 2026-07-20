Shader "Hidden/GlitchShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "GlitchPass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            float _GlitchIntensity;
            float _TimeSeconds;
            float _CRTEnabled;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float Noise(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                if (_GlitchIntensity > 0.01)
                {
                    float blockNoise = Noise(float2(floor(uv.y * 10.0), floor(_TimeSeconds * 8.0)));
                    float lineNoise = Noise(float2(uv.y * 100.0, _TimeSeconds * 30.0));

                    if (blockNoise < _GlitchIntensity * 0.4)
                    {
                        uv.x += (Noise(float2(uv.y, _TimeSeconds)) - 0.5) * 0.1 * _GlitchIntensity;
                    }
                    if (lineNoise < _GlitchIntensity * 0.2)
                    {
                        uv.x += (Noise(float2(uv.y * 10.0, _TimeSeconds)) - 0.5) * 0.05 * _GlitchIntensity;
                    }
                }

                float4 color = _MainTex.Sample(sampler_MainTex, uv);

                if (_GlitchIntensity > 0.01)
                {
                    float r = _MainTex.Sample(sampler_MainTex, uv + float2(0.01 * _GlitchIntensity, 0.0)).r;
                    float b = _MainTex.Sample(sampler_MainTex, uv - float2(0.01 * _GlitchIntensity, 0.0)).b;
                    color.r = r;
                    color.b = b;
                }

                if (_CRTEnabled > 0.5)
                {
                    float scanline = sin(uv.y * 800.0 + _TimeSeconds * 5.0) * 0.08;
                    color.rgb -= scanline;

                    float2 d = abs(uv - 0.5) * 2.0;
                    float vignette = d.x * d.x + d.y * d.y;
                    color.rgb *= saturate(1.0 - vignette * 0.15);
                }

                return color;
            }
            ENDHLSL
        }
    }
}
