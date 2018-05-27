using System;
using System.Collections.Generic;
using AgglomerativeСlustering.Clustering.Colors;

namespace AgglomerativeСlustering.Clustering
{
    public class Cluster
    {
        public int Id { get; private set; }
        public RGBColor Color;
        public List<ResearchObject> Objects { get; private set; }

        public Cluster(int id, RGBColor color)
        {
            Id = id;
            Color = color;
            Objects = new List<ResearchObject>();
        }

        public Cluster(int id, RGBColor color, List<ResearchObject> objects)
        {
            Id = id;
            Color = color;
            Objects = objects;
        }
    }
}