using Common;
using Force.Crc32;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TestTCPServ
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string directorioTest = @"C:\Users\konom\OneDrive\Documentos\ficheros_prueba_tcp";

            //string[] filetitos = Directory.GetFiles(directorio);

            //Creamos el listener
            int port = 13000;
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[3];
            TcpListener tcpListener;

            try
            {
                tcpListener = new TcpListener(ipAddr, port);
                tcpListener.Start();
                Console.WriteLine("Esperando...");
                TcpClient conexion = tcpListener.AcceptTcpClient();
                Console.WriteLine(string.Format("Conectado a {0}", conexion.ToString()));
                CanalTcp canalTcp = new CanalTcp(conexion);
                string[] ficheros = Directory.GetFiles(directorioTest);
                Console.WriteLine(String.Format("Encontrados {0} ficheros", ficheros.Length));
                int bytesTotales = 0;
                foreach (string fichero in ficheros)
                {
                    Console.WriteLine("Transmitiendo fichero " + fichero);

                    ComandoEnvioFichero cef = packageFile(fichero);
                    int bytesEnvio = canalTcp.enviarComando(cef);
                    if(bytesEnvio <= 0)
                    {
                        //TODO: Control de errores necesario
                    }
                    bytesTotales = bytesTotales + bytesEnvio;
                }
                ComandoFinalizarEnvios cfe = new ComandoFinalizarEnvios();
                if(canalTcp.enviarComando(cfe) <= 0)
                {
                    //TODO: Control de errores (quizas mas limpio si quitamos IFs y metemos una excepcion y todo en trycatch? seguramente si
                }  

                Console.WriteLine(String.Format("Se ha intentado enviar un total de {0} bytes, bon voyage!", bytesTotales));

                canalTcp.Cerrar(); //No sacudimos nada
                //tipistrim.Close(3000); //3seg para sacudir las gotitas
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
                    FileName = Path.GetFileName(path),
                    FileBase64 = Convert.ToBase64String(binaryFile)
                };
                return fileJson;
            }

            
        }
    }
}
