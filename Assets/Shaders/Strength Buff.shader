Shader "Custom/StrengthBuff"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _StrengthGlow ("Strength Glow Amount", Range(0,1))=0
        _GlowColor ("Glow Color", Color)= (1,0.35,0,1)
        _GlowIntensity( "Glow Intensity", Range(0,10))=3
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode"= "UniversalForward"}
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                
            };


            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _StrengthGlow;
                float4 _GlowColor;
                float _GlowIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 basecolor = _BaseColor.rgb;
                float3 glow= _GlowColor.rgb *_GlowIntensity* _StrengthGlow;
                return half4(basecolor+ glow, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
