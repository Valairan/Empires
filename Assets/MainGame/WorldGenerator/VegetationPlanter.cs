using UnityEngine;

public static class VegetationPlanter
{
    public static void scatterVegetation(int mapHeight, int mapWidth, GameObject[] vegetation, GameObject grass, float[,] availableSpots, float[,] biome)
    {
        GameObject treeparent = new GameObject("Tree Parent");
        GameObject grassParent = new GameObject("Grass Parent");
        int biomeDimensions = biome.Length;
        int availableItems = vegetation.Length;
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                bool spawn = Random.Range(0f, 1f) > 0.97f;
                int whatToSpawn = (int)(biome[j * ((mapWidth - 1) / (biomeDimensions - 1)), i * ((mapHeight - 1) / (biomeDimensions - 1))] * 5);

                Debug.Log(whatToSpawn);

                if (!((availableSpots[i, j] <= 1) && (availableSpots[i, j] > 30)))
                {
                    if (spawn)
                        GameObject.Instantiate(vegetation[whatToSpawn > 5 ? 2 : 0], new Vector3(i, availableSpots[i, j], j), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), treeparent.transform);
                    else
                    {
                        spawn = Random.Range(0, 100) > 20;
                        if (i - 1 > 0 && i + 1 < mapHeight && j - 1 > 0 && j + 1 < mapHeight)
                        {
                            if (availableSpots[i + 1, j + 1] == availableSpots[i, j] &&
                                    availableSpots[i + 1, j - 1] == availableSpots[i, j] &&
                                        availableSpots[i - 1, j + 1] == availableSpots[i, j] &&
                                        availableSpots[i - 1, j - 1] == availableSpots[i, j])
                            {
                                if (true)
                                {
                                    GameObject.Instantiate(grass, new Vector3(i, availableSpots[i, j], j), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i + Random.Range(0, 0.5f), availableSpots[i, j], j + Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i + Random.Range(0, 0.5f), availableSpots[i, j], j - Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i - Random.Range(0, 0.5f), availableSpots[i, j], j + Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i - Random.Range(0, 0.5f), availableSpots[i, j], j - Random.Range(0, 0.5f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i + Random.Range(0, 0.25f), availableSpots[i, j], j + Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i + Random.Range(0, 0.25f), availableSpots[i, j], j - Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i - Random.Range(0, 0.25f), availableSpots[i, j], j + Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
                                    GameObject.Instantiate(grass, new Vector3(i - Random.Range(0, 0.25f), availableSpots[i, j], j - Random.Range(0, 0.25f)), Quaternion.Euler(new Vector3(-90, Random.Range(0, 360), 0)), grassParent.transform);
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
