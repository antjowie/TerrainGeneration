using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                float x = (float)i / (float)size * 2f - 1f;
                float y = (float)j / (float)size * 2f - 1f;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i,j] = Evaluate(value);
            }
            
        return map;
    }
    
    static float Evaluate(float value)
    {
        // Equation taken from this video
        // https://www.youtube.com/watch?v=COmtTyLCd6I
        
        float a = 3;
        float b = 2.2f;
        
        return Mathf.Pow(value,a)/(Mathf.Pow(value,a) + Mathf.Pow(b-b*value,a));
    }
}
