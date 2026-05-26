Shader "Custom/Brush_URP_WithTexture"
{
    Properties
    {
        // インスペクターからブラシテクスチャを指定できるようにします
        _MainTex ("Brush Texture", 2D) = "white" {}
        _Color ("Brush Color", Color) = (1,1,1,1)
        _Hardness ("Hardness (Falloff)", Range(0,1)) = 1.0 // 1.0でテクスチャそのまま。下げるほど周囲がボケる
        _Opacity ("Opacity", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "BrushWithTex"
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
                // 1. 指定されたブラシテクスチャをサンプリング
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // 2. 基本のアルファ（透明度）を計算
                // テクスチャ自体のアルファ、またはRGBの輝度（黒を透明にしたい場合など）を使用できます
                // ここでは一般的な「テクスチャのアルファチャンネル」を使用します
                float texAlpha = tex.a;

                // 3. 補助的な円形ボケ（Hardness）の計算
                // テクスチャの形状をそのまま出したい場合は、インスペクター側で _Hardness を「1.0」にしてください
                float dist = length(i.uv - 0.5);
                float innerRadius = _Hardness * 0.5;
                float outerRadius = 0.5;
                float edge = smoothstep(innerRadius, outerRadius, dist);

                // 4. すべての不透明度を掛け合わせる
                float finalAlpha = texAlpha * (1.0 - edge) * _Opacity;

                // 5. カラーの適用
                // テクスチャが白黒のマスク画像（ディテール用のグレースケール）の場合、
                // tex.rgb を _Color.rgb に掛けることで、テクスチャの質感を残したまま自由に着色できます
                half4 col;
                // col.rgb = _Color.rgb;
                // col.a = _Color.a * finalAlpha;
                // ★【重要】RGBに対して、あらかじめアルファ値を掛け算（乗算）して出力する
                col.rgb = _Color.rgb * finalAlpha; 
                col.a = finalAlpha; // アルファはそのまま

                return col;
            }
            ENDHLSL
        }
    }
}