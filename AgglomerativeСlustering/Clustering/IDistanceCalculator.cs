namespace AgglomerativeСlustering.Clustering
{
    public interface IDistanceCalculator
    {
        double GetDistance(int xCount, int yCount, int zCount, double xzDistance, double yzDistance, double xyDistance);
    }
}