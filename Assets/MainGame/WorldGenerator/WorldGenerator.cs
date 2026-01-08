using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] AnimationCurve heightCurve;
    [SerializeField] int mapSizeX;
    [SerializeField] int mapSizeY;
    [SerializeField] float scale;
    [SerializeField] float persistance;
    [SerializeField] float lacunarity;
    [SerializeField] Vector2 offset;

    void Start()
    {
        int biomeSeed = Random.Range(0, 100);
        int terrainSeed = Random.Range(0, 100);
        int weatherSeed = Random.Range(0, 100);

        //send the above values to the server


        //generate perlin noise using seeds
        float[,] biomeNoise = MeshGenerator.GenerateNoiseMap(100, 100, biomeSeed, 100, 4, 4, 4, Vector2.zero);
        float[,] terrainNoise = MeshGenerator.GenerateNoiseMap(100, 100, terrainSeed, 100, 4, 4, 4, Vector2.zero);
        float[,] weatherNoise = MeshGenerator.GenerateNoiseMap(100, 100, weatherSeed, 100, 4, 4, 4, Vector2.zero);

        GameObject terrain = MeshGenerator.GenerateTerrainGrid(100, 100, terrainNoise, 2, heightCurve);
        terrain.transform.SetParent(transform);
    }
}
