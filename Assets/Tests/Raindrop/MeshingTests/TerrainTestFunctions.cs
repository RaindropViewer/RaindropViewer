using UnityEngine;

namespace Tests.Raindrop.MeshingTests
{
    public class TerrainTestFunctions
    {
        public static void GenerateNoiseZMap(uint width, uint height, ref float[,] zMap, float noiseAmplitude,
            float noisePeriodicity)
        {
            for (int row = 0; row < height; row++) // in each row...
            {
                for (int col = 0; col < width; col++) // in each col...
                {
                    float perlinXInput = (float)col * noisePeriodicity;
                    float perlinYInput = (float)row * noisePeriodicity;
                    // ... generate the noise at current position and apply....
                    float noiseVal = Mathf.PerlinNoise(perlinXInput, perlinYInput) * noiseAmplitude;
                    zMap[row, col] = noiseVal;
                }
            }
        }
    }
}