Shader "Custom/LayerComposite"
{
    Properties
    {
        _MainTex ("Base (Current Combined)", 2D) = "white" {}
        _OverlayTex ("Overlay Layer", 2D) = "white" {}
        _LayerOpacity ("Layer Opacity", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D(_OverlayTex);
        SAMPLER(sampler_OverlayTex);

        float _LayerOpacity;

        Varyings vert(Attributes v)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            o.uv = v.uv;
            return o;
        }
        ENDHLSL

        // Pass 0: 1枚目のレイヤーをベーステクスチャにコピーする用
        Pass
        {
            Name "BaseCopy"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_copy

            half4 frag_copy(Varyings i) : SV_Target
            {
                // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // col.a *= _LayerOpacity;
                // return col;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float finalAlpha = col.a * _LayerOpacity;
    
                // ★ここでもRGBにアルファを乗算する
                col.rgb = col.rgb * finalAlpha;
                col.a = finalAlpha;
                return col;
            }
            ENDHLSL
        }

        // Pass 1: 下の絵（MainTex）に上のレイヤー（OverlayTex）をアルファブレンドする用
        Pass
        {
            Name "LayerBlend"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_blend

            half4 frag_blend(Varyings i) : SV_Target
            {
                // 下の画像（すでに合成済みの結果）
                half4 dst = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                // 上に重ねる新しいレイヤー
                half4 src = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, i.uv);
                
                // レイヤーの不透明度をアルファに乗算
                float srcAlpha = src.a * _LayerOpacity;

                // 通常レイヤーのデジタルアルファブレンド公式 (Pre-multiplied Alpha を考慮)
                half4 outColor;
                outColor.a = srcAlpha + dst.a * (1.0 - srcAlpha);
                
                if (outColor.a > 0.0)
                {
                    // 下の絵と上の絵のRGBを、アルファの比率に応じて正しく補間合成
                    outColor.rgb = (src.rgb * srcAlpha + dst.rgb * dst.a * (1.0 - srcAlpha)) / outColor.a;
                }
                else
                {
                    outColor.rgb = float3(1, 1, 1); // 完全に透明な場合は白にしておく
                }

                return outColor;
            }
            ENDHLSL
        }
    }
}