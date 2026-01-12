using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    float[,] terrainNoise;
    float[,] biomeNoise;
    float[,] weatherNoise;
    float[,] terrainVertexPositions;
    [SerializeField] Material terrainMaterial;
    [SerializeField] Material oceanMaterial;
    [SerializeField] GameObject[] Vegetation;
    [SerializeField] GameObject Grass;
    Vector3 cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    void Start()
    {
        terrainNoise = NoiseGenerator.GenerateNoiseMap(1000, 1000, 10, 32, 4, 0f, 16f, Vector2.zero);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(250, 250, 20, 32, 4, 16f, 16f, Vector2.zero);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, Vector2.zero);

        terrainVertexPositions = new float[100, 100];
        MeshGenerator.GenerateTerrainMesh(1000, 1000, 10, 5f, terrainNoise, out terrainVertexPositions, 40, 10, terrainMaterial);

        //GameObject ocean = MeshGenerator.GenerateSquareMesh(250, 250, 0, oceanMaterial);
        //ocean.transform.localPosition = new Vector3(0, 2, 0);

        VegetationPlanter.scatterVegetation(1000, 1000, Vegetation, Grass, terrainVertexPositions, biomeNoise);

    }


}
