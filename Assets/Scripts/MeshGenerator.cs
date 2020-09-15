using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    // simplificationMultiplier represents the LOD. 
    public static MeshData GenerateTerrainMesh(float[,] heightMap, int simplificationMultiplier, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        
        int simplificationFactor = simplificationMultiplier == 0 ? 1 : simplificationMultiplier * 2;
        int verticesPerLine = (width - 1) / simplificationFactor + 1;
        
        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);

        float topLeftX = ((float)width - 1) / -2;
        float topLeftZ = ((float)height - 1) / 2;

        int vertexIndex = 0;

        for (int y = 0; y < height; y += simplificationFactor)
            for (int x = 0; x < width; x += simplificationFactor)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if(!(x == width - 1 || y == height - 1))
                {
                    int i = vertexIndex;
                    meshData.AddIndices(i, i + verticesPerLine + 1, i + verticesPerLine);
                    meshData.AddIndices(i + verticesPerLine + 1, i, i + 1);
                }

                vertexIndex++;
            }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] indices;
    public Vector2[] uvs;

    int indicesIndex;

    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        indices = new int[(width - 1) * (height - 1) * 6];
        uvs = new Vector2[width * height];
    }

    public void AddIndices(int a, int b, int c)
    {
        indices[indicesIndex + 0] = a;
        indices[indicesIndex + 1] = b;
        indices[indicesIndex + 2] = c;

        indicesIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}