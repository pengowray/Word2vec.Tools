using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Word2vec.Tools {

    public class FastVocabulary : IVocabulary  {

        // note: index vectors will be normalized while Words might not be
        public ANNIndex annoyIndex;
        public Vocabulary vocab;

        public int limitTrees = -1; // -1 for off

        public override WordRepresentation[] Words { get { return vocab.Words; } }

        public override int VectorDimensionsCount {
            get { return vocab.VectorDimensionsCount; }
            set { vocab.VectorDimensionsCount = value; }
        }

        public FastVocabulary(Vocabulary vocab) {
            //: base(vocab.Words, vocab.VectorDimensionsCount, vocab.StatedVocabularySize) {

            this.vocab = vocab;
        }

        public override WordDistance[] Nearest(Representation nearthis, int maxCount, int onlyFromTop = int.MaxValue) {

            //TODO: onlyFromTop param is ignored with FastVocab (for now)
            //TODO: Should use distances from annoyIndex instead of recalculating
            //TODO: doesn't remove nearthis from results

            var finds = annoyIndex.getNearest(nearthis.NumericVector.ToArray(), maxCount, limitTrees);
            return finds
                .Select(f => vocab.Words[f])
                .Select(wr => new WordDistance(wr, wr.GetCosineDistanceTo(nearthis))).ToArray();
        }

        public override WordDistance[] EuclideanNearest(Representation representation, int maxCount, int onlyFromTop = int.MaxValue) {
            //TODO
            return vocab.EuclideanNearest(representation, maxCount, onlyFromTop);
        }

        public void LoadAnnoyIndex(string filename, IndexType filesIndexType = IndexType.ANGULAR) {
            annoyIndex = new ANNIndex(VectorDimensionsCount, filename, filesIndexType);
        }

        public override bool ContainsWord(string word) {
            return vocab.ContainsWord(word);
        }

        public override WordRepresentation GetRepresentationFor(string word) {
            return vocab.GetRepresentationFor(word);
        }
    }
}
