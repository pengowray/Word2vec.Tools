using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Word2vec.Tools 
{
    public static class Word2vecLoader {

        public static Vocabulary LoadFromZip(string zipFile, string unzipFile = null, bool normalize = false, bool isNormalized = false)
        {
            using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry zipEntry;
                    if (string.IsNullOrEmpty(unzipFile))
                    {
                        // No filename provided, so use first file in zip.
                        zipEntry = archive.Entries[0];
                    }
                    else
                    {
                        zipEntry = archive.GetEntry(unzipFile);
                    }

                    using (Stream path = zipEntry.Open())
                    {
                        if (zipEntry.Name.ToLowerInvariant().EndsWith(".txt") || zipEntry.Name.ToLowerInvariant().EndsWith(".words"))
                        {
                            return new Word2VecTextReader(normalize, isNormalized).Read(path);
                        }
                        else
                        {
                            return new Word2VecBinaryReader(normalize, isNormalized).Read(path);
                        }
                    }
                }
            }
        }

        public static Vocabulary LoadGzip(string path, bool normalize = false, bool isNormalized = true)
        {
            if (!path.ToLowerInvariant().EndsWith(".gz"))
                return null; //TODO: throw error

            string nogzPath = path.Substring(0, path.Length - ".gz".Length);

            FileInfo fileToDecompress = new FileInfo(path);

            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    string fileLower = nogzPath.ToLowerInvariant();
                    if (fileLower.EndsWith(".txt") || fileLower.EndsWith(".words"))
                    { // .words is just for "deps.words" (wikipedia_deps)
                        return new Word2VecTextReader(normalize, isNormalized).Read(decompressionStream);

                    }
                    else if (fileLower.EndsWith(".bin"))
                    {
                        return new Word2VecBinaryReader(normalize, isNormalized).Read(decompressionStream);
                    }

                    return null; //TODO: examine file and guess whether binary or txt?
                }
            }

        }

        /// <summary>
        /// Opens .txt and .bin vocab files and compressed versions.
        /// Automatically selects best method to open files.
        /// Opens .txt.gz and .bin.gz, 
        /// If a zip contains only one .bin or .txt file, it will open that too, but better to use LoadGzip() instead
        /// </summary>
        /// <param name="path">Filename of file to load, including path</param>
        /// <returns></returns>
        public static Vocabulary Load(string path, bool normalize = false, bool isNormalized = true)
        {
            string fileLower = path.ToLowerInvariant();
            if (fileLower.EndsWith(".txt") || fileLower.EndsWith(".words"))
            {
                return new Word2VecTextReader(normalize, isNormalized).Read(path);

            }
            else if (fileLower.EndsWith(".bin"))
            {
                return new Word2VecBinaryReader(normalize, isNormalized).Read(path);

            }
            else if (fileLower.EndsWith(".gz"))
            {
                return LoadGzip(path, normalize, isNormalized);

            }
            else if (fileLower.EndsWith(".zip"))
            {
                return LoadFromZip(path, null, normalize, isNormalized);
            }

            //TODO: .npy (numpy)
            //TODO: .tar.gz / .tgz
            //TODO: .bz2 
            //TODO: .index / .index.d (annoy index)

            //TODO: examine file and guess whether binary or txt?
            //TODO: throw error
            return null;
        }
    }

}
