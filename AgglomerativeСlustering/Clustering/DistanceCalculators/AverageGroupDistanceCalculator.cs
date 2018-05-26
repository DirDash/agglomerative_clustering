namespace AgglomerativeСlustering.Clustering.DistanceCalculators
{
    public class AverageGroupDistanceCalculator : IDistanceCalculator
    {
        public double GetDistance(int xCount, int yCount, int zCount, double xzDistance, double yzDistance, double xyDistance)
        {
            double alphaX = (double)xCount / (xCount + yCount);
            double alphaY = (double)yCount / (xCount + yCount);
            double beta = 0;
            double gamma = 0;
            return ClusterDistanceCalculator.GetLanceWilliamsDistance(xzDistance, yzDistance, xyDistance, alphaX, alphaY, beta, gamma);
        }

        public override string ToString()
        {
            return "среднее групповое расстояние";
        }
    }
}