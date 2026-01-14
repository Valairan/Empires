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


    public static void GenerateTerrainMesh(int mapWidth, int mapHeight, int chunkSize, float[,] noise, Material meshMaterial)
    {
        GameObject parent = new GameObject("Procedural Terrain");

        for (int i = 0; i <= mapHeight; i += chunkSize)
        {
            for (int j = 0; j <= mapWidth; j += chunkSize)
            {
                GameObject currentTerrainChunk = new GameObject("Terrain(" + i + "," + j + ")");
                currentTerrainChunk.transform.SetParent(parent.transform);

                GameObject lod2 = calculateLOD(i, j, currentTerrainChunk, mapWidth, mapHeight, chunkSize, noise, meshMaterial, 2);
                GameObject lod1 = calculateLOD(i, j, currentTerrainChunk, mapWidth, mapHeight, chunkSize, noise, meshMaterial, 1);
                GameObject lod0 = calculateLOD(i, j, currentTerrainChunk, mapWidth, mapHeight, chunkSize, noise, meshMaterial, 0);

                lod0.transform.SetParent(currentTerrainChunk.transform);
                lod1.transform.SetParent(currentTerrainChunk.transform);
                lod2.transform.SetParent(currentTerrainChunk.transform);

                LODGroup lodGroup = currentTerrainChunk.AddComponent<LODGroup>();

                lodGroup.SetLODs(new LOD[] {
                new LOD(0.6f, new Renderer[] { lod0.GetComponent<MeshRenderer>() }),
                new LOD(0.3f, new Renderer[] { lod1.GetComponent<MeshRenderer>() }),
                new LOD(0.1f, new Renderer[] { lod2.GetComponent<MeshRenderer>() })});

                lodGroup.RecalculateBounds();
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.animateCrossFading = true;
            }

        }
    }


    public static GameObject calculateLOD(int i, int j, GameObject parent, int mapWidth, int mapHeight, int chunkSize, float[,] noise, Material meshMaterial, int lodLevel)
    {

        GameObject terrain = new GameObject($"LOD{lodLevel}");
        terrain.transform.position = new Vector3(j, 0, i);
        terrain.transform.SetParent(parent.transform);

        MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();

        meshRenderer.material = meshMaterial;
        Mesh mesh = new Mesh();
        mesh.name = "Terrain Mesh";

        int skip = 1 << lodLevel; // 1, 2, 4


        int vertsPerSide = chunkSize / skip + 1;
        Vector3[] vertices = new Vector3[vertsPerSide * vertsPerSide];

        int[] triangles = new int[(vertsPerSide - 1) * (vertsPerSide - 1) * 6];
        int triangleIndex = 0;

        Vector2[] uvs = new Vector2[vertsPerSide * vertsPerSide];

        for (int y = 0; y < vertsPerSide; y++)
        {
            for (int x = 0; x < vertsPerSide; x++)
            {
                int worldX = j + x * skip;
                int worldY = i + y * skip;

                worldX = Mathf.Min(worldX, mapWidth - 1);
                worldY = Mathf.Min(worldY, mapHeight - 1);

                int v = y * vertsPerSide + x;

                float height = noise[worldX, worldY];
                vertices[v] = new Vector3(x * skip, height, y * skip);
                uvs[v] = new Vector2(
                    (float)worldX / mapWidth,
                    (float)worldY / mapHeight
                );
                if (x < vertsPerSide - 1 && y < vertsPerSide - 1)
                {
                    triangles[triangleIndex + 0] = v;
                    triangles[triangleIndex + 1] = v + vertsPerSide;
                    triangles[triangleIndex + 2] = v + vertsPerSide + 1;

                    triangles[triangleIndex + 3] = v;
                    triangles[triangleIndex + 4] = v + vertsPerSide + 1;
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

        return terrain;
    }


}