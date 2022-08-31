﻿using Common;
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
            IPAddress ipAddr = ipHost.AddressList[0];
            TcpClient tcpClient = null;
            byte[] dataPaq = new byte[256];
            string jsonCommand = "";

            Console.WriteLine("Me duermo 5s");
            Thread.Sleep(5000);

            try
            {
                //Esto puede fallar por lanzarse antes que el servidor
                tcpClient = new TcpClient(ipHost.HostName, 13000); //A capon, como los grandes
                List<byte> acumulador = new List<byte>();
                using (NetworkStream ns = tcpClient.GetStream())
                using (FileStream fisfis = new FileStream("el_pollito.txt", FileMode.Append))
                {
                    Console.WriteLine("I'm in [Hacker noises]");
                    ComandoEnvioFichero comandoRecibido = (ComandoEnvioFichero)recibirComando(ns);
                    UInt32 crcRecalc = Crc32Algorithm.Compute(unpackageFile(comandoRecibido));
                    if (crcRecalc != comandoRecibido.Crc32)
                    {
                        Console.WriteLine("Oh oh, los CRCs no coinciiiideeeeen~~");
                    }
                    else
                    {
                        Console.WriteLine("Hurraaaah!, los CRCs van perfect");
                    }
                }

                tcpClient.Close();
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

            IComando recibirComando(NetworkStream nws)
            {
                List<byte> buffer = new List<byte>();
                //TODO: Tamaño de paquete parametrizable
                byte[] paqueteDatos = new byte[256];


                //Leemos los 4 primeros bytes para tener el tamaño en bytes del comando a recibir y calculamos parametros adicionales
                UInt32 nBytesEsperados;
                int nBytesUltimaLectura = 0;
                int nBytesResto;

                nBytesUltimaLectura = nws.Read(paqueteDatos, 0, 4); //TODO: ver si el tamaño de un uint32 se puede poner por algun enum... aunque tampoco tiene mucho sentido
                nBytesEsperados = BitConverter.ToUInt32(paqueteDatos, 0);

                int numeroLecturasCompletas = (int)Math.Floor((double)nBytesEsperados / paqueteDatos.Length);
                nBytesResto = (int)(nBytesEsperados - (numeroLecturasCompletas * paqueteDatos.Length));

                int i = 0;
                //Empezamos lectura n-1 veces, la ultima lectura la hacemos a mano al final, para evitar condicionales en el bucle principal (optimizacion)
                while ((nBytesUltimaLectura = nws.Read(paqueteDatos, 0, paqueteDatos.Length)) != 0 && i< numeroLecturasCompletas -1)
                {
                    buffer.AddRange(paqueteDatos);
                    i++;
                }
                buffer.AddRange(paqueteDatos);

                if (numeroLecturasCompletas -1 == i)
                {
                    //Terminamos la ultima lectura con la longitud calculada
                    nBytesUltimaLectura = nws.Read(paqueteDatos, 0, nBytesResto);
                    if (nBytesUltimaLectura != nBytesResto)
                    {
                        //TODO: Error por mensaje truncado
                        throw new Exception("La cagaste, burlancaster");
                    }
                    buffer.AddRange(paqueteDatos.ToList().GetRange(0, nBytesResto)); //Nos llevamos exclusivamente los bytes con datos, el resto? pa' los perros

                    //TODO: Esto pide ser otra funcion
                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                    string baulSerializado = Encoding.UTF8.GetString(buffer.ToArray());
                    File.WriteAllText("auxDestino.json", baulSerializado);
                    //BaulTcp baul = JsonSerializer.Deserialize<BaulTcp>(baulSerializado);
                    BaulTcp baul = JsonConvert.DeserializeObject<BaulTcp>(baulSerializado, settings);
                    Type tipoComando = Type.GetType(baul.NombreComando);

                    //Parametrizamos el generico de "Deserialize" utilizando reflection. recibimos un IComando generico
                    //MethodInfo method = Type.GetType("System.Text.Json.JsonSerializer").GetMethod("Deserialize").MakeGenericMethod(new Type[] { tipoComando });

                    //IComando comandoRecibido = (IComando)method.Invoke(null, new object[] { baul.ComandoSerializado });
                    //Console.WriteLine(comandoRecibido.GetType().ToString());
                    return baul.ComandoSerializado;
                    //return comandoRecibido;
                }
                else
                {
                    //TODO: algo ha ido mal en la lectura
                    return null;
                }


            }
        }
    }
}
