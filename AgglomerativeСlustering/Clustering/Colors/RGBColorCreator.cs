using System;

namespace AgglomerativeСlustering.Clustering.Colors
{
    public static class RGBColorCreator
    {
        private static Random _random = new Random();
        
        public static RGBColor GetRandomColor()
        {
            byte red = (byte)_random.Next(0, 256);
            byte green = (byte)_random.Next(0, 256);
            byte blue = (byte)_random.Next(0, 256);
            return new RGBColor(red, green, blue);
        }
    }
}