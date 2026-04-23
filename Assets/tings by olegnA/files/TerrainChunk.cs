using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single terrain chunk. Generates a smooth mesh from noise,
/// applies vertex colours, adds a water plane, and optionally spawns foliage.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    // ─── Public state ──────────────────────────────────────────────────────────

    public Vector2Int chunkCoord { get; private set; }
    public bool       isReady    { get; private set; }

    // ─── Private refs ──────────────────────────────────────────────────────────

    TerrainSettings settings;
    MeshFilter      meshFilter;
    MeshRenderer    meshRenderer;
    MeshCollider    meshCollider;
    GameObject      waterPlane;

    List<GameObject> spawnedFoliage = new List<GameObject>();

    // ─── Initialisation ────────────────────────────────────────────────────────

    public void Initialise(Vector2Int coord, TerrainSettings settings, Material terrainMat, Material waterMat)
    {
        this.chunkCoord = coord;
        this.settings   = settings;

        meshFilter   = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        meshRenderer.material = terrainMat;

        // Position chunk so that (coord * chunkSize) is at the chunk's corner
        float s = settings.chunkSize;
        transform.position = new Vector3(coord.x * s, 0f, coord.y * s);

        // Water plane ─────────────────────────────────────────────────────────
        waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterPlane.name = "Water";
        waterPlane.transform.SetParent(transform);
        waterPlane.transform.localPosition = new Vector3(s * 0.5f, settings.waterLevel, s * 0.5f);
        waterPlane.transform.localScale    = new Vector3(s * 0.1f, 1f, s * 0.1f); // Plane is 10 units wide
        waterPlane.GetComponent<MeshRenderer>().material = waterMat;
        Destroy(waterPlane.GetComponent<Collider>());
    }

    // ─── Mesh Generation ──────────────────────────────────────────────────────

    /// <summary>
    /// Build the terrain mesh. lodStep controls vertex spacing:
    ///   lodStep = 1 → full resolution (all vertices)
    ///   lodStep = 2 → half resolution (every other vertex)
    /// </summary>
    public void GenerateMesh(int lodStep = 1)
    {
        int res = settings.chunkResolution;
        // Ensure lodStep divides evenly into (res - 1)
        lodStep = Mathf.Clamp(lodStep, 1, (res - 1));
        while ((res - 1) % lodStep != 0) lodStep--;

        int verts1D   = (res - 1) / lodStep + 1;
        int vertCount = verts1D * verts1D;
        int quadCount = (verts1D - 1) * (verts1D - 1);

        Vector3[] positions  = new Vector3[vertCount];
        Vector2[] uvs        = new Vector2[vertCount];
        Color[]   colors     = new Color[vertCount];
        int[]     triangles  = new int[quadCount * 6];

        float chunkSize = settings.chunkSize;
        float step      = chunkSize / (res - 1) * lodStep;
        float originX   = chunkCoord.x * chunkSize;
        float originZ   = chunkCoord.y * chunkSize;

        // Sample heights ──────────────────────────────────────────────────────
        float[] heights = new float[vertCount];
        for (int z = 0; z < verts1D; z++)
        for (int x = 0; x < verts1D; x++)
        {
            int   idx = z * verts1D + x;
            float wx  = originX + x * step;
            float wz  = originZ + z * step;
            heights[idx] = SampleHeight(wx, wz);
        }

        // Build vertices & vertex colours ─────────────────────────────────────
        for (int z = 0; z < verts1D; z++)
        for (int x = 0; x < verts1D; x++)
        {
            int   idx    = z * verts1D + x;
            float height = heights[idx];

            positions[idx] = new Vector3(x * step, height, z * step);
            uvs[idx]       = new Vector2((float)x / (verts1D - 1), (float)z / (verts1D - 1));
            colors[idx]    = SampleColor(originX + x * step, originZ + z * step, height, heights, idx, verts1D, step);
        }

        // Build triangle indices ───────────────────────────────────────────────
        int triIdx = 0;
        for (int z = 0; z < verts1D - 1; z++)
        for (int x = 0; x < verts1D - 1; x++)
        {
            int tl = z * verts1D + x;
            int tr = tl + 1;
            int bl = tl + verts1D;
            int br = bl + 1;

            // Alternate diagonal direction for a more symmetric look
            if ((x + z) % 2 == 0)
            {
                triangles[triIdx++] = tl; triangles[triIdx++] = bl; triangles[triIdx++] = tr;
                triangles[triIdx++] = tr; triangles[triIdx++] = bl; triangles[triIdx++] = br;
            }
            else
            {
                triangles[triIdx++] = tl; triangles[triIdx++] = bl; triangles[triIdx++] = br;
                triangles[triIdx++] = tl; triangles[triIdx++] = br; triangles[triIdx++] = tr;
            }
        }

        // Assign mesh ─────────────────────────────────────────────────────────
        Mesh mesh = new Mesh();
        mesh.name = $"Chunk_{chunkCoord.x}_{chunkCoord.y}_lod{lodStep}";
        if (vertCount > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices  = positions;
        mesh.uv        = uvs;
        mesh.colors    = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh  = mesh;
        meshCollider.sharedMesh = lodStep == 1 ? mesh : null; // collider only at full LOD

        isReady = true;
    }

    // ─── Foliage ──────────────────────────────────────────────────────────────

    public void SpawnFoliage()
    {
        if (settings.foliagePrefabs == null || settings.foliagePrefabs.Length == 0) return;

        ClearFoliage();

        float chunkSize = settings.chunkSize;
        float originX   = chunkCoord.x * chunkSize;
        float originZ   = chunkCoord.y * chunkSize;

        // Use chunk coord as seed for deterministic placement
        Random.InitState(chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663);

        for (int i = 0; i < settings.foliagePerChunk; i++)
        {
            float lx = Random.Range(0f, chunkSize);
            float lz = Random.Range(0f, chunkSize);
            float h  = SampleHeight(originX + lx, originZ + lz);

            if (h < settings.foliageMinHeight) continue;

            // Approximate slope via central differences
            float slope = ApproximateSlope(originX + lx, originZ + lz);
            if (slope > settings.foliageMaxSlope) continue;

            GameObject prefab = settings.foliagePrefabs[Random.Range(0, settings.foliagePrefabs.Length)];
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, transform);
            go.transform.localPosition = new Vector3(lx, h, lz);
            go.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float scale = Random.Range(0.7f, 1.3f);
            go.transform.localScale = Vector3.one * scale;

            spawnedFoliage.Add(go);
        }
    }

    public void ClearFoliage()
    {
        foreach (var go in spawnedFoliage)
            if (go != null) Destroy(go);
        spawnedFoliage.Clear();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    float SampleHeight(float wx, float wz)
    {
        float n = NoiseGenerator.Sample(wx, wz, settings.noiseSettings);
        return Mathf.Lerp(settings.minHeight, settings.maxHeight, n);
    }

    float ApproximateSlope(float wx, float wz)
    {
        float d    = settings.chunkSize / (settings.chunkResolution - 1);
        float hL   = SampleHeight(wx - d, wz);
        float hR   = SampleHeight(wx + d, wz);
        float hD   = SampleHeight(wx, wz - d);
        float hU   = SampleHeight(wx, wz + d);
        float dfdx = (hR - hL) / (2f * d);
        float dfdz = (hU - hD) / (2f * d);
        return Mathf.Atan(Mathf.Sqrt(dfdx * dfdx + dfdz * dfdz)) * Mathf.Rad2Deg;
    }

    Color SampleColor(float wx, float wz, float height, float[] heights, int idx, int verts1D, float step)
    {
        // Height as 0–1 relative to terrain range
        float heightT = Mathf.InverseLerp(settings.minHeight, settings.maxHeight, height);

        // Slope (0–1, where 1 = 90°)
        float slopeDeg = ApproximateSlope(wx, wz);
        float slopeT   = Mathf.Clamp01(slopeDeg / 90f);

        // Biome blend
        float biomeT = NoiseGenerator.SampleBiome(wx, wz, settings.noiseSettings);

        Color plains   = settings.plainsGradient.Evaluate(heightT);
        Color mountain = settings.mountainGradient.Evaluate(heightT);

        // Blend biome, then blend in rocky slope colour
        Color biomeColor = Color.Lerp(plains, mountain, biomeT);
        Color slopeColor = settings.mountainGradient.Evaluate(0.2f); // mid-rock
        Color finalColor = Color.Lerp(biomeColor, slopeColor, slopeT * settings.slopeInfluence);

        // Override underwater vertices
        if (height < settings.waterLevel)
        {
            float underwaterT = Mathf.InverseLerp(settings.minHeight, settings.waterLevel, height);
            finalColor = Color.Lerp(settings.deepWaterColor, settings.shallowWaterColor, underwaterT);
        }

        return finalColor;
    }

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    void OnDestroy()
    {
        ClearFoliage();
        if (meshFilter.sharedMesh != null)
            Destroy(meshFilter.sharedMesh);
    }
}
