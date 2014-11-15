using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    using System.IO;
    using System.Net.Sockets;

    using TestClient.Commandline;

    class Program
    {
        private static void Main(string[] args)
        {

            string invokedVerb = "";
            object invokedVerbOptions = null;
            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
                {
                    invokedVerb = verb;
                    invokedVerbOptions = subOptions;
                }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            };


            var commonOptions = (SubOptionsBase)invokedVerbOptions;

            try
            {

                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                using (var client = new TcpClient(commonOptions.Host, commonOptions.Port))
                {

                    var builder = new MessageBuilder();

                    string message = builder.GetMessage(invokedVerb, invokedVerbOptions);


                    // Translate the passed message into ASCII and store it as a Byte array.
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(string.Format("{0}{1}", message, Environment.NewLine));

                    // Get a client stream for reading and writing. 
                    //  Stream stream = client.GetStream();

                    using (var stream = client.GetStream())
                    {

                        // Send the message to the connected TcpServer. 
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Sent: [{0}]", message);

                        // String to store the response ASCII representation.
                        var responseData = "";

                        // Read the first batch of the TcpServer response bytes.
                        data = ReadFully(stream);

                        //Int32 bytes = stream.Read(data, 0, data.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);
                        Console.WriteLine("Received: {0}", responseData);

                    }

                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }

        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }

}
