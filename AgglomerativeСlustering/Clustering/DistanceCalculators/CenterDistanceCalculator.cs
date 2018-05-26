namespace AgglomerativeСlustering.Clustering.DistanceCalculators
{
    public class CenterDistanceCalculator : IDistanceCalculator
    {
        public double GetDistance(int xCount, int yCount, int zCount, double xzDistance, double yzDistance, double xyDistance)
        {
            double alphaX = (double)xCount / (xCount + yCount);
            double alphaY = (double)yCount / (xCount + yCount);
            double beta = -1 * alphaX * alphaY;
            double gamma = 0;
            return ClusterDistanceCalculator.GetLanceWilliamsDistance(xzDistance, yzDistance, xyDistance, alphaX, alphaY, beta, gamma);
        }

        public override string ToString()
        {
            return "расстояние между центрами";
        }
    }
}