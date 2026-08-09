Shader "CyberRakshak/UI/PauseBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Range(0, 12)) = 6
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 texcoord : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurRadius;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurRadius;
                fixed4 color = tex2D(_MainTex, i.texcoord) * 0.20;
                color += tex2D(_MainTex, i.texcoord + float2(offset.x, 0)) * 0.10;
                color += tex2D(_MainTex, i.texcoord - float2(offset.x, 0)) * 0.10;
                color += tex2D(_MainTex, i.texcoord + float2(0, offset.y)) * 0.10;
                color += tex2D(_MainTex, i.texcoord - float2(0, offset.y)) * 0.10;
                color += tex2D(_MainTex, i.texcoord + offset) * 0.10;
                color += tex2D(_MainTex, i.texcoord - offset) * 0.10;
                color += tex2D(_MainTex, i.texcoord + float2(offset.x, -offset.y)) * 0.10;
                color += tex2D(_MainTex, i.texcoord + float2(-offset.x, offset.y)) * 0.10;
                color.a = 1;
                return color * i.color;
            }
            ENDCG
        }
    }
}