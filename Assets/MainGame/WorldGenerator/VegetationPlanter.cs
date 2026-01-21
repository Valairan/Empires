using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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


    public static VegetationChunk[,] scatterGrassInChunks(TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, int probability, float[,] availableSpots)
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
                    availableSpots
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

    public static VegetationChunk GenerateGrassChunk(int chunkX, int chunkY, TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, int probability, float[,] availableSpots)
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

                if (rng.NextInt(0, 11) >= probability) continue;
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


                    // deterministic per-cell RNG

                    for (int i = 0; i < grassPerCell; i++)
                    {
                        float offsetX = isWaterPlant ? 0f : Random.Range(-0.5f, 0.5f);
                        float offsetZ = isWaterPlant ? 0f : Random.Range(-0.5f, 0.5f);

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

                        //int whatToSpawn = (int)Mathf.Clamp(biome[y / ((mapWidth - 1) / (biomeDimensionsY - 1)), x / ((mapHeight - 1) / (biomeDimensionsX - 1))] * 5, 0, availableItems - 1);

                        if (!((availableSpots[x, y] <= 1) && (availableSpots[x, y] > 30)))
                        {
                            if (spawn)
                            {
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


}


