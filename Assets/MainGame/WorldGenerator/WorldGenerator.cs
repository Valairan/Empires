using System;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    float[,] terrainNoise;
    float[,] biomeNoise;
    float[,] weatherNoise;
    [SerializeField] Material terrainMaterial;
    [SerializeField] Material oceanMaterial;
    [SerializeField] GameObject[] TreesToPlace;
    [SerializeField] public BaseResourceBehaviour[,] placedTrees;
    Transform mainCamera;
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
        TerrainSettings settings = new TerrainSettings();
        settings.mapWidth = 1000;
        settings.mapHeight = 1000;
        settings.seed = 10;
        settings.scale = 32;
        settings.octaves = 4;
        settings.persistance = 4;
        settings.lacunarity = 0;
        settings.multiplier = 5;
        settings.offset = Vector2.zero;
        settings.falloffHeight = 20;
        settings.falloffDistance = 5;
        GenerateTerrain(settings);
    }

    void Start()
    {
        //StartGameLocal();
    }

    public void GenerateTerrain(TerrainSettings settings)
    {
        mapHeight = settings.mapHeight;
        mapWidth = settings.mapWidth;
        totaleVegetationChunks = new VegetationChunk[vegetation.Length][,];
        terrainNoise = NoiseGenerator.GenerateSteppedNoiseMap(settings.mapWidth, settings.mapHeight, settings.seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.multiplier, settings.offset, settings.falloffHeight, settings.falloffDistance);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(250, 250, 20, 32, 4, 16f, 16f, 1f, Vector2.zero, 0, 0);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, 1f, Vector2.zero, 0, 0);

        MeshGenerator.GenerateTerrainMesh(settings.mapWidth, settings.mapHeight, 100, terrainMaterial, 6, terrainNoise);
        MeshGenerator.GenerateSquareMesh(settings.mapWidth, settings.mapHeight, 25, oceanMaterial, 4);

        placedTrees = VegetationPlanter.ScatterDecoration(settings.mapWidth, settings.mapHeight, 100, TreesToPlace, terrainNoise, 5, biomeNoise);
        for (int i = 0; i < vegetation.Length; i++)
        {
            totaleVegetationChunks[i] = VegetationPlanter.scatterGrassInChunks(settings, 5, vegetation[i].density, vegetation[i].isWaterPlant, vegetation[i].seed, vegetation[i].scaleRangeMin, vegetation[i].scaleRangeMax, vegetation[i].probability, terrainNoise);
        }

        renderParams = new RenderParams(grassMaterial);
        renderParams.receiveShadows = true;
        renderParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        generationComplete = true;
        mainCamera = Camera.main.transform;

    }

    Matrix4x4[] visibleMatrices = new Matrix4x4[1023]; // max batch size
    int maxInstances = 1023;


    const float chunkWorldSize = 5f;
    const float behindChunkRows = 1f;   // how many chunks behind to keep
    [SerializeField]
    [Range(-1, 1)]
    float forwardDotThreshold = 0.5f; // how far "behind" is acceptable
    void Update()
    {
        if (!generationComplete) return;

        Vector3 camPos = mainCamera.position;
        Vector3 camForward = mainCamera.forward;

        int positionX = (int)Math.Clamp(camPos.x, 0, mapWidth) / 5;
        int positionZ = (int)Math.Clamp(camPos.z, 0, mapHeight) / 5;

        for (int k = 0; k < vegetation.Length; k++)
        {
            int count = 0;

            for (int i = -numberOfChunksToRender; i <= numberOfChunksToRender; i++)
            {
                for (int j = -numberOfChunksToRender; j <= numberOfChunksToRender; j++)
                {
                    int chunkX = positionX + i;
                    int chunkZ = positionZ + j;

                    if (chunkX < 0 || chunkZ < 0 ||
                        chunkX >= mapWidth / 5 ||
                        chunkZ >= mapHeight / 5)
                        continue;

                    // -------- Directional chunk culling --------

                    Vector3 chunkCenter = new Vector3(
                        chunkX * chunkWorldSize + chunkWorldSize * 0.5f,
                        camPos.y,
                        chunkZ * chunkWorldSize + chunkWorldSize * 0.5f
                    );

                    Vector3 toChunk = chunkCenter - camPos;
                    float dot = Vector3.Dot(camForward, toChunk.normalized);

                    // How far behind camera this chunk is (in chunks)
                    float behindChunks = -Vector3.Dot(camForward, toChunk) / chunkWorldSize;

                    // HARD reject:
                    // - not in forward cone
                    // - AND more than N chunks behind
                    if (dot < forwardDotThreshold && behindChunks > behindChunkRows)
                        continue;

                    // ------------------------------------------

                    VegetationChunk chunk = totaleVegetationChunks[k][chunkX, chunkZ];
                    if (chunk.count == 0) continue;

                    for (int m = 0; m < chunk.count; m++)
                    {
                        visibleMatrices[count] = chunk.matrices[m];
                        count++;

                        if (count == maxInstances)
                        {
                            Graphics.RenderMeshInstanced(
                                renderParams,
                                vegetation[k].mesh,
                                0,
                                visibleMatrices
                            );
                            count = 0;
                        }
                    }
                }
            }

            // Render leftovers
            if (count > 0)
            {
                Matrix4x4[] leftover = new Matrix4x4[count];
                Array.Copy(visibleMatrices, leftover, count);

                Graphics.RenderMeshInstanced(
                    renderParams,
                    vegetation[k].mesh,
                    0,
                    leftover
                );
            }
        }
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
    [Range(0, 10)]
    public int probability;
}






[Serializable]
public struct TerrainSettings
{
    public int mapWidth;
    public int mapHeight;
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