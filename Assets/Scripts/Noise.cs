using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }

    // Lacunarity controls the increase in frequency of each octave
    // persistance controls the decrease in amplitude of each octave
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[width, height];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float amplitude = 1;
        float maxPossibleHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i].x = prng.Next(-10000, 10000) + offset.x;
            octaveOffsets[i].y = prng.Next(-10000, 10000) - offset.y;

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
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
                amplitude = 1;
                float frequency = 1;
                float noiseValue = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

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
                if (normalizeMode == NormalizeMode.Local)
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[x, y]);
                else if (normalizeMode == NormalizeMode.Global)
                {
                    float noiseHeight = (noiseMap[x, y] + 1f) / (2f * maxPossibleHeight / 1.75f);
                    noiseMap[x, y] = Mathf.Clamp(noiseHeight, 0, int.MaxValue);
                }
            }


        return noiseMap;
    }
}
