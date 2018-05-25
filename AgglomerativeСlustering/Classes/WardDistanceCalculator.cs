namespace AgglomerativeСlustering.Classes
{
    public class WardDistanceCalculator : IDistanceCalculator
    {
        public double GetDistance(int xCount, int yCount, int zCount, double xzDistance, double yzDistance, double xyDistance)
        {
            double alphaX = (double)(xCount + zCount) / (xCount + yCount + zCount);
            double alphaY = (double)(yCount + zCount) / (xCount + yCount + zCount);
            double beta = (double)-1 * zCount / (xCount + yCount + zCount);
            double gamma = 0;
            return ClustersDistanceCalculator.GetLanceWilliamsDistance(xzDistance, yzDistance, xyDistance, alphaX, alphaY, beta, gamma);
        }

        public override string ToString()
        {
            return "расстоние Уорда";
        }
    }
}