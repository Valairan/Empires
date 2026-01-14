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
    public static Matrix4x4[] grassMatricesTotal;
    RenderParams grassrenderparams;
    [SerializeField] Mesh grassMesh;
    [SerializeField] Material grassMaterial;
    int numberOfChunksToRender = 4;
    Matrix4x4[] toRender = new Matrix4x4[10];

    void Start()
    {
        terrainNoise = NoiseGenerator.GenerateSteppedNoiseMap(settings.mapWidth, settings.mapHeight, settings.seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.multiplier, settings.offset, settings.falloffHeight, settings.falloffDistance);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(250, 250, 20, 32, 4, 16f, 16f, 1f, Vector2.zero, 0, 0);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, 1f, Vector2.zero, 0, 0);

        MeshGenerator.GenerateTerrainMesh(1000, 1000, 100, terrainNoise, terrainMaterial);

        //GameObject ocean = MeshGenerator.GenerateSquareMesh(250, 250, 0, oceanMaterial);
        //ocean.transform.localPosition = new Vector3(0, 2, 0);

        VegetationPlanter.ScatterDecoration(1000, 1000, 100, Vegetation, terrainNoise, 5, biomeNoise);

        grassMatricesTotal = VegetationPlanter.CalculateGrassPositions(1000, 1000, terrainNoise);



        grassrenderparams = new RenderParams(grassMaterial);
        for (int i = 0; i < 10; i++)
        {
            toRender[i] = grassMatricesTotal[i];


        }
        generationComplete = true;
        mainCamera = Camera.main.transform;

    }


    void Update()
    {
        //Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        if (generationComplete)
        {

            Graphics.RenderMeshInstanced(grassrenderparams, grassMesh, 0, toRender);
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