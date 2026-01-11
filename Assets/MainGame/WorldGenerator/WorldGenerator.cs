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
        terrainNoise = NoiseGenerator.GenerateNoiseMap(250, 250, 10, 32, 4, 0f, 16f, Vector2.zero);
        biomeNoise = NoiseGenerator.GenerateNoiseMap(10, 10, 10, 4, 4, 16f, 16f, Vector2.zero);
        weatherNoise = NoiseGenerator.GenerateNoiseMap(100, 100, 10, 4, 4, 16f, 16f, Vector2.zero);

        terrainVertexPositions = new float[100, 100];
        GameObject terrain = MeshGenerator.GenerateTerrainMesh(250, 250, 100, 5f, terrainNoise, out terrainVertexPositions, 40, 10, terrainMaterial);
        
        GameObject ocean = MeshGenerator.GenerateSquareMesh(250, 250, 0, oceanMaterial);
        ocean.transform.localPosition = new Vector3(0, 2, 0);

        scatterVegetation(250, 250, Vegetation, Grass, terrainVertexPositions);

    }

    public static void scatterVegetation(int mapHeight, int mapWidth, GameObject[] vegetation, GameObject grass, float[,] availableSpots)
    {
        int availableItems = vegetation.Length;
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                bool spawn = Random.Range(0, 100) > 98;
                if (!(availableSpots[i, j] < 0.1f))
                {
                    if (spawn)
                        Instantiate(vegetation[(int)Random.Range(0, availableItems)], new Vector3(i, availableSpots[i, j], j), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                    else
                    {
                        spawn = Random.Range(0, 100) > 50;
                        if (i - 1 > 0 && i + 1 < mapHeight && j - 1 > 0 && j + 1 < mapHeight)
                        {
                            if (availableSpots[i + 1, j + 1] == availableSpots[i, j] &&
                                    availableSpots[i + 1, j - 1] == availableSpots[i, j] &&
                                        availableSpots[i - 1, j + 1] == availableSpots[i, j] &&
                                        availableSpots[i - 1, j - 1] == availableSpots[i, j])
                            {
                                if (true)
                                {
                                    Instantiate(grass, new Vector3(i, availableSpots[i, j], j), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i + Random.Range(0, 0.5f), availableSpots[i, j], j + Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i + Random.Range(0, 0.5f), availableSpots[i, j], j - Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i - Random.Range(0, 0.5f), availableSpots[i, j], j + Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i - Random.Range(0, 0.5f), availableSpots[i, j], j - Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i + Random.Range(0, 0.25f), availableSpots[i, j], j + Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i + Random.Range(0, 0.25f), availableSpots[i, j], j - Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i - Random.Range(0, 0.25f), availableSpots[i, j], j + Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                    Instantiate(grass, new Vector3(i - Random.Range(0, 0.25f), availableSpots[i, j], j - Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)));
                                }
                            }
                        }

                    }

                }
                //                if (availableSpots[i, j] == availableSpots[i, j + 1])
                //              {
                //                Instantiate(grass, new Vector3(i, availableSpots[i, j], (j + j + 1) / 2), Quaternion.identity);
                //          }
            }

        }
    }
}
