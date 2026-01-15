using System;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    float[,] terrainNoise;
    float[,] biomeNoise;
    float[,] weatherNoise;
    [SerializeField] TerrainSettings settings;
    [SerializeField] Material terrainMaterial;
    [SerializeField] Material oceanMaterial;


    [SerializeField] GameObject[] Vegetation;
    Vector3 cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    Transform mainCamera;
    bool generationComplete;
    public GrassChunk[,] totalGrassChunks;
    RenderParams grassrenderparams;
    [SerializeField] Mesh grassMesh;
    [SerializeField] Material grassMaterial;
    [SerializeField] int numberOfChunksToRender;

    void Start()
    {
        terrainNoise = NoiseGenerator.GenerateSteppedNoiseMap(settings.mapWidth, settings.mapHeight, settings.seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.multiplier, settings.offset, settings.falloffHeight, settings.falloffDistance);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(250, 250, 20, 32, 4, 16f, 16f, 1f, Vector2.zero, 0, 0);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, 1f, Vector2.zero, 0, 0);

        MeshGenerator.GenerateTerrainMesh(1000, 1000, 100, terrainNoise, terrainMaterial);

        //GameObject ocean = MeshGenerator.GenerateSquareMesh(250, 250, 0, oceanMaterial);
        //ocean.transform.localPosition = new Vector3(0, 2, 0);

        VegetationPlanter.ScatterDecoration(1000, 1000, 100, Vegetation, terrainNoise, 5, biomeNoise);

        totalGrassChunks = VegetationPlanter.scatterGrassInChunks(settings, 10, 8, 4, terrainNoise);

        grassrenderparams = new RenderParams(grassMaterial);
        grassrenderparams.receiveShadows = true;
        grassrenderparams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        generationComplete = true;
        mainCamera = Camera.main.transform;

    }


    void Update()
    {
        if (generationComplete)
        {
            //Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
            if (generationComplete)
            {
                int positionx = (int)Math.Clamp(mainCamera.position.x, 0, settings.mapWidth) / 10;
                int positionz = (int)Math.Clamp(mainCamera.position.z, 0, settings.mapHeight) / 10;

                for (int i = -numberOfChunksToRender; i <= numberOfChunksToRender; i++)
                {
                    for (int j = -numberOfChunksToRender; j <= numberOfChunksToRender; j++)
                    {
                        int chunkX = positionx + i;
                        int chunkZ = positionz + j;

                        if (chunkX < 0 || chunkZ < 0 || chunkX >= 1000 / 32 || chunkZ >= 1000 / 32)
                            continue;

                        GrassChunk chunk = totalGrassChunks[chunkX, chunkZ];
                        if (chunk.count == 0)
                            continue;
                        Graphics.RenderMeshInstanced(grassrenderparams, grassMesh, 0, chunk.matrices);


                    }

                }
            }
        }
    }

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