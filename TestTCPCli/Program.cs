using Common;
using Force.Crc32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace TestTCPCli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // See https://aka.ms/new-console-template for more information


            // Establish the local endpoint for the socket.

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            CanalTcp cTcp = new CanalTcp(ipHost.HostName, 13000);            

            Console.WriteLine("Me duermo 5s");
            Thread.Sleep(5000);

            try
            {
                //Esto puede fallar por lanzarse antes que el servidor
                using (FileStream fisfis = new FileStream("el_pollito.txt", FileMode.Append))
                {
                    Console.WriteLine("I'm in [Hacker noises]");
                    IComando tmp = cTcp.recibirComando();
                    while (!(tmp is ComandoFinalizarEnvios))
                    {
                        ComandoEnvioFichero comandoRecibido = (ComandoEnvioFichero)tmp;
                        byte[] rawFile = unpackageFile(comandoRecibido);
                        UInt32 crcRecalc = Crc32Algorithm.Compute(rawFile);

                        if (crcRecalc != comandoRecibido.Crc32)
                        {
                            Console.WriteLine("Oh oh, los CRCs no coinciiiideeeeen~~");
                        }
                        else
                        {
                            File.WriteAllBytes(comandoRecibido.FileName, rawFile);
                            Console.WriteLine("Hurraaaah!, los CRCs van perfect");
                        }
                        tmp = cTcp.recibirComando();
                    }
                }

                cTcp.Cerrar();
                Console.WriteLine("a.d.i.o.s");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("De nuevo, nada hecho");
                Console.ReadKey();
            }

            byte[] unpackageFile(ComandoEnvioFichero comando)
            {
                return Convert.FromBase64String(comando.FileBase64);

            }          
        }
    }
}
