using System;
namespace Word2vec.Tools
{
    /// <summary>
    /// word2vec sampling file reader
    /// </summary>
    public abstract class IWord2VecReader
    {
        protected bool normalize;
        protected bool isSourceNormalized;
        public IWord2VecReader(bool normalize, bool isSourceNormalized) {
            this.normalize = normalize;
            this.isSourceNormalized = isSourceNormalized;
        }

        public abstract Vocabulary Read(System.IO.Stream inputStream);
        public abstract Vocabulary Read(string path);
    }
}
