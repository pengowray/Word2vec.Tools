using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Word2vec.Tools
{
    /// <summary>
    /// Reads default w2v text file
    /// </summary>
    public class Word2VecTextReader: IWord2VecReader
    {
        public Word2VecTextReader(bool normalize = false, bool isSourceNormalized = false) : base(normalize, isSourceNormalized) 
        {
        }
        
        public override Vocabulary Read(Stream inputStream)
        {
            using (var strStream = new System.IO.StreamReader(inputStream))
            {
                bool isFirstLine = true;
                int vocabularySize = 0;
                int vectorSize = -1;

                //var firstLine = strStream.ReadLine().Split(' ');

                var vectors = new List<WordRepresentation>();

                var enUsCulture = CultureInfo.GetCultureInfo("en-US");
                while (!strStream.EndOfStream)
                {
                    var line = strStream.ReadLine().Split(' ');
                    if (isFirstLine) {
                        if (line.Length == 2) {
                            try {
                                //header
                                vocabularySize = int.Parse(line[0]);
                                vectorSize = int.Parse(line[1]);
                                vectors = new List<WordRepresentation>(vocabularySize);
                                continue; 
                            } catch {
                                vocabularySize = 0;
                                vectorSize = -1;
                            }
                        }

                        if (vectorSize == -1) {
                            // no header, so require all other vectors to match first line's length
                            vectorSize = line.Length - 1;
                        }

                        isFirstLine = false;
                    }

                    var vecs = line.Skip(1).Take(vectorSize).ToArray();
                    if (vecs.Length != vectorSize)
                        throw new FormatException("word \"" + line.First() + "\" has wrong vector size of " + vecs.Length);

                    if (normalize && !isSourceNormalized) {
                        var vector = vecs.Select(v => Single.Parse(v, enUsCulture)).ToArray();
                        //vector: Vector<float>.Build.Dense(vector).Normalize(2)));
                        vectors.Add(new WordRepresentation(
                           word: line.First(), vector: vector));

                    } else {
                        vectors.Add(new WordRepresentation(
                           word: line.First(),
                           vector: vecs.Select(v => Single.Parse(v, enUsCulture)).ToArray())); // Normalize(2)
                    }
                }
                var vocab = new Vocabulary(vectors, vectorSize, vocabularySize);
                vocab.isNormalized = isSourceNormalized || normalize;
                return vocab;
            }
        }

        public override Vocabulary Read(string path)
        {
            return Read(new FileStream(path, FileMode.Open));
        }
    }
}
