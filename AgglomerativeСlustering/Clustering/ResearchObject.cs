using System;
using System.Collections.Generic;

namespace AgglomerativeСlustering.Clustering
{
    public class ResearchObject : ICloneable
    {
        public string Mark { get; private set; }
        public List<double> Features { get; private set; }

        public ResearchObject(string mark, List<double> features)
        {
            Mark = mark;

            Features = new List<double>();
            foreach (var feature in features)
                Features.Add(feature);
        }

        public object Clone()
        {
            var features = new List<double>();
            foreach (var feature in Features)
                features.Add(feature);
            return new ResearchObject(Mark, features);
        }
    }
}