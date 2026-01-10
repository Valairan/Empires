using Unity.Mathematics;
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
    public static GameObject GenerateTerrainMesh(int mapWidth, int mapHeight, float heightMultiplier, float[,] noise, out float[,] vertexHeights, int falloffHeight, int falloffDistance, Material meshMaterial)
    {
        GameObject terrain = new GameObject("Procedural Terrain");

        MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();

        meshRenderer.material = meshMaterial;
        Mesh mesh = new Mesh();
        mesh.name = "Terrain Mesh";

        Vector3[] vertices = new Vector3[mapWidth * mapHeight];
        vertexHeights = new float[mapHeight, mapWidth];
        Vector2[] uvs = new Vector2[mapWidth * mapHeight];
        int[] triangles = new int[(mapWidth - 1) * (mapHeight - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float height = 0f;

                if (y <= falloffDistance || x <= falloffDistance || y >= mapHeight - falloffDistance || x >= mapWidth - falloffDistance)
                {
                    height = falloffHeight;
                }
                else
                {
                    if (noise[x, y] < 1f) height = 1.25f;
                    if (noise[x, y] < 0.75f) height = 0.75f;
                    if (noise[x, y] < 0.5f) height = 0.5f;
                    if (noise[x, y] < 0.25f) height = 0f;
                    height = height * heightMultiplier;
                }


                vertices[vertexIndex] = new Vector3(x, height, y);
                vertexHeights[x, y] = height;
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

}