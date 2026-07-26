using System;
using UnityEngine;

public class WorldGenerator : MonoBehaviour, IWorldDataStore
{
    float[] terrainNoise;
    float[] biomeNoise;
    float[] weatherNoise;
    [SerializeField] Material terrainMaterial;
    [SerializeField] public GameObject[] TreesToPlace;
    [SerializeField] public GameObject[] DecorToPlace;
    [SerializeField] public IScatteredDecoration[,] placedTrees;
    public bool[,] spotsLeft;
    Camera mainCamera;
    bool generationComplete;
    public VegetationType[] vegetation;
    public VegetationChunk[][,] totaleVegetationChunks;
    RenderParams renderParams;
    [SerializeField] Material grassMaterial;
    [SerializeField] public int numberOfChunksToRender;

    int mapWidth = 0;
    int mapHeight = 0;

    public void StartGameLocal()
    {
        TerrainSettings settings = new TerrainSettings
        {
            mapWidth = 250,
            mapHeight = 250,
            biomeWidth = 50,
            biomeHeight = 50,
            seed = 10,
            scale = 32,
            octaves = 4,
            persistance = 4,
            lacunarity = 0,
            multiplier = 5,
            offset = Vector2.zero,
            falloffHeight = 5,
            falloffDistance = 5
        };

        //GenerateTerrain(settings);
    }

    void Start()
    {

        //StartGameLocal();
    }

    public void GenerateTerrain(TerrainSettings settings, ref float progress)
    {
        progress = 0f;
        mapHeight = settings.mapHeight;
        mapWidth = settings.mapWidth;
        totaleVegetationChunks = new VegetationChunk[vegetation.Length][,];
        progress = .2f;
        float[][] noiseMaps = NoiseGenerator.GenerateNoiseMapsParallel(
            new NoiseMapGenerationRequest(settings.mapWidth, settings.mapHeight, settings.seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.multiplier, settings.offset, settings.falloffHeight, settings.falloffDistance, true),
            new NoiseMapGenerationRequest(settings.biomeWidth, settings.biomeHeight, 20, 32, 4, 16f, 16f, 1f, Vector2.zero, 0, 0, false),
            new NoiseMapGenerationRequest(100, 100, 10, 4, 4, 16f, 16f, 1f, Vector2.zero, 0, 0, false));

        terrainNoise = noiseMaps[0];
        biomeNoise = noiseMaps[1];
        weatherNoise = noiseMaps[2];
        progress = .5f;

        RiverGenerator.CarveRiver(settings.mapWidth, settings.mapHeight, settings.seed, 6, 10, terrainNoise);

        MeshGenerator.GenerateTerrainMesh(settings.mapWidth, settings.mapHeight, 100, terrainMaterial, 6, terrainNoise);
        progress = .6f;
        //MeshGenerator.GenerateSquareMesh(settings.mapWidth, settings.mapHeight, 25, oceanMaterial, 4);

        placedTrees = VegetationPlanter.ScatterDecoration(settings.mapWidth, settings.mapHeight, 100, TreesToPlace, DecorToPlace, ref terrainNoise, 5, biomeNoise);
        progress = .7f;


        totaleVegetationChunks[0] = VegetationPlanter.scatterGrassInChunks(settings, 5, vegetation[0].density, vegetation[0].isWaterPlant, vegetation[0].seed, vegetation[0].scaleRangeMin, vegetation[0].scaleRangeMax, vegetation[0].probability, terrainNoise);
        for (int i = 1; i < vegetation.Length; i++)
        {
            totaleVegetationChunks[i] = VegetationPlanter.scatterGrassInChunks(settings, 5, vegetation[i].density, vegetation[i].isWaterPlant, vegetation[i].seed, vegetation[i].scaleRangeMin, vegetation[i].scaleRangeMax, vegetation[i].probability, terrainNoise, biomeNoise);
        }
        progress = .8f;

        renderParams = new RenderParams(grassMaterial)
        {
            receiveShadows = true,
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off
        };


        progress = 1f;
        generationComplete = true;
        //ResourcesManager.Singleton.placedTrees = placedTrees;
        mainCamera = Camera.main;

    }

    Matrix4x4[] visibleMatrices = new Matrix4x4[1023];
    int maxInstances = 1023;
    private Plane[] planes = new Plane[6];
    private int lastChunkX = int.MinValue;
    private int lastChunkZ = int.MinValue;
    private int lastChunkRange = -1;
    private Matrix4x4[] renderBatch = new Matrix4x4[1023];

    void Update()
    {
        if (!generationComplete) return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        Vector3 camPos = mainCamera.transform.position;

        int positionX = (int)Math.Clamp(camPos.x, 0, mapWidth) / 5;
        int positionZ = (int)Math.Clamp(camPos.z, 0, mapHeight) / 5;
        int chunkRange = numberOfChunksToRender;

        if (positionX != lastChunkX || positionZ != lastChunkZ || chunkRange != lastChunkRange)
        {
            lastChunkX = positionX;
            lastChunkZ = positionZ;
            lastChunkRange = chunkRange;
        }

        for (int k = 0; k < vegetation.Length; k++)
        {
            int count = 0;
            int chunkCountX = mapWidth / 5;
            int chunkCountZ = mapHeight / 5;

            for (int i = -chunkRange; i <= chunkRange; i++)
            {
                int chunkX = positionX + i;
                if (chunkX < 0 || chunkX >= chunkCountX)
                    continue;

                for (int j = -chunkRange; j <= chunkRange; j++)
                {
                    int chunkZ = positionZ + j;
                    if (chunkZ < 0 || chunkZ >= chunkCountZ)
                        continue;

                    VegetationChunk chunk = totaleVegetationChunks[k][chunkX, chunkZ];
                    if (chunk.count == 0)
                        continue;

                    if (!GeometryUtility.TestPlanesAABB(planes, chunk.bounds))
                    {
                        continue;
                    }

                    for (int m = 0; m < chunk.count; m++)
                    {
                        renderBatch[count] = chunk.matrices[m];
                        count++;

                        if (count == maxInstances)
                        {
                            Graphics.RenderMeshInstanced(
                                renderParams,
                                vegetation[k].mesh,
                                0,
                                renderBatch,
                                count
                            );
                            count = 0;
                        }
                    }
                }
            }

            if (count > 0)
            {
                Graphics.RenderMeshInstanced(
                    renderParams,
                    vegetation[k].mesh,
                    0,
                    renderBatch,
                    count
                );
            }
        }
    }

    public IScatteredDecoration GetTreeAt(int x, int y)
    {
        return placedTrees[x, y];
    }

    public void RemoveTreeAt(int x, int y)
    {
        placedTrees[x, y] = null;
    }
}

