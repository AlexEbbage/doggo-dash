Shader "Game/CurvedWorld/Unlit"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)

        _CurveStrength ("Curve Strength", Float) = 0.0025
        _CurveStartZ ("Curve Start Z (world)", Float) = 10.0
        _CurveAxis ("Curve Axis (world X bend)", Float) = 1.0

        _VerticalStrength ("Vertical Strength", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;

                float _CurveStrength;
                float _CurveStartZ;
                float _CurveAxis;
                float _VerticalStrength;
            CBUFFER_END

            float _GlobalCurveStrength;
            float _GlobalCurveStartZ;
            float _GlobalVerticalStrength;

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

            float3 ApplyCurvature(float3 worldPos)
            {
                float curveStrength = (_GlobalCurveStrength != 0.0) ? _GlobalCurveStrength : _CurveStrength;
                float curveStartZ   = (_GlobalCurveStartZ   != 0.0) ? _GlobalCurveStartZ   : _CurveStartZ;
                float vertStrength  = (_GlobalVerticalStrength != 0.0) ? _GlobalVerticalStrength : _VerticalStrength;

                float dz = max(0.0, worldPos.z - curveStartZ);
                float bend = dz * dz * curveStrength;

                worldPos.x += bend * _CurveAxis;
                worldPos.y -= dz * dz * vertStrength;

                return worldPos;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                worldPos = ApplyCurvature(worldPos);

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return albedo;
            }
            ENDHLSL
        }
    }
}
