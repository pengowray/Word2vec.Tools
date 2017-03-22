using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Word2vec.Tools
{
    /// <summary>
    /// known w2v vectors
    /// </summary>
    public class Vocabulary 
    {
        /// <summary>
        /// All known words w2v representations
        /// </summary>
        public readonly WordRepresentation[] Words;

        /// <summary>
        /// w2v words vectors dimensions count
        /// </summary>
        public int VectorDimensionsCount { get; set; }

        /// <summary>
        /// false if vectors are not normalized or unknown normalization
        /// true to enable optimizations (NYI)
        /// true to cause centroids to be normalized too
        /// </summary>
        public bool isNormalized;

        readonly Dictionary<string, WordRepresentation> _dictionary;

        public Cluster[] Clusters = null;
        //public Dictionary<int, CentroidRepresentation> _clusterDictionary; // hopefully redundant

        /// <summary>
        /// Number of entries the source file stated there would be. Should match Words.Length if file was read successfully.
        /// </summary>
        public readonly int StatedVocabularySize = 0;

        public Vocabulary(IEnumerable<WordRepresentation> representations, int vectorDimensionsCount, int StatedVocabularySize = 0)
        {
            _dictionary = new Dictionary<string, WordRepresentation>();
            this.VectorDimensionsCount = vectorDimensionsCount;

            foreach (var representation in representations)
            {
                if (representation.NumericVector.Count() != vectorDimensionsCount)
                    throw new ArgumentException("representations.Vector.Length");

                if (string.IsNullOrWhiteSpace(representation.Word))
                    continue; //TODO: track these error entries

                if (_dictionary.ContainsKey(representation.Word))
                    continue; //TODO: track these error entries

                _dictionary.Add(representation.Word, representation);
            }
            Words = _dictionary.Values.ToArray();
            this.StatedVocabularySize = StatedVocabularySize;
        }
        /// <summary>
        /// Returns word2vec word vector if it exists.
        /// Returns null otherwise
        /// </summary>
        public WordRepresentation GetRepresentationOrNullFor(string word)
        {
            if (ContainsWord(word))
                return GetRepresentationFor(word);
            else
                return null;
        }
        /// <summary>
        /// Returns word2vec word vector if it exists.
        /// Throw otherwise
        /// </summary>
        public WordRepresentation GetRepresentationFor(string word)
        {
            return _dictionary[word];
        }
        public WordRepresentation this[string word] { get { return GetRepresentationFor(word); } }
        public bool ContainsWord(string word)
        {
            return _dictionary.ContainsKey(word);
        }

        /// <summary>
        /// if word exists - returns "count" of best fits for target word
        /// otherwise - returns empty array
        /// </summary>
        public WordDistance[] Distance(string word, int count, int onlyFromTop = int.MaxValue)
        {
            if (!this.ContainsWord(word))
                return new WordDistance[0];

            return Distance(this[word], count, onlyFromTop);
        }

        public WordDistance[] SimpleAngleDistance(Representation representation, int maxCount, int onlyFromTop = int.MaxValue) {
            return representation.GetSimpleAngleClosestFrom(Words.Take(onlyFromTop).Where(v => v != representation), maxCount);
        }
        /// <summary>
        /// returns "count" of closest words for target representation, but only from the first "onlyFromTop" entries in the vocab (which is typically sorted by occurrences in the corpus)
        /// </summary>
        public WordDistance[] Distance(Representation representation, int maxCount, int onlyFromTop = int.MaxValue) {
            return representation.GetClosestFrom(Words.Take(onlyFromTop).Where(v => v != representation), maxCount);
        }

        public IEnumerable<ClusterDistance> MinClusterDistaces(Representation representation) {
            return Clusters.Select(toCluster => new ClusterDistance(representation, toCluster, representation.GetSimpleAngleTo(toCluster.Centroid))).OrderBy(dis => dis.MinDistance);
        }


        /// <summary>
        /// If wordA is wordB, then wordC is...
        /// If all words exist - returns "count" of best fits for the result
        /// otherwise - returns empty array
        /// </summary>
        public WordDistance[] Analogy(string wordA, string wordB, string wordC, int count)
        {
            if (!ContainsWord(wordA) || !ContainsWord(wordB) || !ContainsWord(wordC))
                return new WordDistance[0];
            else
                return Analogy(GetRepresentationFor(wordA), GetRepresentationFor(wordB), GetRepresentationFor(wordC), count);
        }
        /// <summary>
        /// wordA is to be wordB, as wordC is to...
        /// Returns "count" of best fits for the result
        /// </summary>
        public WordDistance[] Analogy(Representation wordA, Representation wordB, Representation wordC, int count) {
            var cummulative = wordB.Substract(wordA).Add(wordC);
            //return cummulative.GetClosestFrom(Words.Where(t => t != wordA && t != wordB && t != wordC), count);

            //TODO: don't bother filtering. leave that for the client
            var dist = Distance(cummulative, count + 3);
            return dist.Where(t => t.Representation != wordA && t.Representation != wordB && t.Representation != wordC).Take(count).ToArray();
            //return Distance(cummulative, count);
        }

        public Cluster NearestCluster(Representation rep) {
            if (Clusters == null)
                return null;

            var nearest = rep.NearestCluster();
            if (nearest != null && nearest.Parent == this)
                return nearest;
            
            return DoNearestCluster(rep);
        }

        public Cluster NearestCluster(WordRepresentation word) {
            if (Clusters == null)
                return null;

            if (word.cluster != null && word.cluster.Parent == this)
                return word.cluster;

            return DoNearestCluster(word);
        }

        protected Cluster DoNearestCluster(Representation word) {
            return Clusters.OrderBy(c => c.Centroid.GetCosineDistanceTo(word)).First();
        }



        public void InitializeClusters() {

            int count = Words.Count();

            int k = (int)Math.Sqrt(count / 2);

            var rand = new Random(); // TODO: configurable

            // pick k random items:
            HashSet<int> randomCentIndexes = new HashSet<int>();
            while (randomCentIndexes.Count < k) {
                randomCentIndexes.Add(rand.Next(count));
            }

            //Centroids = new WordRepresentation[]
            //_centroidDictionary = new Dictionary<int, CentroidRepresentation>();
            List<Cluster> clusterList = new List<Cluster>();
            int cIndex = 0;
            foreach (var index in randomCentIndexes.OrderBy(i => i)) {
                var centroid = new CentroidRepresentation(cIndex, Words[index].NumericVector);
                //_centroidDictionary[cIndex] = centroid;
                clusterList.Add(new Cluster(this, centroid, cIndex));
                cIndex++;
            }

            //Centroids = _centroidDictionary.Values.ToArray();
            Clusters = clusterList.ToArray();
            CalculateCentroidDistances();
        }

        public void KmeansIterateClusters() {
            //TODO
            
            // average words distances

            // move clusters

            CalculateCentroidDistances();
        }

        public void CalculateCentroidDistances() {

            // set WordRank property of each word (TODO: should be done in Word2VecBinaryReader and Word2VecTextReader)
            int rank = 0;
            foreach (var word in Words) {
                word.Rank = rank;
                rank++;
            }

            // set Cluster property of each Word
            foreach (var word in Words) {
                // find closest cluster
                var closestCluster = DoNearestCluster(word); // use "Do" version of function to avoid optimization (which would skip the calculation)
                //int closestClusterIndex = Centroids.OrderBy(c => c.GetCosineDistanceToWord(word)).First().Index; // was CalcClosestCentroid(word);
                //_WordClusterMap[word.Word] = closestCluster;
                word.cluster = closestCluster;
            }
            
            // set Words and Radius of each Cluster
            foreach (var cluster in Clusters) {
                //_ClusterWords[centroid.Index] = _WordClusterMap.Where(x => x.Value == centroid.Index).Select(x => x.Key).ToArray(); // just words
                cluster.Words = Words.Where(w => w.cluster == cluster).ToArray();
                cluster.Radius = cluster.Words.Max(w => w.GetSimpleAngleTo(cluster.Centroid));
            }

            // set nearest clusters
            //foreach (var fromCluster in Clusters) {
                ////Clusters[] Nearest = Clusters.OrderBy(c => cluster.Centroid.GetCosineDistanceTo(c.Centroid)).ToArray();
                //fromCluster.Nearest = Clusters.Select(toCluster => fromCluster.GetSimpleAngleTo(toCluster)).OrderBy(dis => dis.MinDistance).ToArray();
            //}

            // create a search tree so all clusters can efficiently find their nearest words
            //foreach (var cluster in Clusters) { 
                // clusters the next closest word could possibly be found in:
                //var candidates = cluster.Nearest.TakeWhile(c => c.Nearest)

            //}

        }

        /*

//TODO: Make a reusable analogy-template. 
//Also remember ~50 neighbouring words of all invoked terms and don't include those
//Maybe also ignore terms that come up with the opposite analogy (black-white, white-black) ?
//Also check that vectors are normalized and if that's important

public AnalogyTemplate TemplatizeAnalogy(string wordA, string wordB, int excludeCount = 50) {
   if (!this.ContainsWord(wordA) || !this.ContainsWord(wordB))
       return null;

   return AnalogyStructure(GetRepresentationFor(wordA), GetRepresentationFor(wordB), excludeCount);
}

public AnalogyTemplate TemplatizeAnalogy(Representation wordA, Representation wordB, int excludeCount = 50) {
   var distance = wordB.Substract(wordA);
}
*/
    }
}