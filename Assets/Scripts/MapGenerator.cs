using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { HeightMap, ColorMap, Mesh };
    
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorLOD;
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

    Queue<MapDataThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapDataThreadInfo<MapData>>();
    Queue<MapDataThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapDataThreadInfo<MeshData>>();

    public void DisplayMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = GetComponent<MapDisplay>();
        if (drawMode == DrawMode.HeightMap)
        {
            display.DisplayMap(TextureGenerator.CreateTextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DisplayMap(TextureGenerator.CreateTextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DisplayMesh(
                MeshGenerator.GenerateTerrainMesh(mapData.heightMap, editorLOD, heightMultiplier, heightCurve).CreateMesh(),
                TextureGenerator.CreateTextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Action<MapData> callback, Vector2 center)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback, center);
        };

        new Thread(threadStart).Start();
    }

    public void MapDataThread(Action<MapData> callback, Vector2 center)
    {
        MapData data = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapDataThreadInfo<MapData>(callback, data));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, float [,] heightMap, int lod)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(callback, heightMap, lod);
        };

        new Thread(threadStart).Start();
    }

    public void MeshDataThread(Action<MeshData> callback, float[,] heightMap, int lod)
    {
        MeshData data = MeshGenerator.GenerateTerrainMesh(heightMap,lod,heightMultiplier,heightCurve);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapDataThreadInfo<MeshData>(callback, data));
        }
    }

    void Update()
    {
        for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
        {
            MapDataThreadInfo<MapData> data = mapDataThreadInfoQueue.Dequeue();
            data.callback(data.parameter);
        }
        
        for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
        {
            MapDataThreadInfo<MeshData> data = meshDataThreadInfoQueue.Dequeue();
            data.callback(data.parameter);
        }
    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset,normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
            for (int x = 0; x < mapChunkSize; x++)
                for (int i = 0; i < regions.Length; i++)
                {
                    if (noiseMap[x, y] >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else break;
                }


        return new MapData(noiseMap, colorMap);
    }

    void OnValidate()
    {
        if (noiseScale < 0) noiseScale = 0;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 1) octaves = 1;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    struct MapDataThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapDataThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}