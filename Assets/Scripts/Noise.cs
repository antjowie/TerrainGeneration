using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    // Lacunarity controls the increase in frequency of each octave
    // persistance controls the decrease in amplitude of each octave
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[width, height];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i].x = prng.Next(-10000, 10000) + offset.x;
            octaveOffsets[i].y = prng.Next(-10000, 10000) + offset.y;
        }

        if (scale <= 0f)
            scale = 0.0001f;

        int halfWidth = width / 2;
        int halfHeight = height / 2;

        float maxNoiseValue = float.MinValue;
        float minNoiseValue = float.MaxValue;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseValue = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                    noiseValue += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseValue < minNoiseValue)
                    minNoiseValue = noiseValue;
                if (noiseValue > maxNoiseValue)
                    maxNoiseValue = noiseValue;

                noiseMap[x, y] = noiseValue;
            }

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[x, y]);
            }


        return noiseMap;
    }
}
