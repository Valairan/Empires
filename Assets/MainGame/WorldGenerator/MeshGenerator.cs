using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class MeshGenerator
{
    private struct LodMeshData
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector2[] Uvs;
    }

    private struct ChunkBuildResult
    {
        public int ChunkX;
        public int ChunkY;
        public LodMeshData[] LodMeshes;
    }

    public static void GenerateSquareMesh(int mapWidth, int mapHeight, int chunkSize, Material meshMaterial, int layer)
    {

        GenerateTerrainMesh(mapWidth, mapHeight, chunkSize, meshMaterial, layer, null, UnityEngine.Rendering.ShadowCastingMode.Off);
    }


    public static void GenerateTerrainMesh(int mapWidth, int mapHeight, int chunkSize, Material meshMaterial, int layer, float[] noise = null, UnityEngine.Rendering.ShadowCastingMode shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On)
    {
        GameObject parent = new GameObject("Procedural Terrain");
        List<(int ChunkX, int ChunkY)> chunkPositions = new List<(int ChunkX, int ChunkY)>();

        for (int i = 0; i <= mapHeight; i += chunkSize)
        {
            for (int j = 0; j <= mapWidth; j += chunkSize)
            {
                chunkPositions.Add((j, i));
            }
        }

        ChunkBuildResult[] chunkResults = new ChunkBuildResult[chunkPositions.Count];
        Parallel.For(0, chunkPositions.Count, index =>
        {
            (int ChunkX, int ChunkY) position = chunkPositions[index];
            chunkResults[index] = BuildChunkMeshData(position.ChunkX, position.ChunkY, mapWidth, mapHeight, chunkSize, noise);
        });

        for (int index = 0; index < chunkResults.Length; index++)
        {
            ChunkBuildResult result = chunkResults[index];
            GameObject currentTerrainChunk = new GameObject($"Terrain({result.ChunkY},{result.ChunkX})");
            currentTerrainChunk.transform.SetParent(parent.transform);
            currentTerrainChunk.gameObject.layer = layer;

            GameObject[] lodObjects = new GameObject[3];
            for (int lodLevel = 0; lodLevel < result.LodMeshes.Length; lodLevel++)
            {
                GameObject lodObject = CreateLodObject(result.ChunkX, result.ChunkY, result.LodMeshes[lodLevel], meshMaterial, shadowCastingMode, layer, lodLevel);
                lodObject.transform.SetParent(currentTerrainChunk.transform);
                lodObjects[lodLevel] = lodObject;
            }

            GameObject lod2 = lodObjects[2];
            GameObject lod1 = lodObjects[1];
            GameObject lod0 = lodObjects[0];

            lod0.gameObject.layer = layer;
            lod1.gameObject.layer = layer;
            lod2.gameObject.layer = layer;

            LODGroup lodGroup = currentTerrainChunk.AddComponent<LODGroup>();
            lodGroup.SetLODs(new LOD[]
            {
                new LOD(0.6f, new Renderer[] { lod0.GetComponent<MeshRenderer>() }),
                new LOD(0.3f, new Renderer[] { lod1.GetComponent<MeshRenderer>() }),
                new LOD(0.1f, new Renderer[] { lod2.GetComponent<MeshRenderer>() })
            });

            lodGroup.RecalculateBounds();
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;

            parent.isStatic = true;
            lod2.isStatic = true;
            lod1.isStatic = true;
            lod0.isStatic = true;
        }
    }

    private static ChunkBuildResult BuildChunkMeshData(int chunkX, int chunkY, int mapWidth, int mapHeight, int chunkSize, float[] noise)
    {
        ChunkBuildResult result = new ChunkBuildResult
        {
            ChunkX = chunkX,
            ChunkY = chunkY,
            LodMeshes = new LodMeshData[3]
        };

        for (int lodLevel = 0; lodLevel < result.LodMeshes.Length; lodLevel++)
        {
            result.LodMeshes[lodLevel] = BuildLodMeshData(chunkX, chunkY, mapWidth, mapHeight, chunkSize, lodLevel, noise);
        }

        return result;
    }

    private static LodMeshData BuildLodMeshData(int chunkX, int chunkY, int mapWidth, int mapHeight, int chunkSize, int lodLevel, float[] noise)
    {
        int skip = 1 << lodLevel;
        int vertsPerSide = chunkSize / skip + 1;
        Vector3[] vertices = new Vector3[vertsPerSide * vertsPerSide];
        int[] triangles = new int[(vertsPerSide - 1) * (vertsPerSide - 1) * 6];
        Vector2[] uvs = new Vector2[vertsPerSide * vertsPerSide];

        int triangleIndex = 0;

        for (int y = 0; y < vertsPerSide; y++)
        {
            for (int x = 0; x < vertsPerSide; x++)
            {
                int worldX = chunkX + x * skip;
                int worldY = chunkY + y * skip;

                worldX = Mathf.Min(worldX, mapWidth - 1);
                worldY = Mathf.Min(worldY, mapHeight - 1);

                int vertexIndex = y * vertsPerSide + x;
                float height = 4.5f;
                if (noise != null)
                {
                    height = noise[worldY * mapWidth + worldX];
                }

                vertices[vertexIndex] = new Vector3(x * skip, height, y * skip);
                uvs[vertexIndex] = new Vector2(
                    (float)worldX / mapWidth,
                    (float)worldY / mapHeight
                );

                if (x < vertsPerSide - 1 && y < vertsPerSide - 1)
                {
                    triangles[triangleIndex + 0] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + vertsPerSide;
                    triangles[triangleIndex + 2] = vertexIndex + vertsPerSide + 1;
                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + vertsPerSide + 1;
                    triangles[triangleIndex + 5] = vertexIndex + 1;
                    triangleIndex += 6;
                }
            }
        }

        return new LodMeshData
        {
            Vertices = vertices,
            Triangles = triangles,
            Uvs = uvs
        };
    }

    private static GameObject CreateLodObject(int chunkX, int chunkY, LodMeshData meshData, Material meshMaterial, UnityEngine.Rendering.ShadowCastingMode shadowCastingMode, int layer, int lodLevel)
    {
        GameObject terrain = new GameObject($"LOD{lodLevel}");
        terrain.transform.position = new Vector3(chunkX, 0, chunkY);

        MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();

        meshRenderer.sharedMaterial = meshMaterial;
        meshRenderer.shadowCastingMode = shadowCastingMode;
        meshRenderer.staticShadowCaster = true;
        Mesh mesh = new Mesh();
        mesh.name = "Terrain Mesh";
        mesh.vertices = meshData.Vertices;
        mesh.triangles = meshData.Triangles;
        mesh.uv = meshData.Uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        if (lodLevel == 0)
        {
            meshCollider.sharedMesh = mesh;
        }

        terrain.layer = layer;
        return terrain;
    }


}