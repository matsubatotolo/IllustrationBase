Shader "Custom/Brush_URP_Simple"
{
    Properties
    {
        _MainTex ("Brush Texture", 2D) = "white" {}
        _Color ("Brush Color", Color) = (0,0,0,1)
        _Hardness ("Hardness", Range(0,1)) = 0.8
        _Opacity ("Opacity", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "Brush"
            Tags { "LightMode"="UniversalForward" }

            // アルファブレンドを適切に設定
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
                o.uv = v.uv; // メッシュのUV（0〜1）をそのまま引き渡す
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // テクスチャをそのままサンプリング
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // 中心（0.5, 0.5）からの距離を計算して硬さ（Hardness）を適用
                float dist = length(i.uv - 0.5);
                
                // discardは使わず、smoothstepのアルファを0にする
                float innerRadius = _Hardness * 0.5;
                float outerRadius = 0.5;
                float edge = smoothstep(innerRadius, outerRadius, dist);
                
                // アルファの計算（1.0 - edge で外側に行くほど0になる）
                float alpha = tex.a * (1.0 - edge) * _Opacity;
                
                // 色を適用
                half4 col = _Color;
                col.a *= alpha;

                return col;
            }
            ENDHLSL
        }
    }
}