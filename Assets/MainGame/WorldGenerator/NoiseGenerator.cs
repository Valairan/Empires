using UnityEngine;

public static class NoiseGenerator
{

    public static float calculateHeight(int x, int y, int mapWidth, int mapHeight, float perlinValue, float multiplier, int falloffDistance, int falloffHeight, bool stepped)
    {
        float height = 0f;
        if (y <= falloffDistance || x <= falloffDistance || y >= mapHeight - falloffDistance || x >= mapWidth - falloffDistance)
        {
            height = falloffHeight;
        }
        else if (stepped)
        {
            if (perlinValue < 1f) height = 4f;
            if (perlinValue < 0.75f) height = 2f;
            if (perlinValue < 0.5f) height = 1f;
            if (perlinValue < 0.25f) height = 0f;
        }


        height = height * multiplier;
        return height;
    }
    public static float[,] GenerateSteppedNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, multiplier, offset, falloffHeight, falloffDistance, true);
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, multiplier, offset, falloffHeight, falloffDistance, false);

    }
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float multiplier, Vector2 offset, int falloffHeight, int falloffDistance, bool stepped)
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
                noiseMap[x, y] = calculateHeight(x, y, mapWidth, mapHeight, noiseMap[x, y], multiplier, falloffDistance, falloffHeight, stepped);
            }
        }

        return noiseMap;
    }

}
