using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Word2vec.Tools;
using System.Collections.Generic;

namespace Word2vec.Tools.Tests {
    [TestClass]
    public class ANNIndexTest {
        string dir = @"..\..\resources\";
        bool verboseOutput = true;

        private void testIndex(ANNIndex index, StreamReader reader, bool verbose) {

            while (true) {
                // read in expected results from file (precomputed from c++ version)
                String line = reader.ReadLine();
                if (line == null)
                    break;
                String[] _l = line.Split('\t');
                int queryItemIndex = int.Parse(_l[0]);
                List<int> expectedResults = new List<int>(); // LinkedList
                foreach (String _i in _l[1].Split(','))
                    expectedResults.Add(int.Parse(_i));

                // do the query
                float[] itemVector = index.getItemVector(queryItemIndex);
                int[] retrievedResults = index.getNearest(itemVector, 10);

                if (verbose) {
                    Console.WriteLine("query: {0}", queryItemIndex);
                    for (int i = 0; i < 10; i++)
                        Console.WriteLine("expected {0,6} retrieved {1,6}", // {0:6N} for 6 dp
                                expectedResults[i],
                                retrievedResults[i]);
                }

                // results will not match exactly, but at least 5/10 should overlap
                HashSet<int> totRes = new HashSet<int>(); // TreeSet
                totRes.UnionWith(expectedResults); // totRes.addAll(expectedResults);
                totRes.IntersectWith(retrievedResults);  // totRes.retainAll(retrievedResults);

                bool pass = totRes.Count >= 5; // assert(totRes.size() >= 5);
                if (!pass || verbose) {
                    Console.WriteLine("Matching: {0} / {1} {2}", 
                        totRes.Count, 
                        expectedResults.Count, 
                        pass ? "(pass)" : "(fail)");
                    Console.WriteLine();
                }

                Assert.IsTrue(pass);
            }
        }

        /**
         Make sure that the NNs retrieved by the C# version match the
         ones pre-computed by the C++ version of the Angular index.
         */
        [TestMethod] //@Test
        public void testAngular() {
            Console.WriteLine("current dir: " + Directory.GetCurrentDirectory());

            ANNIndex index = new ANNIndex(8, dir + "points.angular.annoy", IndexType.ANGULAR);
            StreamReader reader = new StreamReader(dir + "points.angular.ann.txt");
            testIndex(index, reader, verboseOutput);
        }



        /**
         Make sure that the NNs retrieved by the C# version match the
         ones pre-computed by the C++ version of the Euclidean index.
         */
        [TestMethod] //@Test
        public void testEuclidean() {
            ANNIndex index = new ANNIndex(8, dir + "points.euclidean.annoy", IndexType.EUCLIDEAN);
            StreamReader reader = new StreamReader(dir + "points.euclidean.ann.txt");
            testIndex(index, reader, verboseOutput);
        }
        

    }
}
