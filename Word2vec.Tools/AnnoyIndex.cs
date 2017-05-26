using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Word2vec.Tools {
    // port of https://github.com/spotify/annoy-java
    // specifically: https://github.com/spotify/annoy-java/blob/master/src/main/java/com/spotify/annoy/ANNIndex.java

    // see also: https://github.com/spotify/annoy
    // specifically: https://github.com/spotify/annoy/blob/master/src/annoylib.h
    // note: new AnnoyIndex<int32_t, float, Angular or Euclidean, Kiss64Random>(self->f); // int32_t is signed
    // template<typename S, typename T, typename Distance, typename Random> class AnnoyIndex

    public enum IndexType {
        ANGULAR, EUCLIDEAN, NONE
    }

    public class ANNIndex {
        private readonly List<long> roots;
        //private MappedByteBuffer annBuf;
        private BinaryReader annBuf;

        private readonly int DIMENSION, MIN_LEAF_SIZE;
        private readonly IndexType INDEX_TYPE;
        private readonly int INDEX_TYPE_OFFSET;

        // size of C structs in bytes (initialized in init)
        private readonly int K_NODE_HEADER_STYLE;
        private readonly int NODE_SIZE;

        private readonly int INT_SIZE = 4;
        private readonly int FLOAT_SIZE = 4;
        //private MemoryMappedFile memoryMappedFile; // RandomAccessFile

        /**
         * Construct and load an Annoy index of a specific type (euclidean / angular).
         *
         * @param dimension dimensionality of tree, e.g. 40
         * @param filename  filename of tree
         * @param indexType type of index (default ANGULAR)
         */
        public ANNIndex(int dimension,
                        String filename,
                        IndexType indexType = IndexType.ANGULAR) { // throws IOException
            DIMENSION = dimension;
            INDEX_TYPE = indexType;
            INDEX_TYPE_OFFSET = (INDEX_TYPE == IndexType.ANGULAR) ? 4 : 8;
            K_NODE_HEADER_STYLE = (INDEX_TYPE == IndexType.ANGULAR) ? 12 : 16;
            // we can store up to MIN_LEAF_SIZE children in leaf nodes (we put
            // them where the separating plane normally goes)
            this.MIN_LEAF_SIZE = DIMENSION + 2;
            this.NODE_SIZE = K_NODE_HEADER_STYLE + FLOAT_SIZE * DIMENSION;

            roots = new List<long>();
            load(filename);
        }

        private void load(String filename) { // throws IOException
            //memoryMappedFile = new RandomAccessFile(filename, "r");
            //memoryMappedFile = MemoryMappedFile.CreateFromFile(filename, System.IO.FileMode.Open);
            //memoryMappedFile = FileSystem.
            
            FileInfo info = new FileInfo(filename);
            long fileSize = info.Length; // (int)memoryMappedFile.length();
            BinaryReader binaryStream = new BinaryReader(File.OpenRead(filename)); // little endian by default
            annBuf = binaryStream;

            // Java version only supports indexes <4GB as a result of ByteBuffer using an int index

            /*
            annBuf = memoryMappedFile.getChannel().map(FileChannel.MapMode.READ_ONLY, 0, fileSize);
            annBuf.order(ByteOrder.LITTLE_ENDIAN);
            */
            int m = -1;
            for (long i = fileSize - NODE_SIZE; i >= 0; i -= NODE_SIZE) {
                int k = GetInt(i); // node[i].n_descendants

                if (m == -1 || k == m) {
                    roots.Add(i);
                    m = k;
                } else {
                    break;
                }
            }
        }

        public int GetInt(long offset) {
            lock (this) {
                annBuf.BaseStream.Position = offset;
                return annBuf.ReadInt32();
            }
        }

        public uint GetUInt(long offset) {
            lock (this) {
                annBuf.BaseStream.Position = offset;
                return annBuf.ReadUInt32();
            }
        }

        public float GetFloat(long offset) {
            lock (this) {
                annBuf.BaseStream.Position = offset;
                return annBuf.ReadSingle();
            }
        }

        //@Override
        public void getNodeVector(long nodeOffset, float[] v) {
            for (int i = 0; i < DIMENSION; i++) {
                v[i] = GetFloat(nodeOffset + K_NODE_HEADER_STYLE + i * FLOAT_SIZE);
            }
        }

        //@Override
        public void getItemVector(long itemIndex, float[] v) {
            getNodeVector(itemIndex * NODE_SIZE, v);
        }

        private float getNodeBias(long nodeOffset) { // euclidean-only
            return GetFloat(nodeOffset + 4);
        }

        public float[] getItemVector(long itemIndex) {
            return getNodeVector(itemIndex * NODE_SIZE);
        }

        public float[] getNodeVector(long nodeOffset) {
            float[] v = new float[DIMENSION];
            getNodeVector(nodeOffset, v);
            return v;
        }

        private static float norm(float[] u) {
            float n = 0;
            foreach (float x in u)
                n += x * x;
            return (float)Math.Sqrt(n);
        }

        private static float euclideanDistance(float[] u, float[] v) {
            float[] diff = new float[u.Length];
            for (int i = 0; i < u.Length; i++)
                diff[i] = u[i] - v[i];
            return norm(diff);
        }

        public static float cosineMargin(float[] u, float[] v) {
            double d = 0;
            for (int i = 0; i < u.Length; i++)
                d += u[i] * v[i];
            return (float)(d / (norm(u) * norm(v)));
        }

        public static float euclideanMargin(float[] u,
                                            float[] v,
                                            float bias) {
            float d = bias;
            for (int i = 0; i < u.Length; i++)
                d += u[i] * v[i];
            return d;
        }

        //@Override
        public void close() {
            //memoryMappedFile.close();
            annBuf.BaseStream.Close();
            annBuf.Close();
        }
        
        private static bool isZeroVec(float[] v) {
            for (int i = 0; i < v.Length; i++)
                if (v[i] != 0)
                    return false;
            return true;
        }

        //public Tuple<int, float>[] getNearestDistances(float[] queryVector, int nResults) {
        //    return null; //TODO
        //}

        //@Override
        public int[] getNearest(float[] queryVector, int nResults, int limitTrees = -1) {
            // limitTrees: if > 0 then only search that many trees instead of all of them

            var reverseComparer = Comparer<float>.Create((x,y) => y.CompareTo(x)); // bigger to top

            SimplePriorityQueue <long> pq = new SimplePriorityQueue<long>(reverseComparer);
            // PriorityQueue size: roots.Count() * FLOAT_SIZE);
            const float kMaxPriority = 1e30f;
            IEnumerable<long> useRoots = roots;
            if (limitTrees > 0 && limitTrees < roots.Count()) {
                useRoots = roots.Take(limitTrees);
            }

            foreach (long r in useRoots) {
                pq.Enqueue(r, kMaxPriority); // add(new PQEntry(kMaxPriority, r));
            }

            HashSet<long> nearestNeighbors = new HashSet<long>();
            while (nearestNeighbors.Count() < useRoots.Count() * nResults && pq.Count != 0) {
                long topNodeOffset = pq.Dequeue(); //  top; //.nodeOffset;
                int nDescendants = GetInt(topNodeOffset);
                float[] v = getNodeVector(topNodeOffset);
                if (nDescendants == 1) {  // n_descendants
                                          // (from Java) FIXME: does this ever happen?
                    if (isZeroVec(v))
                        continue;

                    nearestNeighbors.Add(topNodeOffset / NODE_SIZE);

                } else if (nDescendants <= MIN_LEAF_SIZE) {

                    for (int i = 0; i < nDescendants; i++) {
                        int j = GetInt(topNodeOffset + INDEX_TYPE_OFFSET + i * INT_SIZE);
                        if (isZeroVec(getNodeVector(j * NODE_SIZE)))
                            continue;
                        nearestNeighbors.Add(j);
                    }

                } else {

                    float margin = (INDEX_TYPE == IndexType.ANGULAR) ?
                            cosineMargin(v, queryVector) :
                            euclideanMargin(v, queryVector, getNodeBias(topNodeOffset));
                    long childrenMemOffset = topNodeOffset + INDEX_TYPE_OFFSET;
                    long lChild = NODE_SIZE * (long)GetUInt(childrenMemOffset);
                    long rChild = NODE_SIZE * (long)GetUInt(childrenMemOffset + 4);
                    pq.Enqueue(lChild, -margin); 
                    pq.Enqueue(rChild, margin);
                }
            }

            //SimplePriorityQueue<int> sortedNNs = new SimplePriorityQueue<int>(); // reverseComparer
            SimplePriorityQueue<int> sortedNNs = new SimplePriorityQueue<int>(reverseComparer); // reverseComparer
            //List<PQEntry> sortedNNs = new List<PQEntry>();

            foreach (int nn in nearestNeighbors) {
                float[] v = getItemVector(nn);
                if (isZeroVec(v)) continue;
                float priority = (INDEX_TYPE == IndexType.ANGULAR) ?
                                    cosineMargin(v, queryVector) :
                                    -euclideanDistance(v, queryVector);
                sortedNNs.Enqueue(nn, priority);
            }

            return sortedNNs.Take(nResults).ToArray();

            /*
            // longer verison of above
            List<int> results = new List<int>();
            for (int i = 0; i < nResults; i++) {
                results.Add(sortedNNs.Dequeue());
            }
            return results.ToArray();
            */
        }


        /* a test query program.
        *
        * @param args tree filename, dimension, indextype ("angular" or
        *             "euclidean" and query item id.
        */
        public static void main(String[] args) {

            String indexPath = args[0];           // 0
            int dimension = int.Parse(args[1]);   // 1
            IndexType indexType = IndexType.NONE; // 2
            if (args[2].ToLowerInvariant() == "angular") {
                indexType = IndexType.ANGULAR;
            } else if (args[2].ToLowerInvariant() == "euclidean") { 
                indexType = IndexType.EUCLIDEAN;
            } else {
                //throw new RuntimeException("wrong index type specified");
                Console.WriteLine("defaulting to ANGULAR: angular/euclidean not specified");
                indexType = IndexType.ANGULAR;
            }
            int queryItem = int.Parse(args[3]);  // 3

            ANNIndex annIndex = new ANNIndex(dimension, indexPath, indexType);

            float[] u = annIndex.getItemVector(queryItem);
            Console.WriteLine("vector[{0}]: ", queryItem);
            foreach (float x in u) {
                Console.Write("{0:2.2f} ", x);
            }
            Console.WriteLine();

            int[] nearestNeighbors = annIndex.getNearest(u, 10);
            foreach (int nn in nearestNeighbors) {
                float[] v = annIndex.getItemVector(nn);
                Console.WriteLine("{0} {1} {2}\n",
                        queryItem, nn,
                          (indexType == IndexType.ANGULAR) ?
                                cosineMargin(u, v) : euclideanDistance(u, v));
            }
        }
    }


}
