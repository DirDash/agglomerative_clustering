using System;
using System.Collections.Generic;
using AgglomerativeСlustering.Clustering.Metrics;

namespace AgglomerativeСlustering.Clustering
{
    public class ClusterSystem
    {
        public Dictionary<int, Cluster> Clusters { get; private set; }
        public Dictionary<string, double> Distances { get; private set; }

        public ClusterSystem(List<Cluster> clusters)
        {
            Clusters = new Dictionary<int, Cluster>();
            foreach (var cluster in clusters)
            {
                Clusters.Add(cluster.Id, cluster);
            }

            Distances = new Dictionary<string, double>();
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    Distances.Add(i + "to" + j, EuclideanMetric.GetDistance(clusters[i].Objects[0], clusters[j].Objects[0]));
                }
            }
        }
    }
}