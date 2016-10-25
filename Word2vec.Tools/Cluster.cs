using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Word2vec.Tools {
    public class Cluster {
        public Vocabulary Parent;
        public CentroidRepresentation Centroid;

        public double Radius; // max distance to a word within this cluster
        public WordRepresentation[] Words;
        //public string[] Words;
        public ClusterDistance[] Nearest; // nearest clusters, sorted by cluster with closest possible words
        public WordDistance[] OtherClusterWords; // nearest words in other clusters (one word per cluster)

        //public Cluster(IEnumerable<WordRepresentation> representations, int vectorDimensionsCount)
        public Cluster(Vocabulary Parent, CentroidRepresentation Centroid) 
        {
            this.Parent = Parent;
            this.Centroid = Centroid;
        }

        public ClusterDistance GetCosineDistanceToCluster(Cluster toCluster) {
            var distance = Distance.Cosine(Centroid.NumericVector.ToArray(), toCluster.Centroid.NumericVector.ToArray());
            return new ClusterDistance(this, toCluster, distance);
        }

    }
}
