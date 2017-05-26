
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Word2vec.Tools
{
    /// <summary>
    /// Word and its w2v meaning vector
    /// </summary>
    public class WordRepresentation : Representation
    {
        public WordRepresentation(string word, float[] vector, int fileOrder = -1): base(vector)
        {
            this.Word = word;
            this.Rank = fileOrder;
        }

        /*
        public WordRepresentation(string word, Vector<float> vector, int rank = -1) : base(vector) {
            this.Word = word;
            this.Rank = rank;
        }
        */

        public WordRepresentation(string word, DenseVector vector, int rank = -1) : base(vector)
        {
            this.Word = word;
            this.Rank = rank;
        }

        public override Representation Normalize() {
            if (IsNormal())
                return this;

            //var ans = NumericVector.Normalize(2);
            var ans = new DenseVector(NumericVector.Normalize(2).ToArray()); //TODO: ugh
            return new WordRepresentation(Word, ans);
        }

        public readonly string Word;

        public int Rank = -1; // order it appears in the Word2Vec file, which is generally the frequency it occurs
    }
}
