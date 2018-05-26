using System;
using System.Collections.Generic;
using AgglomerativeСlustering.Clustering.Colors;

namespace AgglomerativeСlustering.Clustering
{
    public class Dendrogram
    {
        private List<DendrogramNode> _nodes;
        private Dictionary<int, DendrogramNode> _nodeMap;
        private DendrogramNode _head;
        private int _clusterNumber;

        public Dendrogram()
        {
            _nodes = new List<DendrogramNode>();
            _nodeMap = new Dictionary<int, DendrogramNode>();
            _head = null;
            _clusterNumber = 0;
        }

        public void AddNode(Cluster cluster)
        {
            var node = new DendrogramNode(cluster.Id, cluster.Color, (ResearchObject)cluster.Objects[0].Clone());
            _nodes.Add(node);
            _nodeMap.Add(cluster.Id, node);
            _head = node;
            _clusterNumber++;
        }

        public void AddNode(Cluster recepientCluster, Cluster donorCluster)
        {
            var node = new DendrogramNode(recepientCluster.Id, recepientCluster.Color, _nodeMap[recepientCluster.Id], _nodeMap[donorCluster.Id], _clusterNumber);
            _clusterNumber--;
            _nodes.Add(node);
            _nodeMap[recepientCluster.Id] = node;
            _head = node;
        }

        public List<Cluster> GetClusters(int clusterAmount)
        {
            var clusters = new List<Cluster>();
            if (clusterAmount <= 1)
            {
                var allObjects = _head.GetObjects();
                var cluster = new Cluster(_head.Id, _head.Color);
                foreach (var obj in allObjects)
                    cluster.Objects.Add(obj);
                clusters.Add(cluster);
                return clusters;
            }
            var queue = new Queue<DendrogramNode>();
            queue.Enqueue(_head);
            
            while (true)
            {
                var node = queue.Dequeue();
                if (node.Level == clusterAmount)
                {
                    queue.Enqueue(node.LeftChild);
                    queue.Enqueue(node.RightChild);
                    break;
                }
                if (node.Level == -1 || node.Level > clusterAmount)
                {
                    queue.Enqueue(node);
                    continue;
                }
                queue.Enqueue(node.LeftChild);
                queue.Enqueue(node.RightChild);
            }
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Level != -1 && node.Level < clusterAmount)
                {
                    queue.Enqueue(node.LeftChild);
                    queue.Enqueue(node.RightChild);
                    continue;
                }
                var cluster = new Cluster(node.Id, node.Color, node.GetObjects());
                clusters.Add(cluster);
            }

            return clusters;
        }
    }

    class DendrogramNode
    {
        public int Id { get; private set; }
        public RGBColor Color { get; private set; }
        public ResearchObject Object { get; private set; }
        public DendrogramNode LeftChild { get; private set; }
        public DendrogramNode RightChild { get; private set; }
        public int Level { get; private set; }

        public DendrogramNode(int id, RGBColor color, ResearchObject researchObject)
        {
            Id = id;
            Color = color;
            Object = researchObject;
            LeftChild = null;
            RightChild = null;
            Level = -1;
        }

        public DendrogramNode(int id, RGBColor color, DendrogramNode leftChild, DendrogramNode rightChild, int level)
        {
            Id = id;
            Color = color;
            LeftChild = leftChild;
            RightChild = rightChild;
            Level = level;
        }

        public List<ResearchObject> GetObjects()
        {
            if (Level == -1)
                return new List<ResearchObject>() { Object };

            var objects = new List<ResearchObject>();
            foreach (var obj in LeftChild.GetObjects())
                objects.Add(obj);
            foreach (var obj in RightChild.GetObjects())
                objects.Add(obj);

            return objects;
        }
    }
}