Shader "Harmony/TBGSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _CutterTex ("Sprite Texture", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [Toggle] _CutterEnabled("Enable Cutter", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVertHarmony
            #pragma fragment SpriteFragHarmony
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            struct harmony_appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct harmony_v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            harmony_v2f SpriteVertHarmony(harmony_appdata_t IN)
            {
                harmony_v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;

                #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                
                return OUT;
            }

            float4x4 _TexCoordToCutterCoord;
            float4 _CutterRect;
            sampler2D _CutterTex;
            int _CutterInverse;
            float _CutterEnabled;

            fixed4 SpriteFragHarmony(harmony_v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                float2 cutterCoord = mul(_TexCoordToCutterCoord, float4(IN.texcoord, 0, 1)).xy;
                float cutterAlpha = tex2D(_CutterTex, cutterCoord).a;
                if (any(_CutterRect.zw == 0) || any(floor((cutterCoord - _CutterRect.xy) / _CutterRect.zw) != 0)) {
                    cutterAlpha = 0;
                }
                c.a *= lerp(1,  lerp(1 - cutterAlpha, cutterAlpha, _CutterInverse),  _CutterEnabled);
                c.rgb *= c.a;
                return c;
            }

            ENDCG
        }
    }
}

