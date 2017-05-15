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
        public Representation(Vector<float> NumericVector) // todo: go back to using a float[]
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

        // was: GetAngularCosineDistanceTo
        public double GetSimpleAngleTo(Representation representation) {
            //return Math.Acos(1 - GetCosineDistanceTo(representation)); // divide by pi?
            
            var result = Math.Acos(NumericVector.DotProduct(representation.NumericVector));
            return result;
        }

        public WordDistance GetSimpleAngleToWord(WordRepresentation representation) {
            return new WordDistance(representation, GetSimpleAngleTo(representation));
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
        public virtual Representation Normalize() 
        {
            if (IsNormal())
                return this;

            var ans = NumericVector.Normalize(2);
            return new Representation(ans);
        }
        public bool IsNormal() {
            return Math.Abs(MetricLength - 1) < 0.0035; // accept even freebase_skipgram1000_en's normalization
        }
        public WordDistance[] NearestFrom(IEnumerable<WordRepresentation> representations, int maxCount)
        {
            return representations.Select(GetCosineDistanceToWord)
               .OrderBy(s => s.Distance)  //.OrderByDescending(s => s.Distance)
               .Take(maxCount)
               .ToArray();
        }

        
        public WordDistance[] GetSimpleAngleNearestFrom(IEnumerable<WordRepresentation> representations, int maxCount) {
            // temporary test function
            return representations.Select(GetSimpleAngleToWord)
               .OrderBy(s => s.Distance)  //.OrderByDescending(s => s.Distance)
               .Take(maxCount)
               .ToArray();
        }

        /// <returns>Return the nearest cluster if it's known/meaningful.</returns>
        internal virtual Cluster NearestCluster() {
            return null;
        }

    }
}
