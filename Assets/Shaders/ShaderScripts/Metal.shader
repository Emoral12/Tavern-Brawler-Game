Shader "Custom/Metal"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MetallicTex ("Metal (R)", 2D) = "white" {}
        _Metallic ("Metal", Range(0,1)) = 0.0
        _SpecColor ("Specular", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
        }

        CGPROGRAM
        #pragma surface surf StandardSpecular
        
        sampler2D _MetalTex;
        half _Metal;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MetalTex;
        };

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            o.Albedo = _Color.rgb;
            o.Smoothness = tex2D(_MetalTex, IN.uv_MetalTex).r;
            o.Specular = _SpecColor.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
