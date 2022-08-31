using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public class CanalTcp : IDisposable
    {
        private TcpClient tcpClient;
        private NetworkStream nws;
        private JsonSerializerSettings serializerSettings;

        public CanalTcp(NetworkStream nws, JsonSerializerSettings ss)
        {
            this.nws = nws;
            serializerSettings = ss;
        }

        public CanalTcp(NetworkStream nws)
        {
            this.nws = nws;            
            serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto};            
        }

        public CanalTcp(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            this.nws = this.tcpClient.GetStream();
        }

        public CanalTcp(string ip, int port)
        {
            this.tcpClient = new TcpClient(ip, port);
            serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            this.nws = this.tcpClient.GetStream();
        }

        public IComando recibirComando()
        {
            List<byte> buffer = new List<byte>();
            //TODO: Tamaño de paquete parametrizable
            byte[] paqueteDatos = new byte[256];
            UInt32 nBytesEsperados;
            int nBytesUltimaLectura;
            int nBytesResto;


            //Leemos los 4 primeros bytes para tener el tamaño en bytes del comando a recibir y calculamos parametros adicionales
            nBytesUltimaLectura = nws.Read(paqueteDatos, 0, 4); //TODO: ver si el tamaño de un uint32 se puede poner por algun enum... aunque tampoco tiene mucho sentido
            nBytesEsperados = BitConverter.ToUInt32(paqueteDatos, 0);

            int numeroLecturasCompletas = (int)Math.Floor((double)nBytesEsperados / paqueteDatos.Length);
            nBytesResto = (int)(nBytesEsperados - (numeroLecturasCompletas * paqueteDatos.Length));

            int i = 0;
            //Hacemos todas las lecturas de tamaño de buffer paqueteDatos, el resto se hace en una lectura posterior
            while (i < numeroLecturasCompletas && nws.Read(paqueteDatos, 0, paqueteDatos.Length) != 0)
            {
                buffer.AddRange(paqueteDatos);
                i++;
            }

            if (numeroLecturasCompletas != i)
            {
                //TODO: Algo ha pasado en la comunicación, tratar el error
                throw new Exception("Algo ha pasado con la lectura de stream");
            }

            //Terminamos la ultima lectura con la longitud calculada
            nBytesUltimaLectura = nws.Read(paqueteDatos, 0, nBytesResto);
            if (nBytesUltimaLectura != nBytesResto)
            {
                //TODO: Error por mensaje truncado
                throw new Exception("La cagaste, burlancaster");
            }
            buffer.AddRange(paqueteDatos.ToList().GetRange(0, nBytesResto)); //Nos llevamos exclusivamente los bytes con datos, el resto? pa' los perros

            //TODO: Esto pide ser otra funcion
            string baulSerializado = Encoding.UTF8.GetString(buffer.ToArray());
            BaulTcp baul = JsonConvert.DeserializeObject<BaulTcp>(baulSerializado, serializerSettings);

            return baul.ComandoSerializado;

        }

        public int enviarComando(IComando comando)
        {
            byte[] monoficheroBytes;
            BaulTcp bt = new BaulTcp(comando);

            string baulString = JsonConvert.SerializeObject(bt, serializerSettings);
            monoficheroBytes = Encoding.UTF8.GetBytes(baulString);

            //Calculamos y mandamos la longitud del objeto que el receptor debe leer en los primeros 4bytes (Unsigned Integer de 32 bits)
            UInt32 size = (uint)monoficheroBytes.Length;
            Byte[] bytes = BitConverter.GetBytes(size);
            nws.Write(bytes, 0, bytes.Length);

            //Mandamos el objeto del comando empaquetado en el baulTcp. Buen viaje!
            nws.Write(monoficheroBytes, 0, monoficheroBytes.Length);

            return monoficheroBytes.Length;
        }

        public void Dispose()
        {
            this.nws?.Close();
            this.nws?.Dispose();
            this.tcpClient?.Dispose();            
        }

        public void Cerrar()
        {
            this.Dispose();
        }
    }
}
