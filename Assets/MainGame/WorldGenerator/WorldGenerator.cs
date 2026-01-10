using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    float[,] terrainNoise;
    float[,] biomeNoise;
    float[,] weatherNoise;
    float[,] terrainVertexPositions;
    [SerializeField] Material terrainMaterial;
    [SerializeField] Material oceanMaterial;
    Vector3 cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    void Start()
    {
        terrainNoise = NoiseGenerator.GenerateNoiseMap(250, 250, 10, 32, 4, 0f, 16f, Vector2.zero);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(10, 10, 10, 4, 4, 16f, 16f, Vector2.zero);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, Vector2.zero);

        terrainVertexPositions = new float[100, 100];
        GameObject terrain = MeshGenerator.GenerateTerrainMesh(250, 250, 5f, terrainNoise, out terrainVertexPositions, 40, 10, terrainMaterial);
        
        GameObject ocean = MeshGenerator.GenerateSquareMesh(250, 250, 0, oceanMaterial);
        ocean.transform.localPosition = new Vector3(0, 2, 0);
    }
}
