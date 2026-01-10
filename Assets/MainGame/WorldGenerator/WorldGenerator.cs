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
        terrainNoise = NoiseGenerator.GenerateNoiseMap(200, 200, 10, 12, 4, 0f, 16f, Vector2.zero);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(10, 10, 10, 4, 4, 16f, 16f, Vector2.zero);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, Vector2.zero);

        terrainVertexPositions = new float[100, 100];
        GameObject terrain = MeshGenerator.GenerateTerrainMesh(100, 100, 5f, terrainNoise, out terrainVertexPositions, 10, 10, terrainMaterial);
        terrain.transform.localScale = new Vector3(15, 15, 15);
        terrain.transform.position = new Vector3(-750, 0, -750);
        
        GameObject ocean = MeshGenerator.GenerateSquareMesh(160, 160, 35, oceanMaterial);
        ocean.transform.localScale = new Vector3(3, 1, 3);
        ocean.transform.position = new Vector3(-180, 0, -180);

    }
}
