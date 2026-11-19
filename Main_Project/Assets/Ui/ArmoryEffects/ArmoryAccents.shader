Shader "UI/Armory Accents"
{
    Properties
    {
        [PerRendererData] _MainTex ("Shield and cloth dye mask", 2D) = "white" {}
        _RoomTex ("Armory backdrop", 2D) = "black" {}
        _Crests ("Shield crests (5 columns, 2 rows)", 2D) = "white" {}
        _FromCrest ("Previous crest", Float) = 0
        _ToCrest ("Selected crest", Float) = 0
        _Elapsed ("Seconds since selection", Float) = 1
        _StencilComp ("Stencil comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil operation", Float) = 0
        _StencilWriteMask ("Stencil write mask", Float) = 255
        _StencilReadMask ("Stencil read mask", Float) = 255
        _ColorMask ("Color mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex, _RoomTex, _Crests;
            float _FromCrest, _ToCrest, _Elapsed;
            float4 _ClipRect;
            struct Vertex { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Interpolator { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 local : TEXCOORD1; fixed4 color : COLOR; };

            Interpolator vert(Vertex v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.local = v.vertex.xy;
                o.color = v.color;
                return o;
            }

            fixed4 crest(float slot, float2 local)
            {
                float2 cell = float2(fmod(slot, 5.0), 1.0 - floor(slot / 5.0));
                return tex2D(_Crests, (cell + float2(local.x, 1.0 - local.y)) / float2(5, 2));
            }

            // 좌표(p)는 TeamArmory.png를 1280x720으로 본 기준 픽셀값.
            // 방패 영역: x < 162, y > 515 / 문장 중심 (99, 581), 높이 116.
            // TeamArmory 그림을 바꾸면 이 숫자들을 다시 재야 한다.
            // 문장 쓸기 속도 4.6은 Carousel.CrestSwapDuration(0.76초) 안에 끝나도록 맞춘 값.
            fixed3 shieldPaint(float2 p, fixed3 wood)
            {
                float v = (p.y - 581.0) / 116.0;
                // Shear the print along the shield, below the raised metal boss.
                float2 local = float2((p.x - 99.0 - 14.0*v) / 99.0, v) + 0.5;
                if (any(local < 0.0) || any(local > 1.0)) return wood;
                float sweep = _Elapsed * 4.6 - (p.y - 525.0)/135.0 - (p.x - 95.0)/320.0;
                float reveal = smoothstep(0.0, 0.28, sweep);
                fixed4 oldInk = crest(_FromCrest, local);
                fixed4 newInk = crest(_ToCrest, local);
                float grain = 0.78 + 0.39 * sqrt(saturate(wood.r));
                fixed3 before = lerp(wood, oldInk.rgb * grain, oldInk.a * 0.93);
                fixed3 after = lerp(wood, newInk.rgb * grain, newInk.a * 0.93);
                float sheen = saturate(1.0 - abs(sweep - 0.12) / 0.20) * newInk.a;
                return lerp(before, after, reveal) + newInk.rgb * (0.22 * sheen);
            }

            // The backdrop already contains a red cloth. Cover its exposed edge
            // with nearby stone when the dyed hem lifts out of its original place.
            fixed3 exposedStone(float2 uv, float2 units)
            {
                float2 down = saturate(uv - float2(0, 22) * units);
                float2 side = saturate(uv + float2(uv.x < 0.89 ? -15 : 15, -2) * units);
                float downMask = tex2D(_MainTex, down).a;
                float sideMask = tex2D(_MainTex, side).a;
                return tex2D(_RoomTex, downMask <= sideMask ? down : side).rgb;
            }

            // 천 영역: x > 1060, y > 515 (1280x720 기준).
            // 천 흔들림 길이 0.70초(+시작 지연 0.06초)는 Carousel.CrestSwapDuration(0.76초)와 같이 바꿔야 한다.
            fixed4 frag(Interpolator i) : SV_Target
            {
                const float2 size = float2(1280, 720);
                float2 p = float2(i.uv.x, 1.0 - i.uv.y) * size;
                fixed4 dye = tex2D(_MainTex, i.uv);
                fixed4 result = fixed4(dye.rgb * i.color.rgb, dye.a);
                if (p.x < 162.0 && p.y > 515.0)
                    result.rgb = shieldPaint(p, result.rgb);

                float age = _Elapsed - 0.06;
                if (p.x > 1060.0 && p.y > 515.0 && age > 0.0 && age < 0.70)
                {
                    float drop = saturate((p.y - 515.0) / 188.0);
                    float amp = sin(age * UNITY_PI / 0.70) * exp(-2.7 * age);
                    float sway = 21 * sin(age*11.5 - drop*1.7);
                    float lift = 28 * (0.67 + 0.33 * sin(p.x*0.037 - age*5));
                    float2 uv = saturate(i.uv - float2(sway,lift) * (amp*drop*drop) / size);
                    fixed4 hem = tex2D(_MainTex, uv);
                    float shade = 1.0 + 0.11*amp*drop*sin((p.x-1070)*0.09 + age*10);
                    result.rgb = hem.rgb * i.color.rgb * shade;
                    result.a = hem.a;
                    if (dye.a > 0.001 && hem.a < 0.999)
                    {
                        float exposed = dye.a * (1.0 - hem.a);
                        result.a += exposed;
                        result.rgb = (result.rgb * hem.a + exposedStone(i.uv, 1.0/size) * exposed) / result.a;
                    }
                }
                result.a *= i.color.a;
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.local, _ClipRect);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
