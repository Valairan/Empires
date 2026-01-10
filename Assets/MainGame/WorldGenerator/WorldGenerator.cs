using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    float[,] terrainNoise;
    float[,] biomeNoise;
    float[,] weatherNoise;
    Vector3 cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    void Start()
    {
        terrainNoise = NoiseGenerator.GenerateNoiseMap(200, 100, 10, 12, 4, 0f, 16f, Vector2.zero);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(10, 10, 10, 4, 4, 16f, 16f, Vector2.zero);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, Vector2.zero);

        GameObject terrain = MeshGenerator.GenerateTerrainMesh(100, 100, 5f, terrainNoise, 10, 10);
        terrain.transform.localScale = new Vector3(15, 15, 15);
        terrain.transform.position = new Vector3(-750, 0, -750);

        GameObject ocean = MeshGenerator.GenerateSquareMesh(20, 20, 30);
        ocean.transform.localScale = new Vector3(80, 1, 80);
        ocean.transform.position = new Vector3(-750, 0, -750);

    }
}
