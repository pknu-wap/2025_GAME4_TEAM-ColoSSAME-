Shader "UI/House Banner"
{
    Properties
    {
        [PerRendererData] _MainTex ("Common frame", 2D) = "white" {}
        _HouseTex ("House artwork", 2D) = "white" {}
        _CrestRect ("Crest crop (left, top, width, height)", Vector) = (0, 0, 1, 1)
        _ClothUV ("Cloth sample (from top left)", Vector) = (0.5, 0.5, 0, 0)
        _StencilComp ("Stencil comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil operation", Float) = 0
        _StencilWriteMask ("Stencil write mask", Float) = 255
        _StencilReadMask ("Stencil read mask", Float) = 255
        _ColorMask ("Color mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use alpha clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Vertex { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Interpolator { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 local : TEXCOORD1; fixed4 color : COLOR; };
            sampler2D _MainTex, _HouseTex;
            float4 _CrestRect, _ClothUV, _HouseTex_TexelSize, _ClipRect;

            Interpolator vert(Vertex v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.local = v.vertex.xy;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            // Inside edge of the brass trim on the first house's 399 x 620 reference silhouette.
            float clothMask(float2 p)
            {
                float x = abs(p.x - 199.5);
                float top = max(60.0, max(90.0 - x, x - 45.0));
                float side = lerp(134.0, 117.0, saturate((p.y - 176.0) / 24.0));
                float bottom = 554.0 - x;
                float edge = min(side - x, min(p.y - top, bottom - p.y));
                if (p.y > 438.0)
                {
                    float finial = lerp(0.0, 28.0, saturate((p.y - 438.0) / 29.0));
                    finial = lerp(finial, 15.0, saturate((p.y - 467.0) / 13.0));
                    finial += max(0.0, p.y - 498.0);
                    edge = min(edge, x - finial);
                }
                return smoothstep(0.0, 2.0, edge);
            }

            fixed4 frag(Interpolator i) : SV_Target
            {
                fixed4 frame = tex2D(_MainTex, i.uv);
                float2 p = float2(i.uv.x, 1.0 - i.uv.y) * float2(399, 620);
                float2 cropSize = _CrestRect.zw * _HouseTex_TexelSize.zw;
                float scale = min(215.0 / cropSize.x, 332.0 / cropSize.y);
                float2 cropUV = (p - float2(199.5, 258)) / (cropSize * scale) + 0.5;
                float2 sourceUV = _CrestRect.xy + saturate(cropUV) * _CrestRect.zw;
                fixed3 crest = tex2D(_HouseTex, float2(sourceUV.x, 1.0 - sourceUV.y)).rgb;
                fixed3 cloth = tex2D(_HouseTex, float2(_ClothUV.x, 1.0 - _ClothUV.y)).rgb;
                float2 edge = min(cropUV, 1.0 - cropUV);
                float2 radial = abs(cropUV - 0.5) * 2.0;
                float cornerFade = 1.0 - smoothstep(0.94, 1.24, length(radial));
                float blend = smoothstep(0.0, 0.18, min(edge.x, edge.y)) * cornerFade;
                // Extend the cloth into the hem using a strip of the common frame's woven fabric.
                float weave = tex2D(_MainTex, float2(0.224 + 0.023 * cos(p.x * 0.035), 0.44 + 0.19 * cos(p.y * 0.016))).b;
                float referenceBlue = tex2D(_MainTex, float2(0.23, 0.55)).b;
                float thread = clamp(sqrt(weave / max(referenceBlue, 0.0001)), 0.66, 1.26);
                float shade = lerp(1.0, thread, 0.15) * (0.88 + 0.14 * cos((p.x - 199.5) * 0.012));
                fixed3 field = lerp(cloth * shade, crest, blend);
                fixed4 result = fixed4(lerp(frame.rgb, field, clothMask(p)), frame.a) * i.color;
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.local, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
