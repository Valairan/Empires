using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class VegetationPlanter
{
    struct GrassChunk
    {
        public Vector2Int coord;
        public Matrix4x4[] matrices;
        public int writeIndex;
    }


    public static Matrix4x4[] CalculateGrassPositions(int mapHeight, int mapWidth, float[,] availableSpots)
    {
        const int GrassPerCell = 8;

        Matrix4x4[] grassMatrices = new Matrix4x4[mapHeight * mapWidth];

        int writeIndex = 0;

        for (int x = 1; x < mapHeight - 1; x++)
        {
            for (int y = 1; y < mapWidth - 1; y++)
            {
                float height = availableSpots[x, y];

                if (availableSpots[x + 1, y + 1] == height &&
                    availableSpots[x + 1, y - 1] == height &&
                    availableSpots[x - 1, y + 1] == height &&
                    availableSpots[x - 1, y - 1] == height)
                {
                    float offsetX = Random.Range(0f, 0.25f);
                    float offsetZ = Random.Range(0f, 0.25f);

                    float scale = Random.Range(0.5f, 2f);
                    float rotY = Random.Range(0f, 360f);

                    grassMatrices[y * mapWidth + x] = Matrix4x4.TRS(new Vector3(x + offsetX, height, y + offsetZ),
                            Quaternion.Euler(0f, rotY, 0f),
                            Vector3.one * scale
                        );

                }
            }
        }

        // Trim unused space (optional but recommended)

        return grassMatrices;
    }


    public static void ScatterDecoration(int mapHeight, int mapWidth, int chunkSize, GameObject[] vegetation, float[,] availableSpots, int skip, float[,] biome)
    {
        GameObject treeParent = new GameObject("Tree Parent");

        int biomeDimensionsX = biome.GetLength(0);
        int biomeDimensionsY = biome.GetLength(1);
        int availableItems = vegetation.Length;
        int treeCount = 0;


        for (int x = 0; x < mapHeight; x++)
        {
            for (int y = 0; y < mapWidth; y++)
            {
                float height = availableSpots[x, y];
                if (height <= 1f || height > 30f)
                    continue;
                if (x > 0 && x < mapHeight - 1 && y > 0 && y < mapWidth - 1)

                    if (availableSpots[x + 1, y + 1] == height && availableSpots[x + 1, y - 1] == height && availableSpots[x - 1, y + 1] == height && availableSpots[x - 1, y - 1] == height)
                    {

                        bool spawn = Random.Range(0f, 1f) > 0.998f;

                        //int whatToSpawn = (int)Mathf.Clamp(biome[y / ((mapWidth - 1) / (biomeDimensionsY - 1)), x / ((mapHeight - 1) / (biomeDimensionsX - 1))] * 5, 0, availableItems - 1);
                        int whatToSpawn = Random.Range(0, availableItems - 1);

                        if (!((availableSpots[x, y] <= 1) && (availableSpots[x, y] > 30)))
                        {
                            if (spawn)
                            {
                                GameObject.Instantiate(vegetation[whatToSpawn], new Vector3(x, availableSpots[x, y], y), Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f)), treeParent.transform).isStatic = true;
                                treeCount++;
                            }


                        }

                        //Loader.instance.setProgress(x / mapHeight);
                    }
            }

        }

        //StaticBatchingUtility.Combine(grassParentSub);
        //StaticBatchingUtility.Combine(treeParentSub);
    }
}
