Shader "Custom/MosaicSurfaceRigidURP"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _BaseColor ("Base Color", Color) = (0.68, 0.62, 0.56, 1)
        _AccentColor ("Accent Color", Color) = (0.87, 0.82, 0.72, 1)
        _GroutColor ("Grout Color", Color) = (0.94, 0.92, 0.88, 1)

        _PatternScale ("Pattern Scale", Range(0.5, 20)) = 4
        _EdgeThickness ("Edge Thickness", Range(0.001, 0.2)) = 0.05
        _ColorVariation ("Color Variation", Range(0, 1)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float4 _AccentColor;
                float4 _GroutColor;
                float _PatternScale;
                float _EdgeThickness;
                float _ColorVariation;
            CBUFFER_END

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float2 hash22(float2 p)
            {
                float n = hash21(p);
                return float2(n, hash21(p + 19.19));
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionHCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            void VoronoiSimple(float2 uv, out float cellValue, out float edgeValue)
            {
                float2 gv = floor(uv);
                float2 lv = frac(uv);

                float nearest = 999.0;
                float secondNearest = 999.0;
                float nearestId = 0.0;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offs = float2(x, y);
                        float2 cell = gv + offs;
                        float2 p = offs + hash22(cell);

                        float2 diff = p - lv;
                        float d = dot(diff, diff);

                        if (d < nearest)
                        {
                            secondNearest = nearest;
                            nearest = d;
                            nearestId = hash21(cell);
                        }
                        else if (d < secondNearest)
                        {
                            secondNearest = d;
                        }
                    }
                }

                cellValue = nearestId;
                edgeValue = sqrt(secondNearest) - sqrt(nearest);
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Patró en WORLD SPACE perquè no es deformi quan escalis el sprite
                float2 patternUV = i.positionWS.xy * _PatternScale;

                float cellValue;
                float edgeValue;
                VoronoiSimple(patternUV, cellValue, edgeValue);

                float groutMask = 1.0 - smoothstep(_EdgeThickness, _EdgeThickness + 0.02, edgeValue);

                float3 tileColor = lerp(_BaseColor.rgb, _AccentColor.rgb, cellValue);

                float variation = (hash21(float2(cellValue, cellValue + 2.71)) - 0.5) * 2.0 * _ColorVariation;
                tileColor *= (1.0 + variation);

                float3 finalColor = lerp(tileColor, _GroutColor.rgb, groutMask);

                return half4(finalColor, tex.a) * i.color;
            }
            ENDHLSL
        }
    }
}