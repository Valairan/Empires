using System;
using System.Threading.Tasks;
using UnityEngine;

public struct NoiseMapGenerationRequest
{
    public int MapWidth;
    public int MapHeight;
    public int Seed;
    public float Scale;
    public int Octaves;
    public float Persistance;
    public float Lacunarity;
    public float Multiplier;
    public Vector2 Offset;
    public int FalloffHeight;
    public int FalloffDistance;
    public bool Stepped;

    public NoiseMapGenerationRequest(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance, bool stepped)
    {
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Seed = seed;
        Scale = scale;
        Octaves = octaves;
        Persistance = persistance;
        Lacunarity = lacunarity;
        Multiplier = multiplier;
        Offset = offset;
        FalloffHeight = falloffHeight;
        FalloffDistance = falloffDistance;
        Stepped = stepped;
    }
}

public static class NoiseGenerator
{
    public static void InitialiseRandomed(int seed)
    {
        UnityEngine.Random.InitState(seed);
    }

    public static float[][] GenerateNoiseMapsParallel(params NoiseMapGenerationRequest[] requests)
    {
        if (requests == null || requests.Length == 0)
        {
            return new float[0][];
        }

        float[][] results = new float[requests.Length][];
        Parallel.For(0, requests.Length, i =>
        {
            results[i] = GenerateNoiseMap(requests[i]);
        });

        return results;
    }

    public static float[] GenerateNoiseMap(NoiseMapGenerationRequest request)
    {
        return GenerateNoiseMap(request.MapWidth, request.MapHeight, request.Seed, request.Scale, request.Octaves, request.Persistance, request.Lacunarity, request.Multiplier, request.Offset, request.FalloffHeight, request.FalloffDistance, request.Stepped);
    }

    public static float calculateHeight(int x, int y, int mapWidth, int mapHeight, float perlinValue, float multiplier, int falloffDistance, int falloffHeight, bool stepped)
    {
        float height = 0f;
        if (y <= falloffDistance || x <= falloffDistance || y >= mapHeight - falloffDistance || x >= mapWidth - falloffDistance)
        {
            height = falloffHeight;
        }
        else if (stepped)
        {
            if (perlinValue < 0.25f)
                height = 0f;
            else if (perlinValue < 0.5f)
                height = 1f;
            else if (perlinValue < 0.75f)
                height = 2f;
            else
                height = 4f;
        }

        height = height * multiplier;
        return height;
    }
    public static float[] GenerateSteppedNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, multiplier, offset, falloffHeight, falloffDistance, true);
    }

    public static float[] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, multiplier, offset, falloffHeight, falloffDistance, false);

    }
    public static float[] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance, bool stepped)
    {
        float[] noiseMap = new float[mapWidth * mapHeight];

        float[] octaveOffsetX = new float[octaves];
        float[] octaveOffsetY = new float[octaves];
        System.Random random = new System.Random(seed);
        for (int i = 0; i < octaves; i++)
        {
            octaveOffsetX[i] =
                random.Next(-100000, 100000)
                + offset.x;

            octaveOffsetY[i] =
                random.Next(-100000, 100000)
                + offset.y;
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
            float sampleYBase = (y - halfHeight) / scale;
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleXBase = (x - halfWidth) / scale;

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = sampleXBase * frequency + octaveOffsetX[i];
                    float sampleY = sampleYBase * frequency + octaveOffsetY[i];

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
                noiseMap[y * mapWidth + x] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int index = y * mapWidth + x;
                noiseMap[index] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[index]);
                noiseMap[index] = calculateHeight(x, y, mapWidth, mapHeight, noiseMap[index], multiplier, falloffDistance, falloffHeight, stepped);
            }
        }

        return noiseMap;
    }

}


public static class NoiseMapUtility
{
    public static int GetIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    public static float Get(float[] map, int x, int y, int width)
    {
        return map[y * width + x];
    }

    public static void Set(float[] map, int x, int y, int width, float value)
    {
        map[y * width + x] = value;
    }
}


[Serializable]
public struct VegetationType
{
    public bool isWaterPlant;
    public int seed;
    public int density;
    public float scaleRangeMin;
    public float scaleRangeMax;
    public Mesh mesh;
    public int submesh;
    [Range(0, 1)]
    public float probability;
}






[Serializable]
public struct TerrainSettings
{
    public int mapWidth;
    public int mapHeight;
    public int biomeWidth;
    public int biomeHeight;
    public int seed;
    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public float multiplier;
    public Vector2 offset;
    public int falloffHeight;
    public int falloffDistance;
}