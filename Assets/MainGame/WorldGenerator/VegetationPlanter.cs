using System.Threading.Tasks;
using UnityEngine;
public struct VegetationChunk
{
    public Vector2Int coord;
    public Bounds bounds;
    public Matrix4x4[] matrices;
    public ComputeBuffer matrixBuffer;

    public int[] meshes;
    public int count; // number of valid instances
}
public static class VegetationPlanter
{
    public static VegetationChunk[,] scatterGrassInChunks(TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, float probability, float[] availableSpots, float[] biomeNoise)
    {
        int chunkCountX = Mathf.CeilToInt((float)settings.mapWidth / grassChunkSize);

        int chunkCountY = Mathf.CeilToInt((float)settings.mapHeight / grassChunkSize);
        VegetationChunk[,] chunkGrid = new VegetationChunk[chunkCountX, chunkCountY];

        Parallel.For(0, chunkCountY, cy =>
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
        });

        return chunkGrid;
    }
    public static VegetationChunk[,] scatterGrassInChunks(TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, float probability, float[] availableSpots)
    {
        int chunkCountX = Mathf.CeilToInt((float)settings.mapWidth / grassChunkSize);

        int chunkCountY = Mathf.CeilToInt((float)settings.mapHeight / grassChunkSize);
        VegetationChunk[,] chunkGrid = new VegetationChunk[chunkCountX, chunkCountY];
        float[] biomeNoise = new float[settings.biomeWidth * settings.biomeHeight];

        for (int x = 0; x < settings.biomeWidth; x++)
        {
            for (int y = 0; y < settings.biomeHeight; y++)
            {
                biomeNoise[NoiseMapUtility.GetIndex(x, y, settings.biomeWidth)] = 1;
            }
        }
        Parallel.For(0, chunkCountY, cy =>
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
        });

        return chunkGrid;
    }
    private static bool IsEdge(int x, int y, float height, int mapWidth, float[] availableSpots)
    {
        if (availableSpots[NoiseMapUtility.GetIndex(x, y - 1, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x, y + 1, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x - 1, y, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x + 1, y, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x + 1, y + 1, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x + 1, y - 1, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x - 1, y + 1, mapWidth)] != height ||
            availableSpots[NoiseMapUtility.GetIndex(x - 1, y - 1, mapWidth)] != height)
        {
            return true;
        }

        return false;
    }
    private static bool isWaterEdge(int x, int y, float height, int mapWidth, float[] availableSpots)
    {

        if (availableSpots[NoiseMapUtility.GetIndex(x, y - 1, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x, y + 1, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x - 1, y, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x + 1, y, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x + 1, y + 1, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x + 1, y - 1, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x - 1, y + 1, mapWidth)] < height ||
            availableSpots[NoiseMapUtility.GetIndex(x - 1, y - 1, mapWidth)] < height)
        {
            return true;
        }
        return false;
    }
    private static Vector2 CalculateWaterEdgeOffset(
        int x,
        int y,
        int mapWidth,
        int mapHeight,
        float[] heightMap,
        DeterministicRng rng,
        float pushStrength = 0.35f,
        float sideStrength = 0.25f,
        bool scaleBySlope = true)
    {
        Vector2 pushDir = Vector2.zero;
        float current = heightMap[NoiseMapUtility.GetIndex(x, y, mapWidth)];

        int width = mapWidth;
        int height = mapHeight;

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

                float neighbour = heightMap[NoiseMapUtility.GetIndex(nx, ny, width)];

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

    public static VegetationChunk GenerateGrassChunk(int chunkX, int chunkY, TerrainSettings settings, int grassChunkSize, int grassPerCell, bool isWaterPlant, int seed, float scaleRangeMin, float scaleRangeMax, float probability, float[] availableSpots, float[] biomeNoise)
    {
        int chunkSize = grassChunkSize;
        int mapWidth = settings.mapWidth;
        int mapHeight = settings.mapHeight;
        int mapWidthMinusOne = mapWidth - 1;
        int mapHeightMinusOne = mapHeight - 1;
        float[] spotMap = availableSpots;
        bool waterPlant = isWaterPlant;
        int grassCount = grassPerCell;

        int maxInstances = chunkSize * chunkSize * grassCount;
        Matrix4x4[] matrices = new Matrix4x4[maxInstances];
        int[] meshes = new int[maxInstances];

        VegetationChunk chunk = new VegetationChunk
        {
            coord = new Vector2Int(chunkX, chunkY),
            count = 0
        };

        int startX = chunkX * chunkSize;
        int startY = chunkY * chunkSize;

        int endX = Mathf.Min(startX + chunkSize - 1, mapWidthMinusOne);
        int endY = Mathf.Min(startY + chunkSize - 1, mapHeightMinusOne);

        float minH = float.MaxValue;
        float maxH = float.MinValue;

        DeterministicRng rng = new DeterministicRng(seed);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (rng.Hash(x, y) >= probability) continue;

                int spotIndex = y * mapWidth + x;
                float height = spotMap[spotIndex];
                if (height < 4f) continue;
                if (x <= 0 || x >= mapWidthMinusOne || y <= 0 || y >= mapHeightMinusOne) continue;

                if (height < 6f)
                {
                    if (waterPlant)
                    {
                        if (!isWaterEdge(x, y, height, mapWidth, spotMap)) continue;
                    }
                }
                else
                {
                    if (waterPlant) continue;
                }

                if (!waterPlant)
                {
                    if (IsEdge(x, y, height, mapWidth, spotMap)) continue;
                }

                int instanceCount = chunk.count;
                for (int i = 0; i < grassCount; i++)
                {
                    Vector2 offset2D = waterPlant
                        ? CalculateWaterEdgeOffset(x, y, mapWidth, mapHeight, spotMap, rng)
                        : new Vector2(
                            rng.NextFloat(-0.5f, 0.5f),
                            rng.NextFloat(-0.5f, 0.5f)
                          );

                    float offsetX = offset2D.x;
                    float offsetZ = offset2D.y;

                    float scale = waterPlant ? 1f : rng.NextFloat(scaleRangeMin, scaleRangeMax);
                    float rotY = rng.NextFloat(0f, 360f);

                    Vector3 pos = new Vector3(
                        x + offsetX,
                        height,
                        y + offsetZ
                    );

                    matrices[instanceCount++] =
                        Matrix4x4.TRS(
                            pos,
                            Quaternion.Euler(0f, rotY, 0f),
                            Vector3.one * scale
                        );

                    minH = Mathf.Min(minH, height);
                    maxH = Mathf.Max(maxH, height);
                }

                chunk.count = instanceCount;
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
        chunk.matrices = matrices;
        chunk.meshes = meshes;

        return chunk;
    }
    public static IScatteredDecoration[,] ScatterDecoration(int mapHeight, int mapWidth, int seed, GameObject[] vegetation, GameObject[] extras, ref float[] availableSpots, int skip, float[] biomeNoise)
    {
        GameObject treeParent = new GameObject("Tree Parent");
        IScatteredDecoration[,] placedDecoration = new IScatteredDecoration[mapHeight, mapWidth];
        int treeCount = 0;
        DeterministicRng rng = new DeterministicRng(seed);
        DeterministicRng rng1 = new DeterministicRng(seed + 1);
        for (int x = 0; x < mapHeight; x++)
        {
            for (int y = 0; y < mapWidth; y++)
            {
                float height = availableSpots[NoiseMapUtility.GetIndex(x, y, mapWidth)];
                if (height < 4) continue;
                if (x > 0 && x < mapHeight - 1 && y > 0 && y < mapWidth - 1)

                    if (availableSpots[NoiseMapUtility.GetIndex(x + 1, y + 1, mapWidth)] == height && availableSpots[NoiseMapUtility.GetIndex(x + 1, y - 1, mapWidth)] == height && availableSpots[NoiseMapUtility.GetIndex(x - 1, y + 1, mapWidth)] == height && availableSpots[NoiseMapUtility.GetIndex(x - 1, y - 1, mapWidth)] == height)
                    {
                        bool spawn = rng.NextFloat() > 0.98f;

                        if (!((availableSpots[NoiseMapUtility.GetIndex(x, y, mapWidth)] <= 1) && (availableSpots[NoiseMapUtility.GetIndex(x, y, mapWidth)] < 40)))
                        {
                            if (spawn)
                            {
                                //int whatToSpawn = (int)Mathf.Clamp(biome[y / ((mapWidth - 1) / (biomeDimensionsY - 1)), x / ((mapHeight - 1) / (biomeDimensionsX - 1))] * 5, 0, availableItems - 1);
                                int whatToSpawn = rng.NextInt(0, vegetation.Length);
                                GameObject resource = GameObject.Instantiate(vegetation[whatToSpawn], new Vector3(x + rng.NextFloat(), availableSpots[NoiseMapUtility.GetIndex(x, y, mapWidth)], y + rng1.NextFloat()), Quaternion.Euler(new Vector3(0f, rng.NextInt(0, 360), 0f)), treeParent.transform);
                                placedDecoration[x, y] = resource.GetComponent<IScatteredDecoration>();
                                placedDecoration[x, y].InitializeDecoration(x, y);
                                resource.isStatic = true;
                                treeCount++;
                            }
                        }
                        //Loader.instance.setProgress(x / mapHeight);
                    }
            }

        }

        return placedDecoration;
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

public interface IScatteredDecoration
{
    GameObject decorationObject { get; }
    void InitializeDecoration(int x, int y);
}

public static class WorldDataStore
{
    public static IWorldDataStore Lookup { get; set; }
}

public interface IWorldDataStore
{
    IScatteredDecoration GetTreeAt(int x, int y);
    void RemoveTreeAt(int x, int y);
}