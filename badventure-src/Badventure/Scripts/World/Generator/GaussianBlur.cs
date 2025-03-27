using System;
using System.Drawing;

namespace MapMatrix2d.Generator
{
    public static class GaussianBlur
    {
        /// <summary>  
        /// This method applies Gaussian blur to a matrix.  
        /// A Gaussian blur smooths out rapid changes in pixel values.  
        /// </summary>  
        /// <param name="matrix">The input 2D array of float values representing the image data.</param>  
        /// <param name="radius">The radius of the blur effect, determining the size of the kernel.</param>  
        /// <param name="sigma">The standard deviation of the Gaussian distribution controlling the blur strength.</param>  
        /// <returns>A 2D array of float values representing the blurred image data.</returns>  
        public static float[,] ApplyGaussianBlurMatrix(float[,] matrix, int radius, float sigma)
        {
            int size = radius * 2 + 1; // Calculate the size of the kernel  
            float[,] kernel = CreateGaussianKernel(size, sigma); // Create the Gaussian kernel  

            float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)]; // Initialize output array  

            for (int x = 0; x < matrix.GetLength(0); x++) 
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    float sum = 0F; // Initialize sum for weighted values  
                    float totalWeight = 0F; // Initialize total weight  

                    for (int i = -radius; i <= radius; i++)
                    {
                        for (int j = -radius; j <= radius; j++)
                        {
                            int xi = Clamp(x + i, 0, matrix.GetLength(0) - 1); // Clamp x coordinate  
                            int yj = Clamp(y + j, 0, matrix.GetLength(1) - 1); // Clamp y coordinate  

                            float weight = kernel[i + radius, j + radius]; // Get kernel weight  
                            sum += matrix[xi, yj] * weight; // Calculate weighted sum  
                            totalWeight += weight; // Update total weight  
                        }
                    }

                    result[x, y] = sum / totalWeight; // Normalize by total weight  
                }
            }

            return result; // Return the blurred matrix  
        }

        /// <summary>  
        /// This method applies Gaussian blur to a bitmap image.  
        /// The resulting image is smoother and less detailed.  
        /// </summary>  
        /// <param name="bitmap">The input <see cref="Bitmap"/> object representing the image to be blurred.</param>  
        /// <param name="radius">The radius of the blur effect, determining the size of the kernel.</param>  
        /// <param name="sigma">The standard deviation of the Gaussian distribution controlling the blur strength.</param>  
        /// <returns>A new <see cref="Bitmap"/> object representing the blurred image.</returns>  
        public static Bitmap ApplyGaussianBlurBitmap(Bitmap bitmap, int radius, float sigma)
        {
            int size = radius * 2 + 1; // Calculate the size of the kernel  
            float[,] kernel = CreateGaussianKernel(size, sigma); // Create the Gaussian kernel  

            Bitmap result = new Bitmap(bitmap.Width, bitmap.Height); // Initialize output bitmap  

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)  
                {
                    float r = 0, g = 0, b = 0; // Initialize color sums  

                    for (int i = -radius; i <= radius; i++) 
                    {
                        for (int j = -radius; j <= radius; j++)
                        {
                            int xi = Clamp(x + i, 0, bitmap.Width - 1); // Clamp x coordinate  
                            int yj = Clamp(y + j, 0, bitmap.Height - 1); // Clamp y coordinate  

                            Color pixel = bitmap.GetPixel(xi, yj); // Get pixel color  

                            float weight = kernel[i + radius, j + radius]; // Get kernel weight  

                            r += pixel.R * weight; // Calculate weighted red value  
                            g += pixel.G * weight; // Calculate weighted green value  
                            b += pixel.B * weight; // Calculate weighted blue value  
                        }
                    }

                    result.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b)); // Set pixel color in the result bitmap  
                }
            }

            return result; // Return the blurred bitmap  
        }

        /// <summary>  
        /// This method creates a Gaussian kernel for use in blurring.  
        /// The kernel determines how much neighboring pixels influence the center pixel.  
        /// </summary>  
        /// <param name="size">The size of the kernel, which should be an odd number for symmetry.</param>  
        /// <param name="sigma">The standard deviation that controls the spread of the kernel values.</param>  
        /// <returns>A 2D array of float values representing the Gaussian kernel, normalized to sum to 1.</returns>  
        private static float[,] CreateGaussianKernel(int size, float sigma)
        {
            float[,] kernel = new float[size, size]; // Create a new kernel  
            float sum = 0; // Initialize sum for normalization  

            int radius = size / 2; // Calculate radius from size  

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    float exponent = -(x * x + y * y) / (2 * sigma * sigma); // Calculate exponent  
                    float value = (float)(Math.Exp(exponent) / (2 * Math.PI * sigma * sigma)); // Calculate kernel value  

                    kernel[x + radius, y + radius] = value; // Assign value to kernel  
                    sum += value; // Add to total sum  
                }
            }

            for (int x = 0; x < size; x++) // Normalize the kernel  
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] /= sum; // Normalize each value  
                }
            }

            return kernel; // Return the kernel  
        }

        /// <summary>  
        /// This method clamps an integer value between a minimum and maximum.  
        /// It ensures the value does not go below the minimum or above the maximum.  
        /// </summary>  
        /// <param name="value">The value to be clamped.</param>  
        /// <param name="min">The minimum allowable value.</param>  
        /// <param name="max">The maximum allowable value.</param>  
        /// <returns>The clamped value within the specified range.</returns>  
        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        /// <summary>  
        /// This method clamps a float value between a minimum and maximum.  
        /// It ensures the value does not go below the minimum or above the maximum.  
        /// </summary>  
        /// <param name="value">The float value to be clamped.</param>  
        /// <param name="min">The minimum allowable value.</param>  
        /// <param name="max">The maximum allowable value.</param>  
        /// <returns>The clamped float value within the specified range.</returns>  
        private static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}