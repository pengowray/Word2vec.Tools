using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Word2vec.Tools {
    public abstract class IVocabulary {


        public abstract int VectorDimensionsCount { get; set; }
        public abstract WordRepresentation[] Words { get; }

        public abstract bool ContainsWord(string word);
        //public abstract WordDistance[] EuclideanNearest(Representation representation, int maxCount, int onlyFromTop = int.MaxValue);
        public abstract WordRepresentation GetRepresentationFor(string word);
        public abstract WordDistance[] Nearest(Representation representation, int maxCount, int onlyFromTop = int.MaxValue);
        public abstract WordDistance[] EuclideanNearest(Representation representation, int maxCount, int onlyFromTop = int.MaxValue);

        //public abstract void WriteTextFile(string filename);

        //////////////// conveinience methods ///////////////////////////

        /// <summary>
        /// if word exists - returns "count" of best fits for target word
        /// otherwise - returns empty array
        /// </summary>
        public virtual WordDistance[] Nearest(string word, int count, int onlyFromTop = int.MaxValue) {
            if (!this.ContainsWord(word))
                return new WordDistance[0];

            return Nearest(this[word], count, onlyFromTop);
        }

        /// <summary>
        /// Returns word2vec word vector if it exists.
        /// Returns null otherwise
        /// </summary>
        public virtual WordRepresentation GetRepresentationOrNullFor(string word) {
            if (ContainsWord(word))
                return GetRepresentationFor(word);
            else
                return null;
        }

        public virtual WordRepresentation this[string word] { get { return GetRepresentationOrNullFor(word); } }

        /// <summary>
        /// If wordA is wordB, then wordC is...
        /// If all words exist - returns "count" of best fits for the result
        /// otherwise - returns empty array
        /// </summary>
        public virtual WordDistance[] Analogy(string wordA, string wordB, string wordC, int count) {
            if (!ContainsWord(wordA) || !ContainsWord(wordB) || !ContainsWord(wordC))
                return new WordDistance[0];
            else
                return Analogy(this[wordA], this[wordB], this[wordC], count);
        }
        /// <summary>
        /// wordA is to be wordB, as wordC is to...
        /// Returns "count" of best fits for the result
        /// </summary>
        public virtual WordDistance[] Analogy(Representation wordA, Representation wordB, Representation wordC, int count) {
            var cummulative = wordB.Substract(wordA).Add(wordC);
            //return cummulative.GetClosestFrom(Words.Where(t => t != wordA && t != wordB && t != wordC), count);

            //TODO: don't bother filtering. leave that for the client
            var dist = Nearest(cummulative, count + 3);
            return dist.Where(t => t.Representation != wordA && t.Representation != wordB && t.Representation != wordC).Take(count).ToArray();
        }

    }
}