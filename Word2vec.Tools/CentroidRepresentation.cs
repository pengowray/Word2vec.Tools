
using MathNet.Numerics.LinearAlgebra;

namespace Word2vec.Tools
{
    /// <summary>
    /// Word and its w2v meaning vector
    /// </summary>
    public class CentroidRepresentation : Representation
    {
        public CentroidRepresentation(int index, float[] vector): base(vector)
        {
            this.Index = index;
        }

        public CentroidRepresentation(int index, Vector<float> vector) : base(vector) {
            this.Index = index;
        }

        public override Representation Normalize() {
            if (IsNormal())
                return this;

            var ans = NumericVector.Normalize(2);
            return new CentroidRepresentation(Index, ans);
        }

        public readonly int Index;
       
    }
}
