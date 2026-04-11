using UnityEngine;

/// <summary>
/// Provides fractal Brownian Motion (fBm) noise with optional domain warping
/// for realistic, organic-looking terrain heightmaps.
/// </summary>
public static class NoiseGenerator
{
    /// <summary>
    /// Sample fBm noise at a world-space position.
    /// Returns a value roughly in [0, 1] (may exceed slightly due to octave layering).
    /// </summary>
    public static float Sample(float worldX, float worldZ, NoiseSettings settings)
    {
        float x = (worldX + settings.offset.x) * settings.scale;
        float z = (worldZ + settings.offset.y) * settings.scale;

        // Domain warping: distort input coordinates using a secondary noise pass
        // This breaks up the regularity of Perlin noise and creates natural-looking
        // overhangs, plateaus and river valleys.
        if (settings.useDomainWarping)
        {
            float warpStrength = settings.warpStrength;
            float wx = FBm(x + 1.7f, z + 9.2f, settings.octaves, settings.lacunarity, settings.persistence);
            float wz = FBm(x + 8.3f, z + 2.8f, settings.octaves, settings.lacunarity, settings.persistence);
            x += wx * warpStrength;
            z += wz * warpStrength;
        }

        float raw = FBm(x, z, settings.octaves, settings.lacunarity, settings.persistence);

        // Apply a power curve to redistribute heights (values > 1 exaggerate mountains)
        raw = Mathf.Pow(Mathf.Abs(raw), settings.redistribution);

        return raw;
    }

    /// <summary>
    /// Sample the biome blend weight at a position (uses a separate, smoother noise pass).
    /// Returns [0, 1] — 0 = plains/lowlands, 1 = mountains.
    /// </summary>
    public static float SampleBiome(float worldX, float worldZ, NoiseSettings settings)
    {
        float x = (worldX + settings.offset.x * 0.3f) * settings.scale * 0.4f;
        float z = (worldZ + settings.offset.y * 0.3f) * settings.scale * 0.4f;
        // Only 3 octaves — biomes transition slowly and smoothly
        float raw = FBm(x + 100f, z + 100f, 3, 2.0f, 0.5f);
        return Mathf.Clamp01(raw);
    }

    // ─── Internal ──────────────────────────────────────────────────────────────

    static float FBm(float x, float z, int octaves, float lacunarity, float persistence)
    {
        float value      = 0f;
        float amplitude  = 0.5f;
        float frequency  = 1f;
        float maxValue   = 0f;

        for (int i = 0; i < octaves; i++)
        {
            value     += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
            maxValue  += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return value / maxValue; // normalise to [0, 1]
    }
}

// ─── Settings ─────────────────────────────────────────────────────────────────

[System.Serializable]
public class NoiseSettings
{
    [Tooltip("Multiplier applied to world coordinates before sampling. Smaller = broader features.")]
    public float scale = 0.003f;

    [Tooltip("Translation of the noise field — change to get a different world.")]
    public Vector2 offset = Vector2.zero;

    [Range(1, 10)]
    [Tooltip("Number of noise layers stacked together.")]
    public int octaves = 7;

    [Tooltip("How quickly frequency increases each octave. 2 = classic fBm.")]
    public float lacunarity = 2.1f;

    [Range(0f, 1f)]
    [Tooltip("How quickly amplitude decreases each octave. 0.5 = classic fBm.")]
    public float persistence = 0.48f;

    [Range(0.5f, 3f)]
    [Tooltip("Power applied to the final value. >1 sharpens peaks; <1 flattens terrain.")]
    public float redistribution = 1.4f;

    [Tooltip("Distort input coordinates for more organic shapes.")]
    public bool useDomainWarping = true;

    [Range(0f, 3f)]
    public float warpStrength = 1.2f;
}
