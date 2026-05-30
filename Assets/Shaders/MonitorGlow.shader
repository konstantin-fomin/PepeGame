Shader "PepeIdle/MonitorGlow"
{
    Properties
    {
        // Required by Unity UI (CanvasRenderer feeds the sprite through _MainTex).
        // The glow itself is procedural, so the texture is not sampled.
        _MainTex   ("Sprite Texture", 2D) = "white" {}
        _Color     ("Glow Color",    Color)  = (0.15, 0.9, 0.4, 1)
        _Intensity ("Intensity",     Range(0, 2)) = 0.4
        _Falloff   ("Falloff",       Range(0.5, 4)) = 1.8
        _ScaleX    ("Scale X",       Range(0.1, 2)) = 1.0
        _ScaleY    ("Scale Y",       Range(0.1, 2)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"
               "IgnoreProjector"="True" "PreviewType"="Plane" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float  _Intensity;
            float  _Falloff;
            float  _ScaleX;
            float  _ScaleY;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 centered = (IN.uv - 0.5) * 2.0;
                centered.x *= _ScaleX;
                centered.y *= _ScaleY;

                float dist  = length(centered);
                float glow  = pow(saturate(1.0 - dist), _Falloff);
                float alpha = glow * _Intensity;

                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
