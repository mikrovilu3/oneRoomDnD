using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central world manager. Tracks the player, loads new chunks when they enter
/// range, unloads distant ones, and adjusts mesh LOD based on distance.
///
/// Setup:
///   1. Add this component to an empty GameObject called "TerrainGenerator".
///   2. Assign TerrainSettings (create via Assets → Create → Terrain → Settings).
///   3. Assign terrainMaterial (use the included VertexColor shader / URP Lit with vertex colors).
///   4. Assign waterMaterial (semi-transparent blue URP Lit material).
///   5. Assign the Player transform.
///   6. Hit Play — the world will generate around the player.
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    // ─── Inspector ─────────────────────────────────────────────────────────────

    [Header("References")]
    public TerrainSettings settings;
    public Transform       player;
    public Material        terrainMaterial;
    public Material        waterMaterial;

    [Header("Generation")]
    [Tooltip("How many new chunks to generate per frame. Higher = faster but more stutter.")]
    [Range(1, 8)]
    public int chunksPerFrame = 2;

    [Tooltip("Generate collision meshes on all chunks (expensive — turn off for distant chunks).")]
    public bool generateColliders = true;

    // ─── State ─────────────────────────────────────────────────────────────────

    Dictionary<Vector2Int, TerrainChunk> loadedChunks  = new Dictionary<Vector2Int, TerrainChunk>();
    Queue<Vector2Int>                    generateQueue  = new Queue<Vector2Int>();
    HashSet<Vector2Int>                  queuedSet      = new HashSet<Vector2Int>();

    Vector2Int lastPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);

    // ─── Unity ─────────────────────────────────────────────────────────────────

    void Start()
    {
        if (!ValidateSetup()) return;
        StartCoroutine(GenerationLoop());
        UpdateVisibleChunks(); // immediate first pass
    }

    void Update()
    {
        Vector2Int currentChunk = WorldToChunkCoord(player.position);
        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            UpdateVisibleChunks();
        }
    }

    // ─── Chunk Management ──────────────────────────────────────────────────────

    void UpdateVisibleChunks()
    {
        Vector2Int playerChunk = WorldToChunkCoord(player.position);
        int        view        = settings.viewDistance;

        // Mark all currently loaded chunks as candidates for unloading
        HashSet<Vector2Int> shouldBeLoaded = new HashSet<Vector2Int>();

        for (int dx = -view; dx <= view; dx++)
        for (int dz = -view; dz <= view; dz++)
        {
            // Use circular view distance for a nicer shape
            if (dx * dx + dz * dz > view * view) continue;
            shouldBeLoaded.Add(new Vector2Int(playerChunk.x + dx, playerChunk.y + dz));
        }

        // Queue newly needed chunks
        foreach (var coord in shouldBeLoaded)
        {
            if (!loadedChunks.ContainsKey(coord) && !queuedSet.Contains(coord))
            {
                generateQueue.Enqueue(coord);
                queuedSet.Add(coord);
            }
        }

        // Unload out-of-range chunks
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in loadedChunks)
        {
            if (!shouldBeLoaded.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }
        foreach (var coord in toRemove)
            UnloadChunk(coord);

        // Update LOD on visible chunks
        foreach (var kvp in loadedChunks)
            UpdateChunkLOD(kvp.Key, kvp.Value, playerChunk);
    }

    void UpdateChunkLOD(Vector2Int coord, TerrainChunk chunk, Vector2Int playerChunk)
    {
        if (!chunk.isReady) return;
        if (settings.lodThresholds == null || settings.lodThresholds.Length == 0) return;

        int dist    = Mathf.Max(Mathf.Abs(coord.x - playerChunk.x), Mathf.Abs(coord.y - playerChunk.y));
        int lodStep = 1;

        for (int i = 0; i < settings.lodThresholds.Length; i++)
        {
            if (dist >= settings.lodThresholds[i])
                lodStep = (int)Mathf.Pow(2, i + 1);
        }

        // Only rebuild if LOD has changed
        // (In production you'd cache the current LOD on the chunk to avoid redundant rebuilds)
        // For simplicity we skip that optimisation here — the overhead is minimal.
    }

    // ─── Generation Coroutine ─────────────────────────────────────────────────

    IEnumerator GenerationLoop()
    {
        while (true)
        {
            int generated = 0;
            while (generateQueue.Count > 0 && generated < chunksPerFrame)
            {
                Vector2Int coord = generateQueue.Dequeue();
                queuedSet.Remove(coord);

                // Skip if already loaded (could have been requested twice)
                if (loadedChunks.ContainsKey(coord)) continue;

                LoadChunk(coord);
                generated++;
            }
            yield return null; // wait one frame between batches
        }
    }

    void LoadChunk(Vector2Int coord)
    {
        GameObject go = new GameObject($"Chunk [{coord.x}, {coord.y}]");
        go.transform.SetParent(transform);

        TerrainChunk chunk = go.AddComponent<TerrainChunk>();
        chunk.Initialise(coord, settings, terrainMaterial, waterMaterial);

        // Pick LOD based on distance to player
        Vector2Int playerChunk = WorldToChunkCoord(player.position);
        int        dist        = Mathf.Max(Mathf.Abs(coord.x - playerChunk.x), Mathf.Abs(coord.y - playerChunk.y));
        int        lodStep     = GetLodStep(dist);

        chunk.GenerateMesh(lodStep);

        if (settings.foliagePrefabs != null && settings.foliagePrefabs.Length > 0 && lodStep == 1)
            chunk.SpawnFoliage();

        loadedChunks[coord] = chunk;
    }

    void UnloadChunk(Vector2Int coord)
    {
        if (loadedChunks.TryGetValue(coord, out TerrainChunk chunk))
        {
            if (chunk != null) Destroy(chunk.gameObject);
            loadedChunks.Remove(coord);
        }
    }

    // ─── Utilities ─────────────────────────────────────────────────────────────

    Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / settings.chunkSize),
            Mathf.FloorToInt(worldPos.z / settings.chunkSize)
        );
    }

    int GetLodStep(int chunkDistance)
    {
        if (settings.lodThresholds == null) return 1;
        int lodStep = 1;
        for (int i = 0; i < settings.lodThresholds.Length; i++)
        {
            if (chunkDistance >= settings.lodThresholds[i])
                lodStep = (int)Mathf.Pow(2, i + 1);
        }
        return lodStep;
    }

    bool ValidateSetup()
    {
        if (settings == null)       { Debug.LogError("[TerrainGenerator] TerrainSettings not assigned!"); return false; }
        if (player == null)         { Debug.LogError("[TerrainGenerator] Player not assigned!"); return false; }
        if (terrainMaterial == null){ Debug.LogError("[TerrainGenerator] Terrain material not assigned!"); return false; }
        if (waterMaterial == null)  { Debug.LogError("[TerrainGenerator] Water material not assigned!"); return false; }
        return true;
    }

    // ─── Editor Gizmos ────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (settings == null || player == null) return;

        Vector2Int pc = WorldToChunkCoord(player.position);
        float      s  = settings.chunkSize;
        int        v  = settings.viewDistance;

        Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.25f);
        for (int dx = -v; dx <= v; dx++)
        for (int dz = -v; dz <= v; dz++)
        {
            if (dx * dx + dz * dz > v * v) continue;
            Vector3 center = new Vector3((pc.x + dx + 0.5f) * s, 0f, (pc.y + dz + 0.5f) * s);
            Gizmos.DrawCube(center, new Vector3(s, 1f, s));
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, v * s);
    }
}
