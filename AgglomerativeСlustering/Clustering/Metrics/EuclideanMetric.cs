using System;

namespace AgglomerativeСlustering.Clustering.Metrics
{
    public static class EuclideanMetric
    {
        public static double GetDistance(ResearchObject x, ResearchObject y)
        {
            if (x.Features.Count != y.Features.Count)
                throw new Exception("Objects dimentions don't match!");

            int featuresCount = x.Features.Count;
            double distance = 0;
            for (int i = 0; i < featuresCount; i++)
            {
                distance += Math.Pow((x.Features[i] - y.Features[i]), 2);
            }

            return Math.Sqrt(distance);
        }
    }
}