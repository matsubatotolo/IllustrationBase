Shader "Custom/Brush_URP"
{
    Properties
    {
        _MainTex ("Brush Texture", 2D) = "white" {}
        _Color ("Brush Color", Color) = (0,0,0,1)
        _BrushPos ("BrushPos", Vector) = (0,0,0.1,0.1)
        _Hardness ("Hardness", Range(0,1)) = 0.8
        _Opacity ("Opacity", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" }

        Pass
        {
            Name "Brush"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float4 _BrushPos;
            float _Hardness;
            float _Opacity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            // half4 frag(Varyings i) : SV_Target
            // {
            //     float2 diff = i.uv - _BrushPos.xy;
            //     float2 scaled = diff / _BrushPos.zw;
            //     float dist = length(scaled);

            //     if (dist > 0.5)
            //         discard;

            //     float2 brushUV = scaled * 0.5 + 0.5;
            //     half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, brushUV);
            //     float innerRadius = _Hardness * 0.5;
            //     float outerRadius = 0.5;
            //     float edge = smoothstep(innerRadius, outerRadius, dist);
            //     float alpha = tex.a * (1.0 - edge) * _Opacity;
            //     half4 col = _Color;
            //     col.a *= alpha;

            //     return col;
            // }
            half4 frag(Varyings i) : SV_Target
            {
                float2 localUV = (i.uv - _BrushPos.xy) / _BrushPos.zw;
                localUV += 0.5;

                // ブラシ外
                if (localUV.x < 0 || localUV.x > 1 || localUV.y < 0 || localUV.y > 1)
                    discard;

                float2 centered = localUV - 0.5;
                float dist = length(centered);
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, localUV);
                float edge = smoothstep(_Hardness * 0.5, 0.5, dist);
                float alpha = tex.a * (1.0 - edge) * _Opacity;
                clip(alpha - 0.001);
                half4 col = _Color;
                col.a = alpha;

                return col;
            }
            ENDHLSL
        }
    }
}