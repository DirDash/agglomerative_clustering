using System;
using System.Collections.Generic;

namespace AgglomerativeСlustering.Clustering
{
    public interface IClusterizator
    {
        IDistanceCalculator ClusterDistanceCalculator { get; set; }

        void Clusterize(List<ResearchObject> researchObjects);
        List<Cluster> GetClusters(int clusterAmount);
    }
}