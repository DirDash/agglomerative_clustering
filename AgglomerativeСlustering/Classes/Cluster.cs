using System;
using System.Collections.Generic;

namespace AgglomerativeСlustering.Classes
{
    public class Cluster : ICloneable
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

        public object Clone()
        {
            Cluster copy = new Cluster(Id, Color);
            foreach (var obj in Objects)
                copy.Objects.Add((ResearchObject)obj.Clone());
            return copy;
        }
    }
}