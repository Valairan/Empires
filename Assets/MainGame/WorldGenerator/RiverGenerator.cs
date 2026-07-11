using System.Collections.Generic;
using UnityEngine;

public static class RiverGenerator
{
    public enum direction { Up, Down, Left, Right };
    public static float[] CarveRiver(int mapWidth, int mapHeight, int seed, float riverWidth, float cost, float[] heightMap)
    {
        int maxRiverLength = 1000;
        int riverLength = 0;
        direction currentDirection = GetRandomDirection(seed);
        int startPointX = 0, startPointY = 0;



        switch (currentDirection)
        {
            case direction.Up:
                startPointX = Random.Range(0, mapWidth);
                startPointY = 0;
                break;
            case direction.Down:
                startPointX = Random.Range(0, mapWidth);
                startPointY = mapHeight - 1;
                break;
            case direction.Left:
                startPointX = 0;
                startPointY = Random.Range(0, mapHeight);
                break;
            case direction.Right:
                startPointX = mapWidth - 1;
                startPointY = Random.Range(0, mapHeight);
                break;
        }


        while (true)
        {
            heightMap[startPointY * mapWidth + startPointX] = 0f;
            heightMap[(startPointY + 1) * mapWidth + startPointX] = 0f;
            heightMap[(startPointY + 1) * mapWidth + startPointX + 1] = 0f;
            heightMap[(startPointY + 1) * mapWidth + startPointX - 1] = 0f;
            heightMap[(startPointY - 1) * mapWidth + startPointX] = 0f;
            heightMap[(startPointY - 1) * mapWidth + startPointX + 1] = 0f;
            heightMap[(startPointY - 1) * mapWidth + startPointX - 1] = 0f;


            switch (GetRandomDirection(seed))
            {
                case direction.Up:
                    if (currentDirection == direction.Up) continue;
                    if (startPointY < mapHeight - 1)
                    {
                        startPointY += 1;
                    }
                    break;
                case direction.Down:
                    if (currentDirection == direction.Down) continue;
                    if (startPointY > 0)
                    {
                        startPointY -= 1;
                    }
                    break;
                case direction.Left:
                    if (currentDirection == direction.Left) continue;
                    if (startPointX > 0)
                    {
                        startPointX -= 1;
                    }
                    break;
                case direction.Right:
                    if (currentDirection == direction.Right) continue;
                    if (startPointX < mapWidth - 1)
                    {
                        startPointX += 1;
                    }
                    break;
            }

            riverLength++;
            if (riverLength >= maxRiverLength)
            {
                break;
            }
        }
        return heightMap;
    }

    public static void dijkstras(int startX, int startY, int endX, int endY, float[] heightMap, int mapWidth, int mapHeight)
    {

        throw new System.NotImplementedException("Dijkstra's algorithm is not implemented yet.");
    }

    public static direction GetRandomDirection(int seed)
    {
        return (direction)new System.Random(seed).Next(0, 3);
    }
}