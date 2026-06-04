using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Palmmedia.ReportGenerator.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public struct VegetationChunk
{
    public Vector2Int coord;
    public Bounds bounds;

    public Matrix4x4[] matrices;
    public int[] meshes;
    public int count; // number of valid instances
}
public static class VegetationPlanter
{
    public static VegetationChunk[,] scatterGrassInChunks(TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, float probability, float[,] availableSpots, float[,] biomeNoise)
    {
        int chunkCountX = Mathf.CeilToInt((float)settings.mapWidth / grassChunkSize);

        int chunkCountY = Mathf.CeilToInt((float)settings.mapHeight / grassChunkSize);
        VegetationChunk[,] chunkGrid = new VegetationChunk[chunkCountX, chunkCountY];

        for (int cy = 0; cy < chunkCountY; cy++)
        {
            for (int cx = 0; cx < chunkCountX; cx++)
            {
                chunkGrid[cx, cy] = GenerateGrassChunk(
                    cx,
                    cy,
                    settings,
                    grassChunkSize,
                    grassPerCell,
                    isWaterPlant,
                    seed,
                    scaleRangeMin,
                    scaleRangeMax,
                    probability,
                    availableSpots,
                    biomeNoise
                );
            }
        }

        return chunkGrid;
    }
    public static VegetationChunk[,] scatterGrassInChunks(TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, float probability, float[,] availableSpots)
    {
        int chunkCountX = Mathf.CeilToInt((float)settings.mapWidth / grassChunkSize);

        int chunkCountY = Mathf.CeilToInt((float)settings.mapHeight / grassChunkSize);
        VegetationChunk[,] chunkGrid = new VegetationChunk[chunkCountX, chunkCountY];
        float[,] biomeMap = new float[settings.biomeWidth, settings.biomeHeight];

        for (int x = 0; x < settings.biomeWidth; x++)
        {
            for (int y = 0; y < settings.biomeHeight; y++)
            {
                biomeMap[x, y] = 1;
            }
        }
        for (int cy = 0; cy < chunkCountY; cy++)
        {
            for (int cx = 0; cx < chunkCountX; cx++)
            {
                chunkGrid[cx, cy] = GenerateGrassChunk(
                    cx,
                    cy,
                    settings,
                    grassChunkSize,
                    grassPerCell,
                    isWaterPlant,
                    seed,
                    scaleRangeMin,
                    scaleRangeMax,
                    probability,
                    availableSpots,
                    biomeMap
                );
            }
        }

        return chunkGrid;
    }
    private static bool isEdge(int x, int y, float height, float[,] availableSpots)
    {

        if (availableSpots[x, y - 1] != height ||
            availableSpots[x, y + 1] != height ||
            availableSpots[x - 1, y] != height ||
            availableSpots[x + 1, y] != height ||
            availableSpots[x + 1, y + 1] != height ||
            availableSpots[x + 1, y - 1] != height ||
            availableSpots[x - 1, y + 1] != height ||
            availableSpots[x - 1, y - 1] != height)
        {
            return true;
        }
        return false;
    }
    private static bool isWaterEdge(int x, int y, float height, float[,] availableSpots)
    {

        if (availableSpots[x, y - 1] < height ||
            availableSpots[x, y + 1] < height ||
            availableSpots[x - 1, y] < height ||
            availableSpots[x + 1, y] < height ||
            availableSpots[x + 1, y + 1] < height ||
            availableSpots[x + 1, y - 1] < height ||
            availableSpots[x - 1, y + 1] < height ||
            availableSpots[x - 1, y - 1] < height)
        {
            return true;
        }
        return false;
    }
    private static Vector2 CalculateWaterEdgeOffset(
        int x,
        int y,
        float[,] heightMap,
        DeterministicRng rng,
        float pushStrength = 0.35f,
        float sideStrength = 0.25f,
        bool scaleBySlope = true)
    {
        Vector2 pushDir = Vector2.zero;
        float current = heightMap[x, y];

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // Accumulate direction AWAY from lower neighbours
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                float neighbour = heightMap[nx, ny];

                if (neighbour < current)
                {
                    float weight = scaleBySlope ? (current - neighbour) : 1f;

                    // push AWAY from lower neighbour
                    pushDir -= new Vector2(dx, dy) * weight;
                }
            }
        }

        if (pushDir == Vector2.zero)
            return Vector2.zero;

        pushDir.Normalize();

        // Base inland push
        Vector2 offset = pushDir * pushStrength;

        // Perpendicular shoreline sliding
        Vector2 alongEdge = new Vector2(-pushDir.y, pushDir.x);

        float sideSlide = (rng.NextFloat() * 2f - 1f) * sideStrength;
        offset += alongEdge * sideSlide;

        return offset;
    }

    public static VegetationChunk GenerateGrassChunk(int chunkX, int chunkY, TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, float probability, float[,] availableSpots, float[,] biomeMap)
    {
        int chunkSize = grassChunkSize;

        int maxInstances = chunkSize * chunkSize * grassPerCell;

        VegetationChunk chunk = new VegetationChunk
        {
            coord = new Vector2Int(chunkX, chunkY),
            matrices = new Matrix4x4[maxInstances],
            meshes = new int[maxInstances],
            count = 0
        };

        int startX = chunkX * chunkSize;
        int startY = chunkY * chunkSize;

        int endX = Mathf.Min(startX + chunkSize - 1, settings.mapWidth - 1);
        int endY = Mathf.Min(startY + chunkSize - 1, settings.mapHeight - 1);

        float minH = float.MaxValue;
        float maxH = float.MinValue;

        DeterministicRng rng = new DeterministicRng(seed);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {

                float u = (x + 0.5f) / settings.mapWidth;
                float v = (y + 0.5f) / settings.mapHeight;

                int biomeX = Mathf.Clamp(
                    (int)(u * settings.biomeWidth),
                    0, settings.biomeWidth - 1
                );

                int biomeY = Mathf.Clamp(
                    (int)(v * settings.biomeHeight),
                    0, settings.biomeHeight - 1
                );

                //float biome = biomeMap[biomeX, biomeY];

                if (x % 100 == 0)
                {
                    Debug.Log(biomeX);
                    Debug.Log(biomeY);
                }

                float finalProbability = probability;
                if (rng.Hash(x, y) >= finalProbability) continue;

                float height = availableSpots[x, y];
                if (height < 4) continue;
                if (x > 0 && x < settings.mapWidth - 1 && y > 0 && y < settings.mapHeight - 1)
                {
                    if (height < 6)
                    {
                        if (isWaterPlant)
                        {
                            if (!isWaterEdge(x, y, height, availableSpots)) continue;
                        }
                    }
                    else
                    {
                        if (isWaterPlant) continue;
                    }

                    if (!isWaterPlant)
                        if (isEdge(x, y, height, availableSpots)) continue;


                    for (int i = 0; i < grassPerCell; i++)
                    {
                        Vector2 offset2D = isWaterPlant
                            ? CalculateWaterEdgeOffset(x, y, availableSpots, rng)
                            : new Vector2(
                                rng.NextFloat(-0.5f, 0.5f),
                                rng.NextFloat(-0.5f, 0.5f)
                              );

                        float offsetX = offset2D.x;
                        float offsetZ = offset2D.y;

                        float scale = isWaterPlant ? 1f : Random.Range(scaleRangeMin, scaleRangeMax);
                        float rotY = Random.Range(0f, 360f);

                        Vector3 pos = new Vector3(
                            x + offsetX,
                            height,
                            y + offsetZ
                        );

                        chunk.matrices[chunk.count++] =
                            Matrix4x4.TRS(
                                pos,
                                Quaternion.Euler(0f, rotY, 0f),
                                Vector3.one * scale
                            );

                        minH = Mathf.Min(minH, height);
                        maxH = Mathf.Max(maxH, height);

                        //chunk.meshes[chunk.count] = 0;//(int)Random.Range(0, numberOfMeshes - 1);

                    }

                }
            }
        }

        Vector3 center = new Vector3(
            startX + chunkSize * 0.5f,
            (minH + maxH) * 0.5f,
            startY + chunkSize * 0.5f
        );

        Vector3 size = new Vector3(
            chunkSize,
            Mathf.Max(1f, maxH - minH),
            chunkSize
        );

        chunk.bounds = new Bounds(center, size);

        return chunk;
    }
    public static BaseResourceBehaviour[,] ScatterDecoration(int mapHeight, int mapWidth, int seed, GameObject[] vegetation, float[,] availableSpots, int skip, float[,] biome)
    {
        GameObject treeParent = new GameObject("Tree Parent");
        BaseResourceBehaviour[,] placedTrees = new BaseResourceBehaviour[mapHeight, mapWidth];
        int treeCount = 0;
        DeterministicRng rng = new DeterministicRng(seed);
        DeterministicRng rng1 = new DeterministicRng(seed + 1);
        for (int x = 0; x < mapHeight; x++)
        {
            for (int y = 0; y < mapWidth; y++)
            {
                float height = availableSpots[x, y];
                if (height < 4) continue;
                if (x > 0 && x < mapHeight - 1 && y > 0 && y < mapWidth - 1)

                    if (availableSpots[x + 1, y + 1] == height && availableSpots[x + 1, y - 1] == height && availableSpots[x - 1, y + 1] == height && availableSpots[x - 1, y - 1] == height)
                    {
                        bool spawn = rng.NextFloat() > 0.98f;

                        if (!((availableSpots[x, y] <= 1) && (availableSpots[x, y] < 40)))
                        {
                            if (spawn)
                            {
                                //int whatToSpawn = (int)Mathf.Clamp(biome[y / ((mapWidth - 1) / (biomeDimensionsY - 1)), x / ((mapHeight - 1) / (biomeDimensionsX - 1))] * 5, 0, availableItems - 1);
                                int whatToSpawn = rng.NextInt(0, vegetation.Length);
                                GameObject resource = GameObject.Instantiate(vegetation[whatToSpawn], new Vector3(x + rng.NextFloat(), availableSpots[x, y], y + rng1.NextFloat()), Quaternion.Euler(new Vector3(0f, rng.NextInt(0, 360), 0f)), treeParent.transform);
                                resource.isStatic = true;
                                placedTrees[x, y] = resource.GetComponent<BaseResourceBehaviour>();
                                placedTrees[x, y].xCoordinate = x;
                                placedTrees[x, y].yCoordinate = y;
                                treeCount++;
                            }
                        }


                        //Loader.instance.setProgress(x / mapHeight);
                    }
            }

        }

        return placedTrees;
        //StaticBatchingUtility.Combine(grassParentSub);
        //StaticBatchingUtility.Combine(treeParentSub);
    }
    public static (int x2, int y2) RemapIndex(
        int x1, int y1,
        int width1, int height1,
        int width2, int height2)
    {
        // Normalize coordinates (0 → 1 range)
        float u = (float)x1 / (width1 - 1);
        float v = (float)y1 / (height1 - 1);

        // Scale to new resolution
        int x2 = Mathf.RoundToInt(u * (width2 - 1));
        int y2 = Mathf.RoundToInt(v * (height2 - 1));

        return (x2, y2);
    }

}


