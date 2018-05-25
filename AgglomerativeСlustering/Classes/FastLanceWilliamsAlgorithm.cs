using System;
using System.Collections.Generic;
using System.Linq;

namespace AgglomerativeСlustering.Classes
{
    public class FastLanceWilliamsAlgorithm
    {
        private ClusterSystem _clusterSystem;
        public IDistanceCalculator ClusterDistanceCalculator { get; set; }
        private Cluster _recepientCluster;
        private Cluster _donorCluster;
        private double _gamma;
        private int _n1;
        private int _n2;
        private Dictionary<string, double> _closeArea;

        public Dendrogram Clusterize(ClusterSystem clusterSystem, int n1 = 250, int n2 = 50)
        {
            _clusterSystem = clusterSystem;
            _n1 = n1;
            _n2 = n2;
            var dendrogram = new Dendrogram();

            foreach (var cluster in _clusterSystem.Clusters.Values)
                dendrogram.AddNode(cluster);

            CalculateCloseArea();

            while (_clusterSystem.Clusters.Count > 1)
            {
                if (_closeArea.Count == 0)
                    CalculateCloseArea();

                var nearestClustersIds = GetNearestClustersIds();
                _recepientCluster = _clusterSystem.Clusters[nearestClustersIds.Item1];
                _donorCluster = _clusterSystem.Clusters[nearestClustersIds.Item2];

                dendrogram.AddNode(_recepientCluster, _donorCluster);

                RecalculateClustersDistances();

                MergeClusters();

                RemoveDonorCluster();
            }

            return dendrogram;
        }

        private void CalculateCloseArea()
        {
            if (_clusterSystem.Clusters.Count <= _n1)
            {
                _closeArea = _clusterSystem.Distances;
            }
            else
            {
                FindNextGamma();
                _closeArea = new Dictionary<string, double>();
                foreach (var distance in _clusterSystem.Distances)
                    if (distance.Value <= _gamma)
                        _closeArea.Add(distance.Key, distance.Value);
            }
        }

        private void FindNextGamma()
        {
            var random = new Random();
            var allDistances = _clusterSystem.Distances.Values.ToList();

            var randomDistances = new List<double>();
            for (int i = 0; i < _n2 && i < allDistances.Count; i++)
                randomDistances.Add(allDistances[random.Next(0, allDistances.Count)]);
            if (randomDistances.Count == 0)
                return;

            double minDistance = randomDistances[0];
            for (int i = 1; i < randomDistances.Count; i++)
                if (randomDistances[i] < minDistance)
                    minDistance = randomDistances[i];

            _gamma = minDistance;
        }

        private Tuple<int, int> GetNearestClustersIds()
        {
            if (_clusterSystem.Clusters.Count == 2)
            {
                var clusters = _clusterSystem.Clusters.Values.ToList();
                return new Tuple<int, int>(clusters[0].Id, clusters[1].Id);
            }

            double minDistance = 0;
            foreach(var distance in _closeArea.Values)
            {
                minDistance = distance;
                break;
            }
            var firstId = -1;
            var secondId = -1;
            foreach (var distance in _closeArea)
            {
                if (distance.Value <= minDistance)
                {
                    var ids = distance.Key.Split(new string[] { "to" }, StringSplitOptions.RemoveEmptyEntries);
                    minDistance = distance.Value;
                    firstId = int.Parse(ids[0]);
                    secondId = int.Parse(ids[1]);                    
                }
            }

            return new Tuple<int, int>(firstId, secondId);
        }

        private void MergeClusters()
        {
            foreach (var obj in _donorCluster.Objects)
                _recepientCluster.Objects.Add(obj);
            _donorCluster.Objects.Clear();
        }

        private void RecalculateClustersDistances()
        {
            foreach (var cluster in _clusterSystem.Clusters)
            {
                if (cluster.Key == _recepientCluster.Id || cluster.Key == _donorCluster.Id)
                    continue;

                string xyKey = "";
                if (_recepientCluster.Id < _donorCluster.Id)
                    xyKey = _recepientCluster.Id + "to" + _donorCluster.Id;
                else
                    xyKey = _donorCluster.Id + "to" + _recepientCluster.Id;

                string xzKey = "";
                if (_recepientCluster.Id < cluster.Key)
                    xzKey = _recepientCluster.Id + "to" + cluster.Key;
                else
                    xzKey = cluster.Key + "to" + _recepientCluster.Id;

                string yzKey = "";
                if (_donorCluster.Id < cluster.Key)
                    yzKey = _donorCluster.Id + "to" + cluster.Key;
                else
                    yzKey = cluster.Key + "to" + _donorCluster.Id;

                double xyDistance = _clusterSystem.Distances[xyKey];
                double xzDistance = _clusterSystem.Distances[xzKey];
                double yzDistance = _clusterSystem.Distances[yzKey];
                
                double newDistance = ClusterDistanceCalculator.GetDistance(
                    _recepientCluster.Objects.Count,
                    _donorCluster.Objects.Count,
                    cluster.Value.Objects.Count,
                    xzDistance,
                    yzDistance,
                    xyDistance);

                _clusterSystem.Distances[xzKey] = newDistance;

                if (_closeArea.Keys.Contains(xzKey))
                    _closeArea[xzKey] = newDistance;
                else
                    if (newDistance <= _gamma)
                        _closeArea.Add(xzKey, newDistance);
            }
        }

        private void RemoveDonorCluster()
        {
            foreach (var cluster in _clusterSystem.Clusters)
            {
                if (_donorCluster.Id == cluster.Key)
                    continue;

                string distanceKey = "";
                if (_donorCluster.Id < cluster.Key)
                    distanceKey = _donorCluster.Id + "to" + cluster.Key;
                else
                    distanceKey = cluster.Key + "to" + _donorCluster.Id;

                _clusterSystem.Distances.Remove(distanceKey);
                _closeArea.Remove(distanceKey);
            }

            _clusterSystem.Clusters.Remove(_donorCluster.Id);
        }

        public override string ToString()
        {
            return "быстрый алгоритм Ланса-Уильямса";
        }
    }
}