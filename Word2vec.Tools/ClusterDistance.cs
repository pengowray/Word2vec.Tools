
namespace Word2vec.Tools
{
    public class ClusterDistance
    {
        public ClusterDistance(Cluster from, Cluster toCluster, double distance)
        {
            //From = from;
            Cluster = toCluster;
            //Distance = distance;
            MinDistance = distance - from.Radius - toCluster.Radius;
        }

        public ClusterDistance(Representation from, Cluster toCluster, double distance) {
            //From = from;
            Cluster = toCluster;
            //Distance = distance;
            MinDistance = distance - toCluster.Radius;

            //var nearestCluster = from.NearestCluster();
            //if (nearestCluster != null) {
              //  var clusterDist = nearestCluster.Centroid.GetSimpleAngleTo(from);
              //  MinDistance = distance - toCluster.Radius;
            //}
        }

        //public readonly Cluster From; // add this back if needed
        public readonly Cluster Cluster;
        //public readonly double Distance; // add this back if needed
        public readonly double MinDistance;

        /// <summary>
        /// minimum possible distance between words in these clusters.
        /// </summary>
        /// <param name="from">the Cluster used to create the distance in the first place.</param>
        /// <returns></returns>
        /*
        public double MinDistance() {
            return Distance - From.Radius - Cluster.Radius;
        }

        // maximum possible distance between words in these clusters
        public double MaxDistance() {
            return Distance + From.Radius + Cluster.Radius;
        }
        */
    }
}
