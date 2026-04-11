// AnimatedWater.shader  —  Built-in Render Pipeline
// Transparent water surface with scrolling normals, Fresnel depth blend,
// and vertex-animated waves. No packages required.

Shader "Custom/AnimatedWater"
{
    Properties
    {
        _ShallowColor  ("Shallow Color",      Color)        = (0.13, 0.55, 0.72, 0.65)
        _DeepColor     ("Deep Color",         Color)        = (0.03, 0.15, 0.38, 0.92)
        _FresnelPower  ("Fresnel Power",      Range(1, 8))  = 3
        _WaveSpeed     ("Wave Speed",         Range(0, 2))  = 0.35
        _WaveScale     ("Normal Map Scale",   Range(0.01, 0.5)) = 0.08
        _WaveHeight    ("Vertex Wave Height", Range(0, 1))  = 0.12
        _Smoothness    ("Smoothness",         Range(0, 1))  = 0.88
        _NormalMap     ("Normal Map A",       2D)           = "bump" {}
        _NormalMapB    ("Normal Map B",       2D)           = "bump" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf BlinnPhong vertex:vert alpha:fade
        #pragma target 3.0

        sampler2D _NormalMap;
        sampler2D _NormalMapB;

        float4 _ShallowColor;
        float4 _DeepColor;
        float  _FresnelPower;
        float  _WaveSpeed;
        float  _WaveScale;
        float  _WaveHeight;
        float  _Smoothness;

        struct Input
        {
            float2 uv_NormalMap;
            float3 worldPos;
            float3 viewDir;
        };

        void vert(inout appdata_full v)
        {
            float t  = _Time.y * _WaveSpeed;
            float wx = mul(unity_ObjectToWorld, v.vertex).x;
            float wz = mul(unity_ObjectToWorld, v.vertex).z;
            v.vertex.y += sin(wx * 0.08 + t * 1.3) * _WaveHeight;
            v.vertex.y += sin(wz * 0.11 + t * 0.9) * _WaveHeight * 0.7;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float t = _Time.y * _WaveSpeed;

            // Two scrolling normal samples at different speeds/angles
            float2 uvA = IN.uv_NormalMap * _WaveScale + float2( t * 0.08,  t * 0.06);
            float2 uvB = IN.uv_NormalMap * _WaveScale * 1.4 + float2(-t * 0.05, -t * 0.09);

            float3 nA = UnpackNormal(tex2D(_NormalMap,  uvA));
            float3 nB = UnpackNormal(tex2D(_NormalMapB, uvB));
            o.Normal = normalize(nA + nB);

            // Fresnel: how much we're looking along the surface vs. straight down
            float NdotV  = saturate(dot(o.Normal, normalize(IN.viewDir)));
            float fresnel = pow(1.0 - NdotV, _FresnelPower);

            // Blend shallow/deep based on fresnel angle
            float4 waterColor = lerp(_ShallowColor, _DeepColor, fresnel);

            o.Albedo     = waterColor.rgb;
            o.Alpha      = waterColor.a;
            o.Specular   = _Smoothness;
            o.Gloss      = _Smoothness;
        }
        ENDCG
    }

    Fallback "Transparent/Diffuse"
}
