using System;
using System.Collections.Generic;
using System.Linq;
using AgglomerativeСlustering.Clustering.Colors;

namespace AgglomerativeСlustering.Clustering.Clusterizators
{
    public class FastLanceWilliamsAlgorithmClusterizator : IClusterizator
    {
        private static Random _random = new Random();

        public IDistanceCalculator ClusterDistanceCalculator { get; set; }
        private ClusterSystem _clusterSystem;
        private Cluster _recepientCluster;
        private Cluster _donorCluster;
        private Dendrogram _dendrogram;

        private Dictionary<string, double> _searchArea;
        private double _delta;
        private int _n1;
        private int _n2;

        public FastLanceWilliamsAlgorithmClusterizator(int n1 = 50, int n2 = 20)
        {
            _n1 = n1;
            _n2 = n2;
        }        

        public override string ToString()
        {
            return "быстрый алгоритм Ланса-Уильямса";
        }

        public void Clusterize(List<ResearchObject> researchObjects)
        {
            _clusterSystem = new ClusterSystem(InitialClustering(researchObjects));

            _dendrogram = new Dendrogram();
            foreach (var cluster in _clusterSystem.Clusters.Values)
            {
                _dendrogram.AddNode(cluster);
            }

            CalculateSearchArea();

            while (_clusterSystem.Clusters.Count > 1)
            {
                PerformClusteringStep();
            }
        }

        public List<Cluster> GetClusters(int clusterAmount)
        {
            return _dendrogram.GetClusters(clusterAmount);            
        }

        private List<Cluster> InitialClustering(List<ResearchObject> objects)
        {
            var clusters = new List<Cluster>();
            int lastId = 0;
            foreach (var obj in objects)
            {
                var cluster = new Cluster(lastId, RGBColorCreator.GetRandomColor());
                lastId++;
                cluster.Objects.Add((ResearchObject)obj.Clone());
                clusters.Add(cluster);
            }

            return clusters;
        }

        private void PerformClusteringStep()
        {
            if (_clusterSystem.Clusters.Count <= _n1 || _searchArea.Count <= 1)
            {
                CalculateSearchArea();
            }

            var nearestClustersIndexes = GetNearestClustersIndexes();
            if (_clusterSystem.Clusters[nearestClustersIndexes.Item1].Objects.Count >= _clusterSystem.Clusters[nearestClustersIndexes.Item2].Objects.Count)
            {
                _recepientCluster = _clusterSystem.Clusters[nearestClustersIndexes.Item1];
                _donorCluster = _clusterSystem.Clusters[nearestClustersIndexes.Item2];
            }
            else
            {
                _recepientCluster = _clusterSystem.Clusters[nearestClustersIndexes.Item2];
                _donorCluster = _clusterSystem.Clusters[nearestClustersIndexes.Item1];
            }

            _dendrogram.AddNode(_recepientCluster, _donorCluster);

            RecalculateClusterDistances();

            MergeClusters();

            RemoveDonorCluster();
        }

        private void CalculateSearchArea()
        {
            if (_clusterSystem.Clusters.Count <= _n1)
            {
                _searchArea = _clusterSystem.Distances;
            }
            else
            {
                FindNextDelta();
                _searchArea = new Dictionary<string, double>();
                foreach (var distance in _clusterSystem.Distances)
                    if (distance.Value <= _delta)
                        _searchArea.Add(distance.Key, distance.Value);
            }
        }

        private void FindNextDelta()
        {
            var allDistances = _clusterSystem.Distances.Values.ToList();

            var randomDistances = new List<double>();
            for (int i = 0; i < _n2 && i < allDistances.Count; i++)
            {
                randomDistances.Add(allDistances[_random.Next(0, allDistances.Count)]);
            }

            if (randomDistances.Count == 0)
                return;

            double minDistance = randomDistances[0];
            for (int i = 1; i < randomDistances.Count; i++)
            {
                if (randomDistances[i] < minDistance)
                {
                    minDistance = randomDistances[i];
                }
            }

            _delta = minDistance + 0.1;
        }

        private Tuple<int, int> GetNearestClustersIndexes()
        {
            if (_clusterSystem.Clusters.Count == 2)
            {
                var clusters = _clusterSystem.Clusters.Values.ToList();
                return new Tuple<int, int>(clusters[0].Id, clusters[1].Id);
            }

            double minDistance = _searchArea.Values.First() + 1;
            var firstId = -1;
            var secondId = -1;
            foreach (var distance in _searchArea)
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

        private void RecalculateClusterDistances()
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

                if (_searchArea.Keys.Contains(xzKey))
                {
                    if (newDistance <= _delta)
                    {
                        _searchArea[xzKey] = newDistance;
                    }
                    else
                    {
                        if (_clusterSystem.Clusters.Count > _n1)
                        {
                            _searchArea.Remove(xzKey);
                        }
                    }
                }
                else
                {
                    if (newDistance <= _delta)
                    {
                        _searchArea.Add(xzKey, newDistance);
                    }
                }
            }
        }

        private void MergeClusters()
        {
            foreach (var obj in _donorCluster.Objects)
            {
                _recepientCluster.Objects.Add(obj);
            }
            _donorCluster.Objects.Clear();
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
                _searchArea.Remove(distanceKey);
            }

            _clusterSystem.Clusters.Remove(_donorCluster.Id);
        }
    }
}