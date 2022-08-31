using System.Net;
using System.Net.Sockets;
using Common;
using Force.Crc32;
using System.Text.Json;
using System.Text;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace TestTCPServ
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string directorioTest = @"C:\Users\konom\OneDrive\Documentos\ficheros_prueba_tcp";

            //string[] filetitos = Directory.GetFiles(directorio);
            byte[] monoficheroBytes = null;
            //Creamos el listener
            int port = 13000;
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[3];
            TcpListener tcpListener = null;

            try
            {
                tcpListener = new TcpListener(ipAddr, port);
                tcpListener.Start();
                Console.WriteLine("Esperando...");
                TcpClient conexion = tcpListener.AcceptTcpClient();
                Console.WriteLine(string.Format("Conectado a {0}", conexion.ToString()));

                string[] ficheros = Directory.GetFiles(directorioTest);
                Console.WriteLine(String.Format("Encontrados {0} ficheros", ficheros.Length));
                int bytesTotales = 0;
                NetworkStream tipistrim = conexion.GetStream();
                foreach (string fichero in ficheros)
                {
                    Console.WriteLine("Transmitiendo fichero " + fichero);

                    ComandoEnvioFichero cef = packageFile(fichero);
                    BaulTcp bt = new BaulTcp(cef);

                    //string baulString = JsonSerializer.Serialize<BaulTcp>(bt);
                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                    string baulString = JsonConvert.SerializeObject(bt, settings);
                    File.WriteAllText("auxOrigen.json", baulString);
                    monoficheroBytes = Encoding.UTF8.GetBytes(baulString);

                    UInt32 redondeoSize = (uint)(Math.Ceiling((double)monoficheroBytes.Length / 256) * 256);

                    UInt32 size = (uint)monoficheroBytes.Length;
                    Byte[] bytes = BitConverter.GetBytes(size);
                    tipistrim.Write(bytes, 0, bytes.Length);  //Mandamos tamaño de comando


                    tipistrim.Write(monoficheroBytes, 0, monoficheroBytes.Length);
                    tipistrim.Flush();
                    bytesTotales = bytesTotales + monoficheroBytes.Length;
                }
                Console.WriteLine(String.Format("Se ha intentado enviar un total de {0} bytes, bon voyage!", bytesTotales));
                

                tipistrim.Close(30000); //3seg para sacudir las gotitas
                Console.ReadLine();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Pensabas que iba a hacer algo con la excepcion? ja");
                Console.WriteLine("Linguo HA muerto");
                Console.ReadLine();
            }

            /**
             * Esta funcion se encarga de componer un objeto comando a partir de la ruta de un fichero.
             * */
            ComandoEnvioFichero packageFile(string path)
            {
                byte[] binaryFile = File.ReadAllBytes(path);
                ComandoEnvioFichero fileJson = new ComandoEnvioFichero
                {
                    Crc32 = Crc32Algorithm.Compute(binaryFile, 0, binaryFile.Length),
                    FileBase64 = Convert.ToBase64String(binaryFile)
                };
                return fileJson;
            }
        }
    }
}
