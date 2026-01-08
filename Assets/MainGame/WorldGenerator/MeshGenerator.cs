using UnityEngine;
using System.Collections;
public static class MeshGenerator
{
    static float IslandFalloff(int x, int y, int width, int height)
    {
        float nx = (x / (float)width) * 2f - 1f;
        float ny = (y / (float)height) * 2f - 1f;

        float distance = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));
        float falloff = Mathf.Pow(distance, 3f);

        return Mathf.Clamp01(1f - falloff);
    }

    public static GameObject GenerateTerrainGrid(int mapWidth, int mapHeight, float[,] noise, float heightMultiplier, AnimationCurve heightCurve)
    {
        GameObject terrain = new GameObject("Procedural Island");

        MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh { name = "Terrain Mesh" };

        Vector3[] vertices = new Vector3[mapWidth * mapHeight];
        Vector2[] uvs = new Vector2[mapWidth * mapHeight];
        int[] triangles = new int[(mapWidth - 1) * (mapHeight - 1) * 6];

        int v = 0;
        int t = 0;

        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float falloff = IslandFalloff(x, z, mapWidth, mapHeight);
                float height = heightCurve.Evaluate(noise[x, z] * falloff) * heightMultiplier;

                vertices[v] = new Vector3(x, height, z);
                uvs[v] = new Vector2((float)x / mapWidth, (float)z / mapHeight);

                if (x < mapWidth - 1 && z < mapHeight - 1)
                {
                    triangles[t + 0] = v;
                    triangles[t + 1] = v + mapWidth;
                    triangles[t + 2] = v + mapWidth + 1;

                    triangles[t + 3] = v;
                    triangles[t + 4] = v + mapWidth + 1;
                    triangles[t + 5] = v + 1;

                    t += 6;
                }

                v++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        return terrain;
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}

