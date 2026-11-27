Shader "UI/Armory Accents"
{
    Properties
    {
        [PerRendererData] _MainTex ("Shield and cloth dye mask", 2D) = "white" {}
        _RoomTex ("Armory backdrop", 2D) = "black" {}
        _Crests ("Shield crests (5 columns, 2 rows)", 2D) = "white" {}
        _CrestMotion ("Stroke timing and particles", 2D) = "black" {}
        _FromAge ("Outgoing stroke age", Float) = 1
        _FromOpacity ("Outgoing opacity", Float) = 1
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

            sampler2D _MainTex, _RoomTex, _Crests, _CrestMotion;
            float _FromCrest, _ToCrest, _Elapsed, _FromAge, _FromOpacity;
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

            // Each pair of houses shares 24 frames (12 columns by 2 rows).
            // RG holds timing and light for the even slot; BA holds them for the odd slot.
            float2 motionSample(float2 p, float house, float frame)
            {
                float2 pixel = clamp(p - float2(38, 506), 0.5, float2(119.5,143.5));
                float2 tile = float2(fmod(frame,12), floor(house/2)*2 + floor(frame/12));
                float2 uv = (tile + pixel/float2(120,144)) / float2(12,10);
                float4 data = tex2D(_CrestMotion, float2(uv.x,1-uv.y));
                return fmod(house,2) < 1 ? data.rg : data.ba;
            }

            float2 motionData(float2 p, float house, float age)
            {
                float frame = saturate(age / 0.76) * 23;
                float2 first = motionSample(p,house,floor(frame));
                float next = motionSample(p,house,min(floor(frame)+1,23)).y;
                return float2(first.x * 0.76, lerp(first.y,next,frac(frame)));
            }

            float3 strokeLight(float house)
            {
                if (house == 0) return float3(0.60,0.84,0.92); // Caelus
                if (house == 1) return float3(0.53,0.91,0.30); // Flora
                if (house == 2) return float3(1.00,0.43,0.06); // Ignis
                if (house == 3) return float3(1.00,0.90,0.65); // Lumen
                if (house == 4) return float3(0.70,0.70,0.96); // Nox
                if (house == 5) return float3(0.62,0.48,0.86); // Mors
                if (house == 6) return float3(0.95,0.84,0.35); // Fulger
                if (house == 7) return float3(0.62,0.95,1.00); // Mare
                if (house == 8) return float3(0.82,0.61,0.40); // Terra
                return float3(1.00,0.88,0.54);                 // Astra
            }

            float paintedFraction(float2 p, float house, float age)
            {
                if (age >= 0.75) return 1;
                float arrival = motionSample(p,house,0).x * 0.76;
                return smoothstep(-0.014,0.040,age-arrival);
            }

            // 좌표(p)는 TeamArmory.png를 1280x720으로 본 기준 픽셀값.
            // 방패 영역: x 38~158, y 506~650 / 문장 중심 (99, 581), 높이 116. CrestMotion 타일도 이 영역(120x144) 기준.
            // TeamArmory 그림을 바꾸면 이 숫자들을 다시 재야 한다.
            fixed3 shieldPaint(float2 p, fixed3 wood)
            {
                float v = (p.y - 581.0) / 116.0;
                // Shear the print along the shield, below the raised metal boss.
                float2 local = float2((p.x - 99.0 - 14.0*v) / 99.0, v) + 0.5;
                bool onTile = all(local >= 0.0) && all(local <= 1.0);
                fixed4 newInk = crest(_ToCrest,saturate(local)) * onTile;
                float grain = 0.78 + 0.39 * sqrt(saturate(wood.r));
                fixed3 after = lerp(wood,newInk.rgb*grain,newInk.a*0.93);
                if (_Elapsed >= 0.75) return after;

                fixed4 oldInk = crest(_FromCrest,saturate(local)) * onTile;
                float previousAlpha = _FromOpacity * paintedFraction(p,_FromCrest,_FromAge) * oldInk.a;
                fixed3 before = lerp(wood,oldInk.rgb*grain,previousAlpha*0.93);
                float2 motion = motionData(p,_ToCrest,_Elapsed);
                float dt = _Elapsed-motion.x;
                before = lerp(before,wood,smoothstep(0,0.15,_Elapsed));
                fixed3 surface = lerp(before,after,smoothstep(-0.014,0.040,dt));
                if (_ToCrest == 4 || _ToCrest == 5)
                {
                    float shadow = (dt+0.027)/0.048;
                    surface *= 1-0.64*exp(-shadow*shadow)*newInk.a;
                }

                float3 light = strokeLight(_ToCrest);
                float edge = (dt-0.014)/0.032;
                float highlight = 0.48*exp(-edge*edge)*newInk.a + motion.y;
                return surface + (1-saturate(surface))*light*saturate(highlight);
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
                // Remap the new upper shield face to the existing animated crest atlas.
                float2 crestPixel = (p - float2(53, 277)) / 0.72 + float2(99, 581);
                if (all(crestPixel >= float2(38, 506)) && all(crestPixel < float2(158, 650)))
                    result.rgb = shieldPaint(crestPixel, result.rgb);

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
