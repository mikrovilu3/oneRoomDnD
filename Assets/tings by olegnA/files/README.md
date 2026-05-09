# Unity Smooth Terrain Generator
## Chunk-Based Infinite Terrain · Biomes · LOD · Animated Water

---

## Features

| Feature | Detail |
|---|---|
| **Chunk system** | Minecraft-style coord grid, circular view distance |
| **Smooth meshes** | No voxels — real triangle meshes with configurable resolution |
| **fBm + domain warping** | Multi-octave Perlin noise distorted by a secondary noise pass |
| **Biome blending** | Plains vs. mountain gradients blended per vertex |
| **Vertex colours** | Height + slope + biome → baked into mesh, zero texture lookups |
| **Level of Detail** | Mesh resolution halves at configurable distance thresholds |
| **Water plane** | Per-chunk animated water surface with Fresnel + wave shader |
| **Foliage spawning** | Deterministic per-chunk placement, respects height + slope |
| **Fly camera** | Fly anywhere to explore; optional terrain-following mode |

---

## Project Requirements

- Unity **2022.3 LTS** or newer
- **Universal Render Pipeline (URP)** package installed
- Works with Built-in RP too — just swap the shader include paths

---

## Setup (5 minutes)

### 1 · Import the scripts

Copy all five files into your Unity project's `Assets/Scripts/Terrain/` folder:

```
NoiseGenerator.cs
TerrainSettings.cs
TerrainChunk.cs
TerrainGenerator.cs
PlayerController.cs
```

Copy the shaders into `Assets/Shaders/`:

```
VertexColorTerrain.shader
AnimatedWater.shader
```

### 2 · Create Materials

**Terrain material**
1. `Assets → Create → Material` → name it `TerrainMaterial`
2. Shader: `Custom/VertexColorTerrain`
3. Leave defaults — the shader reads vertex colours from the mesh

**Water material**
1. `Assets → Create → Material` → name it `WaterMaterial`
2. Shader: `Custom/AnimatedWater`
3. Optionally assign a normal map texture to both `Normal Map A` and `Normal Map B`
   (any seamless water normal map works; download one free from Poly Haven)

> **Quick alternative**: Use URP Lit for both materials.
> For terrain: enable "Vertex Colors" in the material surface inputs.
> For water: set Surface Type to Transparent, blue albedo, high smoothness.

### 3 · Create TerrainSettings ScriptableObject

1. `Assets → Create → Terrain → Settings`
2. Name it `DefaultTerrainSettings`
3. Tweak values to taste (defaults produce a hilly landscape with mountains)

Key parameters to experiment with first:

| Parameter | Effect |
|---|---|
| `maxHeight` | Tallest possible mountain (metres) |
| `noiseSettings.scale` | Broader (smaller) or finer (larger) terrain features |
| `noiseSettings.octaves` | More detail layers (6–8 is sweet spot) |
| `noiseSettings.redistribution` | >1 sharpens peaks; <1 flattens everything |
| `noiseSettings.useDomainWarping` | Toggle for organic vs. regular look |
| `viewDistance` | Chunks loaded in each direction (3–5 recommended) |
| `chunkSize` | Real-world metres per chunk (128–256 recommended) |
| `chunkResolution` | Vertices per edge — must be 2^n+1 (65, 129, 257) |

### 4 · Build the scene

1. Create an empty GameObject, name it `TerrainGenerator`
2. Add the `TerrainGenerator` component
3. Assign:
   - `Settings` → your `DefaultTerrainSettings`
   - `Terrain Material` → `TerrainMaterial`
   - `Water Material` → `WaterMaterial`
   - `Player` → your player/camera transform

4. Create a `Player` GameObject (empty)
5. Add the `PlayerController` component to it
6. Add a `Camera` as a child of the Player (position it at `(0, 0, 0)` relative to the player)

### 5 · Add foliage (optional)

1. Create or import some tree/rock/bush prefabs
2. In `TerrainSettings`, assign them to the `Foliage Prefabs` array
3. Tune `foliagePerChunk`, `foliageMinHeight`, and `foliageMaxSlope`

### 6 · Hit Play

The terrain will generate around the player's starting position. Fly around with:

- **WASD** — Move
- **Right Mouse Button + drag** — Look
- **Q/E** — Move down/up
- **Scroll wheel** — Adjust speed
- **Left Shift** — Sprint

---

## Architecture Overview

```
TerrainGenerator (MonoBehaviour)
│  Tracks player chunk coord each frame
│  Maintains a load queue — generates chunksPerFrame per frame
│  Spawns / destroys TerrainChunk GameObjects
│
├── TerrainChunk (MonoBehaviour, per chunk)
│   │  Holds MeshFilter, MeshRenderer, MeshCollider
│   │  GenerateMesh(lodStep) — builds vertices, UVs, triangles, vertex colours
│   │  SpawnFoliage() — deterministic random placement
│   └── Water plane (child GameObject)
│
├── NoiseGenerator (static utility)
│   │  Sample(wx, wz, settings) → height [0..1]
│   │  SampleBiome(wx, wz, settings) → biome weight [0..1]
│   └── FBm(x, z, octaves, lacunarity, persistence) → internal
│
└── TerrainSettings (ScriptableObject)
    Holds all parameters: chunk size, resolution, noise, colours, LOD thresholds
```

---

## Customisation Tips

### Different world shapes

| Goal | Change |
|---|---|
| Flat plains + rare mountains | `redistribution = 0.7`, `scale = 0.002` |
| Dense jagged mountains | `redistribution = 2.0`, `octaves = 8` |
| Island archipelago | Add a radial falloff multiplier in `NoiseGenerator.Sample` |
| Desert dunes | `lacunarity = 1.8`, `persistence = 0.6`, low `maxHeight` |

### Performance

- Reduce `chunkResolution` to 65 for very large `chunkSize`
- Reduce `foliagePerChunk` or clear foliage on distant LOD chunks
- Use `chunksPerFrame = 1` to reduce per-frame stutter on weaker hardware
- Set `generateColliders = false` on the TerrainGenerator for fly-only exploration

### Multiplayer / server builds

- Move chunk generation into `Task.Run(...)` threads (mesh building is pure CPU math)
- Only apply the mesh on the main thread (Unity requirement)
- Use `JobSystem + Burst` for production-grade performance

---

## File Reference

| File | Purpose |
|---|---|
| `NoiseGenerator.cs` | fBm + domain-warped Perlin noise utility |
| `TerrainSettings.cs` | ScriptableObject — all configurable parameters |
| `TerrainChunk.cs` | Single chunk: mesh generation, vertex colours, foliage |
| `TerrainGenerator.cs` | World manager: chunk loading/unloading, LOD, player tracking |
| `PlayerController.cs` | Fly-cam for exploring the terrain |
| `VertexColorTerrain.shader` | URP terrain shader (vertex colour + diffuse + fog) |
| `AnimatedWater.shader` | URP water shader (Fresnel + scrolling normals + waves) |
