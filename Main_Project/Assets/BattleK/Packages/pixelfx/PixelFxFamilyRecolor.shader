// 파란 기준색(_From 8단계)을 가문 색(_To)으로 바꿔 칠함. 흰색, 회색은 그대로
Shader "PixelFx/FamilyRecolor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _MinSat ("Min Saturation", Range(0, 1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _MinSat;
            float4 _From[8]; // 감마 공간 값 그대로
            float4 _To[8];

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 t = tex2D(_MainTex, i.uv);
                float3 c = t.rgb;
            #ifndef UNITY_COLORSPACE_GAMMA
                c = LinearToGammaSpace(c);
            #endif
                float mx = max(c.r, max(c.g, c.b)), mn = min(c.r, min(c.g, c.b));
                if (mx > 0.001 && (mx - mn) / mx >= _MinSat)
                {
                    float best = 1e5;
                    float3 to = c;
                    for (int k = 0; k < 8; k++)
                    {
                        float3 d = c - _From[k].rgb;
                        float dd = dot(d, d);
                        if (dd < best) { best = dd; to = _To[k].rgb; }
                    }
                    c = to;
                }
            #ifndef UNITY_COLORSPACE_GAMMA
                c = GammaToLinearSpace(c);
            #endif
                return fixed4(c, t.a) * i.color;
            }
            ENDCG
        }
    }
}
