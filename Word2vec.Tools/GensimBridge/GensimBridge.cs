using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net;

//WIP
namespace Word2vec.Tools.GensimBridge {
    public class GensimBridge {
        public Int32 port = 5015;
        public string hostname = "127.0.0.1";

        public GensimBridge() {
        }

        static void Main(string[] args) {
            // Display the number of command line arguments:
            System.Console.WriteLine(args.Length);
        }

        public void SendData(Vocabulary vocab) {
            string domain = string.Format("{0}:{1}...", hostname, port);
            System.Console.WriteLine("Connecting to {0}:{1}...", hostname, port);
            var encoder = System.Text.Encoding.Unicode;


            using (WebClient client = new WebClient()) { 

                //client.DownloadFile("http://yoursite.com/page.html", @"C:\localfile.html");
                string htmlCode = client.DownloadString("http://" + domain + "/hello");
                System.Console.WriteLine("Connected.");

                foreach (WordRepresentation wr in vocab.Words) {
                    //byte[] bytes = encoder.GetBytes(wr.Word + '\n');
                    //stream.Write(bytes, 0, bytes.Length);
                    string htmlAdded = client.DownloadString("http://" + domain + "/add/" + wr.Word);
                }

                string goodbyeHtmlCode = client.DownloadString("http://" + domain + "/goodbye");
            }



        }

        public void SendDataSimple(Vocabulary vocab) {
            System.Console.WriteLine("Connecting to {0}:{1}...", hostname, port);
            TcpClient connection = new TcpClient();
            connection.Connect(hostname, port);
            NetworkStream stream = connection.GetStream();
            var encoder = System.Text.Encoding.Unicode;

            System.Console.WriteLine("Connected.");

            foreach (WordRepresentation wr in vocab.Words) {
                byte[] bytes = encoder.GetBytes(wr.Word + '\n');
                stream.Write(bytes, 0, bytes.Length);
            }

            stream.Close();
            connection.Close();
        }
    }
}
