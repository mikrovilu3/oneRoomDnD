using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralTerrain : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private int width = 100;
    [SerializeField] private int depth = 100;
    [SerializeField] private float scale = 20f;
    [SerializeField] private float heightMultiplier = 5f;

    [Header("Noise Settings")]
    [SerializeField] private float noiseScale = 0.3f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float lacunarity = 2f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private int seed = 0;

    [Header("Color Settings")]
    [SerializeField] private Gradient colorGradient;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    void Awake()
    {
        // Initialize gradient if not set
        if (colorGradient == null)
        {
            colorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[5];
            colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.3f, 0.8f), 0.0f); // Water
            colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.7f, 0.4f), 0.3f); // Sand
            colorKeys[2] = new GradientColorKey(new Color(0.2f, 0.6f, 0.2f), 0.5f); // Grass
            colorKeys[3] = new GradientColorKey(new Color(0.4f, 0.3f, 0.2f), 0.7f); // Rock
            colorKeys[4] = new GradientColorKey(new Color(1.0f, 1.0f, 1.0f), 0.9f); // Snow

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            colorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Start()
    {
        GenerateTerrain();
        SetupMaterial();
    }

    void SetupMaterial()
    {
        // Create a material that uses vertex colors
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Default-Material")
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            renderer.material = mat;
        }
    }

    public void GenerateTerrain()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        // Create vertices
        vertices = new Vector3[(width + 1) * (depth + 1)];
        float[,] noiseMap = GenerateNoiseMap();

        for (int i = 0, z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = noiseMap[x, z] * heightMultiplier;
                vertices[i] = new Vector3(x * scale / width, y, z * scale / depth);
                i++;
            }
        }

        // Create triangles
        triangles = new int[width * depth * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        // Create colors based on height
        colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = Mathf.InverseLerp(0, heightMultiplier, vertices[i].y);
            colors[i] = colorGradient.Evaluate(height);
        }
    }

    float[,] GenerateNoiseMap()
    {
        float[,] noiseMap = new float[width + 1, depth + 1];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x / (float)width) * noiseScale * frequency + octaveOffsets[i].x;
                    float sampleZ = (z / (float)depth) * noiseScale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, z] = noiseHeight;
            }
        }

        // Normalize noise map
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                noiseMap[x, z] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        // Update the mesh collider
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    void OnValidate()
    {
        if (width < 1) width = 1;
        if (depth < 1) depth = 1;
        if (octaves < 1) octaves = 1;
    }
}