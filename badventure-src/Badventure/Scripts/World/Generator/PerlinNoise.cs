using System;
using System.Drawing;

namespace MapMatrix2d.Generator
{
    public static class PerlinNoise
    {
        /// <summary>  
        /// Generates a map of Perlin noise.   
        /// </summary>  
        /// <param name="width">Width of the map to be generated.</param>  
        /// <param name="height">Height of the map to be generated.</param>  
        /// <param name="frequency">Controls the scale of the noise features. Higher values result in smaller, more detailed patterns, while lower values create larger, smoother features.</param>  
        /// <param name="amplitude">Controls the intensity or height of the noise. Higher values produce more pronounced variations, while lower values result in flatter noise.</param>  
        /// <param name="persistence">Controls how much each octave contributes to the final noise. Lower values reduce the influence of higher octaves, creating smoother noise.</param>  
        /// <param name="octaves">The number of layers of noise added together. Each octave adds finer details at a higher frequency, increasing the complexity of the noise.</param>  
        /// <param name="seed">The seed value for the random number generator. Ensures that the same seed produces the same noise map, allowing for reproducibility.</param>  
        /// <param name="power">A value applied to the noise to modify its distribution. Values greater than 1 reduce low values and emphasize high values, while values less than 1 do the opposite. Useful for adjusting the balance between low and high areas.</param>  
        public static Bitmap GetNoiseMap(int width, int height, float frequency, float amplitude, float persistence, int octaves, int seed, float power = 0.9f)
        {
            // Create a new bitmap for storing the noise map  
            Bitmap result = new Bitmap(width, height);
            // Generate initial noise values based on the given seed  
            float[,] noise = GenerateNoise(seed, width, height);

            // Iterate through each pixel in the bitmap  
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Get the noise value for the current pixel position  
                    float value = GetValue(x, y, width, height, frequency, amplitude, persistence, octaves, noise);

                    // Normalize the value to be in the range [0, 1]  
                    value = (value * 0.5f) + 0.5f;
                    // Apply the power modifier to adjust distribution  
                    value = (float)Math.Pow(value, power);

                    // Clamp the value to the range [0, 255] for RGB  
                    int rgbValue = Clamp((int)(value * 255), 0, 255);

                    // Set the pixel color based on the computed value  
                    result.SetPixel(x, y, Color.FromArgb(rgbValue, rgbValue, rgbValue));
                }
            }

            // Apply Gaussian blur to the resultant noise map for smoothing  
            result = GaussianBlur.ApplyGaussianBlurBitmap(result, 10, 2.6f);

            return result; // Return the generated noise map  
        }

        public static float[,] GetNoiseMatrix(int width, int height, float frequency, float amplitude, float persistence, int octaves, int seed, float power = 0.9f)
        {
            // Create a 2D array to store noise values  
            float[,] result = new float[width, height];
            // Generate initial noise values based on the given seed  
            float[,] noise = GenerateNoise(seed, width, height);

            // Iterate through each pixel to compute noise values  
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Get the noise value for the current pixel position  
                    float value = GetValue(x, y, width, height, frequency, amplitude, persistence, octaves, noise);
                    // Normalize and apply power modifier to the value  
                    value = (value * 0.5f) + 0.5f;
                    value = (float)Math.Pow(value, power);

                    // Store the computed value in the result matrix  
                    result[x, y] = value;
                }
            }

            // Apply Gaussian blur to the noise matrix for smoothing  
            result = GaussianBlur.ApplyGaussianBlurMatrix(result, 10, 2.6f);

            return result; // Return the noise matrix  
        }

        public static float GetValue(int x, int y, int width, int height, float frequency, float amplitude, float persistence, int octaves, float[,] noise)
        {
            float finalValue = 0.0f; // Initialize final value  

            // Compute the contributions from each octave  
            for (int i = 0; i < octaves; i++)
            {
                finalValue += CubicInterpolateNoise(x * frequency, y * frequency, width, height, noise) * amplitude;
                frequency *= 2.0f; // Increase frequency for the next octave  
                amplitude *= persistence; // Decrease amplitude for smoother results  
            }

            // Clamp the final value to the range [-1, 1]  
            return Clamp(finalValue, -1.0f, 1.0f);
        }

        public static float CubicInterpolateNoise(float x, float y, int width, int height, float[,] noise)
        {
            // Determine integer coordinates for surrounding pixels  
            int x0 = (int)x;
            int y0 = (int)y;

            // Calculate fractional parts for interpolation  
            float fracX = x - x0;
            float fracY = y - y0;

            // Prepare an array to hold noise values from surrounding pixels  
            float[,] values = new float[4, 4];

            // Collect noise values from the surrounding grid  
            for (int i = -1; i <= 2; i++)
            {
                for (int j = -1; j <= 2; j++)
                {
                    // Wrap around the coordinates to ensure continuity  
                    int xi = (x0 + i + width) % width;
                    int yi = (y0 + j + height) % height;

                    // Store the noise value in the array  
                    values[i + 1, j + 1] = noise[xi, yi];
                }
            }

            // Interpolate in the x-direction  
            float[] xInterpolated = new float[4];
            for (int i = 0; i < 4; i++)
                xInterpolated[i] = CubicInterpolate(values[i, 0], values[i, 1], values[i, 2], values[i, 3], fracY);

            // Interpolate the result in the y-direction  
            return CubicInterpolate(xInterpolated[0], xInterpolated[1], xInterpolated[2], xInterpolated[3], fracX);
        }

        public static float CubicInterpolate(float a, float b, float c, float d, float t)
        {
            // Calculate the cubic interpolation based on four surrounding values and a time factor  
            float p = (d - c) - (a - b);
            return p * t * t * t + ((a - b) - p) * t * t + (c - a) * t + b;
        }

        public static float[,] GenerateNoise(int seed, int width, int height)
        {
            // Create a 2D array to store noise values  
            float[,] noise = new float[width, height];
            Random random = new Random(seed); // Create a random number generator with a specific seed  

            // Populate the noise array with random values between -1 and 1  
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noise[x, y] = (float)random.NextDouble() * 2 - 1;
                }
            }

            return noise; // Return the generated noise array  
        }

        // Clamp an integer value to a specified range  
        public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        // Clamp a float value to a specified range  
        public static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}