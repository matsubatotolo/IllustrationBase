Shader "Custom/Eraser_URP"
{
    Properties
    {
        _MainTex ("Brush Texture", 2D) = "white" {}
        _Hardness ("Hardness (Falloff)", Range(0,1)) = 1.0
        _Opacity ("Opacity", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "Eraser"
            Tags { "LightMode"="UniversalForward" }

            // RGB : 既存のRGBをそのまま保持
            // Alpha: ブラシのAlpha分だけ既存のAlphaを削る
            Blend Zero One, Zero OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

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

            half4 frag(Varyings i) : SV_Target
            {
                // 1. ブラシテクスチャをサンプリング
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float texAlpha = tex.a;

                // 2. 円形ボケ（Hardness）の計算
                float dist = length(i.uv - 0.5);
                float innerRadius = _Hardness * 0.5;
                float outerRadius = 0.5;
                float edge = smoothstep(innerRadius, outerRadius, dist);

                // 3. 最終的な消去強度
                float finalAlpha = texAlpha * (1.0 - edge) * _Opacity;

                // RGBは使われないが、Alphaだけ出力する
                return half4(0, 0, 0, finalAlpha);
            }
            ENDHLSL
        }
    }
}
