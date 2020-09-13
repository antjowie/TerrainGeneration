using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        MeshData meshData = new MeshData(width, height);

        float topLeftX = ((float)width - 1) / -2;
        float topLeftZ = ((float)height - 1) / 2;

        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if(!(x == width - 1 || y == height - 1))
                {
                    int i = vertexIndex;
                    meshData.AddIndices(i, i + width + 1, i + width);
                    meshData.AddIndices(i + width + 1, i, i + 1);
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