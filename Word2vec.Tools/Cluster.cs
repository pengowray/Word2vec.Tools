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

        public double Radius; // max distance to a word within this cluster [angular cosine distance]
        public WordRepresentation[] Words;
        //public string[] Words;
        //public ClusterDistance[] Nearest; // nearest clusters, sorted by cluster with closest possible words
        public WordDistance[] OtherClusterWords; // nearest words in other clusters (one word per cluster)

        public int Index = -1; // a number identifying this cluster. Its position in Vocabulary.Clusters.

        //public Cluster(IEnumerable<WordRepresentation> representations, int vectorDimensionsCount)
        public Cluster(Vocabulary Parent, CentroidRepresentation Centroid, int Index) 
        {
            this.Parent = Parent;
            this.Centroid = Centroid;
            this.Index = Index;
        }

        public ClusterDistance GetSimpleAngleTo(Cluster toCluster) {
            //var distance = Math.Acos(1 - Distance.Cosine(Centroid.NumericVector.ToArray(), toCluster.Centroid.NumericVector.ToArray()));
            var distance = Math.Acos(Centroid.NumericVector.DotProduct(toCluster.Centroid.NumericVector));

            return new ClusterDistance(this, toCluster, distance);
        }

    }
}
