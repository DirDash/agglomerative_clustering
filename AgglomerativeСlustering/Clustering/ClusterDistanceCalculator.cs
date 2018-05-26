using System;

namespace AgglomerativeСlustering.Clustering
{
    public static class ClusterDistanceCalculator
    {
        public static double GetLanceWilliamsDistance(double xzDistance, double yzDistance, double xyDistance, double alphaX, double alphaY, double beta, double gamma)
        {
            return alphaX * xzDistance + alphaY * yzDistance + beta * xyDistance - gamma * Math.Abs(xzDistance - yzDistance);
        }

        public static double GetAverageClusterDistance(ClusterSystem clusterSystem)
        {
            double totalDistance = 0;
            foreach (var distance in clusterSystem.Distances)
            {
                totalDistance += distance.Value;
            }

            return totalDistance / clusterSystem.Distances.Count;
        }
    }
}