using UnityEngine;

/// <summary>
/// ScriptableObject holding every parameter that controls terrain generation.
/// Create via: Assets → Create → Terrain → Settings
/// </summary>
[CreateAssetMenu(menuName = "Terrain/Settings", fileName = "TerrainSettings")]
public class TerrainSettings : ScriptableObject
{
    // ─── Chunk Layout ──────────────────────────────────────────────────────────

    [Header("Chunk Layout")]
    [Tooltip("Number of vertices along each edge of a chunk. Must be (2^n + 1) for clean LOD. 129 is good.")]
    public int chunkResolution = 129;

    [Tooltip("World-space size of each chunk in meters.")]
    public float chunkSize = 200f;

    [Tooltip("How many chunks in each direction from the player to keep loaded. 4 = 9×9 = 81 chunks.")]
    [Range(1, 8)]
    public int viewDistance = 5;

    // ─── Height ────────────────────────────────────────────────────────────────

    [Header("Height")]
    [Tooltip("Maximum terrain height in meters.")]
    public float maxHeight = 160f;

    [Tooltip("Minimum terrain height (sea floor baseline).")]
    public float minHeight = -10f;

    [Tooltip("Height of the water plane.")]
    public float waterLevel = 10f;

    // ─── Noise ─────────────────────────────────────────────────────────────────

    [Header("Noise")]
    public NoiseSettings noiseSettings = new NoiseSettings();

    // ─── Biome Colors (vertex-painted) ─────────────────────────────────────────

    [Header("Biome Colors")]
    public Gradient plainsGradient   = DefaultPlainsGradient();
    public Gradient mountainGradient = DefaultMountainGradient();

    [Tooltip("How much the slope influences colour selection (0 = ignore slope).")]
    [Range(0f, 1f)]
    public float slopeInfluence = 0.6f;

    [Tooltip("Colour of deep water.")]
    public Color deepWaterColor   = new Color(0.05f, 0.18f, 0.38f);

    [Tooltip("Colour of shallow water / shore.")]
    public Color shallowWaterColor = new Color(0.13f, 0.42f, 0.60f);

    // ─── Foliage ───────────────────────────────────────────────────────────────

    [Header("Foliage")]
    [Tooltip("Prefabs randomly placed on terrain. Assign trees, rocks, grass clumps etc.")]
    public GameObject[] foliagePrefabs;

    [Range(0, 64)]
    [Tooltip("Maximum foliage objects spawned per chunk.")]
    public int foliagePerChunk = 24;

    [Tooltip("Only spawn foliage above this height (avoids water).")]
    public float foliageMinHeight = 12f;

    [Tooltip("Only spawn foliage where slope (degrees) is below this value.")]
    public float foliageMaxSlope = 30f;

    // ─── LOD ───────────────────────────────────────────────────────────────────

    [Header("Level of Detail")]
    [Tooltip("Chunk distances (in chunks) at which the mesh simplification factor doubles. " +
             "Leave empty to disable LOD.")]
    public int[] lodThresholds = { 2, 4, 6 };

    // ─── Default gradient helpers ──────────────────────────────────────────────

    static Gradient DefaultPlainsGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.19f, 0.49f, 0.20f), 0.00f), // lush grass
                new GradientColorKey(new Color(0.44f, 0.61f, 0.26f), 0.30f), // lighter grass
                new GradientColorKey(new Color(0.62f, 0.56f, 0.36f), 0.60f), // dry grass / dirt
                new GradientColorKey(new Color(0.68f, 0.62f, 0.48f), 1.00f), // sandy hilltop
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            }
        );
        return g;
    }

    static Gradient DefaultMountainGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.40f, 0.36f, 0.30f), 0.00f), // dark rock
                new GradientColorKey(new Color(0.57f, 0.53f, 0.46f), 0.45f), // mid rock
                new GradientColorKey(new Color(0.78f, 0.77f, 0.75f), 0.75f), // light grey rock
                new GradientColorKey(new Color(0.97f, 0.97f, 1.00f), 1.00f), // snow cap
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            }
        );
        return g;
    }
}
