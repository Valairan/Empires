using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class MeshGenerator
{

    public static GameObject GenerateSquareMesh(int mapWidth, int mapHeight, float height, Material meshMaterial)
    {
        GameObject terrain = new GameObject("Procedural Terrain");

        MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();
        meshRenderer.material = meshMaterial;
        Mesh mesh = new Mesh();
        mesh.name = "Terrain Mesh";

        Vector3[] vertices = new Vector3[mapWidth * mapHeight];
        Vector2[] uvs = new Vector2[mapWidth * mapHeight];
        int[] triangles = new int[(mapWidth - 1) * (mapHeight - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                vertices[vertexIndex] = new Vector3(x, height, y);
                uvs[vertexIndex] = new Vector2(
                    (float)x / mapWidth,
                    (float)y / mapHeight
                );

                if (x < mapWidth - 1 && y < mapHeight - 1)
                {
                    triangles[triangleIndex + 0] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + mapWidth;
                    triangles[triangleIndex + 2] = vertexIndex + mapWidth + 1;

                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + mapWidth + 1;
                    triangles[triangleIndex + 5] = vertexIndex + 1;

                    triangleIndex += 6;
                }

                vertexIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        return terrain;
    }
    public static void GenerateTerrainMesh(int mapWidth, int mapHeight, int chunkSize, float heightMultiplier, float[,] noise, out float[,] vertexHeights, int falloffHeight, int falloffDistance, Material meshMaterial)
    {

        vertexHeights = new float[mapHeight, mapWidth];

        GameObject parent = new GameObject("Procedural Terrain");

        int chunkIdWidth = 0;
        int chunkIdHeight = 0;

        for (int i = 0; i <= mapHeight; i += chunkSize - 1)
        {
            for (int j = 0; j <= mapWidth; j += chunkSize - 1)
            {
                GameObject terrain = new GameObject("Terrain(" + i + "," + j + ")");


                // chunk world position
                //terrain.transform.position = new Vector3(j - chunkIdWidth, 0, i - chunkIdHeight);
                terrain.transform.position = new Vector3(j, 0, i);
                terrain.transform.SetParent(parent.transform);


                MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
                MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();

                meshRenderer.material = meshMaterial;
                Mesh mesh = new Mesh();
                mesh.name = "Terrain Mesh";

                Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
                Vector2[] uvs = new Vector2[(chunkSize + 1) * (chunkSize + 1)];

                int[] triangles = new int[(chunkSize - 1) * (chunkSize - 1) * 6];

                int triangleIndex = 0;

                for (int y = 0; y <= chunkSize; y++)
                {
                    for (int x = 0; x <= chunkSize; x++)
                    {
                        int worldX = x + j;
                        int worldY = y + i;

                        worldX = Mathf.Min(worldX, mapWidth - 1);
                        worldY = Mathf.Min(worldY, mapHeight - 1);

                        if (worldY > mapHeight - 1 || worldX > mapWidth - 1) return;

                        float height = 0f;

                        if (worldY <= falloffDistance || worldX <= falloffDistance || worldY >= mapHeight - falloffDistance || worldX >= mapWidth - falloffDistance)
                        {
                            height = falloffHeight;
                        }
                        else
                        {
                            if (noise[worldX, worldY] < 1f) height = 4f;
                            if (noise[worldX, worldY] < 0.75f) height = 2f;
                            if (noise[worldX, worldY] < 0.5f) height = 1f;
                            if (noise[worldX, worldY] < 0.25f) height = 0f;
                            height = height * heightMultiplier;
                        }

                        int v = y * (chunkSize + 1) + x;

                        vertices[v] = new Vector3(x, height, y);
                        vertexHeights[worldX, worldY] = height;
                        uvs[v] = new Vector2(
                            (float)worldX / mapWidth,
                            (float)worldY / mapHeight
                        );
                        if (x < chunkSize - 1 && y < chunkSize - 1)
                        {
                            triangles[triangleIndex + 0] = v;
                            triangles[triangleIndex + 1] = v + chunkSize + 1;
                            triangles[triangleIndex + 2] = v + chunkSize + 2;

                            triangles[triangleIndex + 3] = v;
                            triangles[triangleIndex + 4] = v + chunkSize + 2;
                            triangles[triangleIndex + 5] = v + 1;

                            triangleIndex += 6;
                        }


                    }
                }

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                meshFilter.mesh = mesh;
                meshCollider.sharedMesh = mesh;
                chunkIdWidth++;
            }
            chunkIdWidth = 0;
            chunkIdHeight++;

        }
    }

}