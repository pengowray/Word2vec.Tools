using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Word2vec.Tools
{
    /// <summary>
    /// w2v meaning vector
    /// </summary>
    public class Representation
    {
        public Representation(Vector<float> NumericVector) 
        {
            this.NumericVector = NumericVector;
            MetricLength = NumericVector.L2Norm(); // Euclidean norm
        }
        public Representation(float[] numericVector)
        {
            this.NumericVector = Vector<float>.Build.Dense(numericVector);
            MetricLength = NumericVector.L2Norm(); // Euclidean norm
        }

        public readonly Vector<float> NumericVector;
        public readonly double MetricLength; // The square root of the sum of the squared values. (Euclidean norm)

        public WordDistance GetCosineDistanceToWord(WordRepresentation representation)
        {
            return new WordDistance(representation, GetCosineDistanceTo(representation));
        }

        public double GetCosineDistanceTo(Representation representation)
        {
            //return Distance.Cosine(NumericVector, representation.NumericVector);
            return Distance.Cosine(NumericVector.ToArray(), representation.NumericVector.ToArray());
        }

        public Representation Substract(Representation representation)
        {
            var ans = NumericVector - representation.NumericVector;
            return new Representation(ans);
            /*
            var ans = new float[NumericVector.Length];
            for (int i = 0; i < NumericVector.Length; i++)
                ans[i] = NumericVector[i] - representation.NumericVector[i];
            return new Representation(ans);
            */

        }
        public Representation Add(Representation representation)
        {
            var ans = NumericVector + representation.NumericVector;
            return new Representation(ans);
        }
        public WordDistance[] GetClosestFrom(IEnumerable<WordRepresentation> representations, int maxCount)
        {
            return representations.Select(GetCosineDistanceToWord)
               .OrderByDescending(s => s.Distance)
               .Take(maxCount)
               .ToArray();
        }
    }
}
