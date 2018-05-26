namespace AgglomerativeСlustering.Clustering.DistanceCalculators
{
    public class FarestNeighborDistanceCalculator : IDistanceCalculator
    {
        public double GetDistance(int xCount, int yCount, int zCount, double xzDistance, double yzDistance, double xyDistance)
        {
            double alphaX = 0.5;
            double alphaY = 0.5;
            double beta = 0;
            double gamma = 0.5;
            return ClusterDistanceCalculator.GetLanceWilliamsDistance(xzDistance, yzDistance, xyDistance, alphaX, alphaY, beta, gamma);
        }

        public override string ToString()
        {
            return "расстояние дальнего соседа";
        }
    }
}