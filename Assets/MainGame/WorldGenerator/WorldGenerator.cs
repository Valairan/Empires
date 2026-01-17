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
    [SerializeField] GameObject[] TreesAndDecor;
    Vector3 cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    Transform mainCamera;
    bool generationComplete;
    public VegetationType[] vegetation;
    public VegetationChunk[][,] totaleVegetationChunks;
    RenderParams renderParams;
    [SerializeField] Mesh grassMesh;
    [SerializeField] Material grassMaterial;
    [SerializeField] int numberOfChunksToRender;

    int mapWidth = 0;
    int mapHeight = 0;


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

        VegetationPlanter.ScatterDecoration(settings.mapWidth, settings.mapHeight, 100, TreesAndDecor, terrainNoise, 5, biomeNoise);
        for (int i = 0; i < vegetation.Length; i++)
        {
            totaleVegetationChunks[i] = VegetationPlanter.scatterGrassInChunks(settings, 10, vegetation[i].density, vegetation[i].seed, vegetation[i].probability, terrainNoise);

        }

        renderParams = new RenderParams(grassMaterial);
        renderParams.receiveShadows = true;
        renderParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

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
                int positionx = (int)Math.Clamp(mainCamera.position.x, 0, mapWidth) / 10;
                int positionz = (int)Math.Clamp(mainCamera.position.z, 0, mapHeight) / 10;

                for (int i = -numberOfChunksToRender; i <= numberOfChunksToRender; i++)
                {
                    for (int j = -numberOfChunksToRender; j <= numberOfChunksToRender; j++)
                    {
                        int chunkX = positionx + i;
                        int chunkZ = positionz + j;


                        if (chunkX < 0 || chunkZ < 0 || chunkX >= 1000 / 10 || chunkZ >= 1000 / 10)
                            continue;
                        for (int k = 0; k < vegetation.Length; k++)
                        {

                            VegetationChunk chunk = totaleVegetationChunks[k][chunkX, chunkZ];
                            if (chunk.count == 0)
                                continue;
                            Graphics.RenderMeshInstanced(renderParams, vegetation[k].mesh, 0, chunk.matrices);

                        }


                    }

                }
            }
        }
    }

}


[Serializable]
public struct VegetationType
{
    public int density;
    public int seed;
    public Mesh mesh;
    public Material material;
    public int submesh;
    public float probability;
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