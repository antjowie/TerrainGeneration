using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { HeightMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    public float lacunarity;
    [Range(0, 1)]
    public float persistance;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public int seed;
    public Vector2 offset;

    public bool liveUpdate;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                for (int i = 0; i < regions.Length; i++)
                {
                    if (noiseMap[x, y] <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }

        MapDisplay display = GetComponent<MapDisplay>();
        if (drawMode == DrawMode.HeightMap)
        {
            display.DisplayMap(TextureGenerator.CreateTextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DisplayMap(TextureGenerator.CreateTextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DisplayMesh(
                MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve).CreateMesh(),
                TextureGenerator.CreateTextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
    }

    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (noiseScale < 0) noiseScale = 0;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 1) octaves = 1;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}