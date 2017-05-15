using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Word2vec.Tools
{
    /// <summary>
    /// known w2v vectors
    /// </summary>
    public class Vocabulary : IVocabulary {
        /// <summary>
        /// All known words w2v representations
        /// </summary>
        public override WordRepresentation[] Words { get { return _words; } }
        protected WordRepresentation[] _words;

        /// <summary>
        /// w2v words vectors dimensions count
        /// </summary>
        public override int VectorDimensionsCount { get; set; }

        /// <summary>
        /// false if vectors are not normalized or unknown normalization
        /// true to enable optimizations (NYI)
        /// </summary>
        public bool isNormalized;

        readonly Dictionary<string, WordRepresentation> _dictionary;

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
            _words = _dictionary.Values.ToArray();
            this.StatedVocabularySize = StatedVocabularySize;
        }

        /// <summary>
        /// Returns word2vec word vector if it exists.
        /// Throw otherwise
        /// </summary>
        public override WordRepresentation GetRepresentationFor(string word)
        {
            return _dictionary[word];
        }

        public override bool ContainsWord(string word)
        {
            return _dictionary.ContainsKey(word);
        }

        public override WordDistance[] EuclideanNearest(Representation representation, int maxCount, int onlyFromTop = int.MaxValue) {
            // acos dot product angle
            // was called: SimpleAngel

            return representation.GetSimpleAngleNearestFrom(Words.Take(onlyFromTop).Where(v => v != representation), maxCount);
        }

        /// <summary>
        /// returns "count" of closest words for target representation, but only from the first "onlyFromTop" entries in the vocab (which is typically sorted by occurrences in the corpus)
        /// </summary>
        public override WordDistance[] Nearest(Representation representation, int maxCount, int onlyFromTop = int.MaxValue) {
            return representation.NearestFrom(Words.Take(onlyFromTop).Where(v => v != representation), maxCount);
        }


        //untested
        public void WriteTextFile(string filename) {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8)) {
                // header
                int vocabularySize = Words.Length;
                int vectorSize = VectorDimensionsCount;
                if (vocabularySize > 0 && vectorSize <= 0) {
                    vectorSize = Words[0].NumericVector.Count();
                }
                writer.WriteLine("{0} {1}", vocabularySize, vectorSize);


                foreach (var word in Words) {
                    StringBuilder vecString = new StringBuilder();
                    foreach (var v in word.NumericVector) {
                        vecString.Append(v.ToString("N6")); // 6 d.p.
                        vecString.Append(" ");
                    }

                    writer.WriteLine("{0} {1}",
                        word.Word,
                        vecString);
                }
            }
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