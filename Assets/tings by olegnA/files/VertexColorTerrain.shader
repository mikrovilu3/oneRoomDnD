// VertexColorTerrain.shader  —  Built-in Render Pipeline
// Reads vertex colours baked into the terrain mesh by TerrainChunk.
// Uses Unity's surface shader framework so you get lighting, shadows
// and fog for free without any extra packages.

Shader "Custom/VertexColorTerrain"
{
    Properties
    {
        _AmbientStrength  ("Ambient Strength",   Range(0,1)) = 0.4
        _FogColor         ("Horizon Fog Color",  Color)      = (0.65, 0.75, 0.85, 1)
        _FogStart         ("Fog Start Distance", Float)      = 600
        _FogEnd           ("Fog End Distance",   Float)      = 1400
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Lambert = diffuse-only lighting model; fullforwardshadows = receive shadows
        #pragma surface surf Lambert vertex:vert fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float4 color    : COLOR;   // vertex colour from mesh
            float3 worldPos;           // world-space position (auto-filled by Unity)
        };

        float  _AmbientStrength;
        float4 _FogColor;
        float  _FogStart;
        float  _FogEnd;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color    = v.color;
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Distance fog blended into albedo (quadratic ease-in)
            float dist   = distance(IN.worldPos, _WorldSpaceCameraPos);
            float fogT   = saturate((dist - _FogStart) / (_FogEnd - _FogStart));
            fogT = fogT * fogT;

            o.Albedo = lerp(IN.color.rgb, _FogColor.rgb, fogT);
            o.Alpha  = 1.0;
        }
        ENDCG
    }

    Fallback "Diffuse"
}
