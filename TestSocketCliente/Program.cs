// See https://aka.ms/new-console-template for more information
using Common;
using Force.Crc32;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;

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
    using (FileStream fisfis = new("el_pollito.txt", FileMode.Append))
    {
        Console.WriteLine("I'm in [Hacker noises]");
        //int i, j;
        //j = 1;
        //while ((i = ns.Read(dataPaq, 0, dataPaq.Length)) != 0)
        //{
        //    if (j % 100 == 0)
        //    {
        //        Console.WriteLine(String.Format("{0} Lectura de {1} bytes", j, i));
        //    }
        //    j++;
        //    //fisfis.Write(dataPaq, 0, i);
        //    acumulador.AddRange(dataPaq);
        //    //jsonCommand = jsonCommand + Encoding.UTF8.GetString(dataPaq, 0, i);
        //}
    }
    //FileJson fson = JsonSerializer.Deserialize<FileJson>(jsonCommand);
    //byte[] innerDoc = Convert.FromBase64String(fson.FileBase64);
    //UInt32 crcRecalc = Crc32Algorithm.Compute(innerDoc);

    //if (crcRecalc != fson.Crc32)
    //{
    //    Console.WriteLine("Oh oh, los CRCs no coinciiiideeeeen~~");
    //}
    //else
    //{
    //    Console.WriteLine("Hurraaaah!, los CRCs van perfect");
    //}

    ComandoEnvioFichero feison = unpackageFile(acumulador.ToArray());
    UInt32 crcRecalc = Crc32Algorithm.Compute(Convert.FromBase64String(feison.FileBase64));
    if (crcRecalc != feison.Crc32)
    {
        Console.WriteLine("Oh oh, los CRCs no coinciiiideeeeen~~");
    }
    else
    {
        Console.WriteLine("Hurraaaah!, los CRCs van perfect");
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

ComandoEnvioFichero unpackageFile(byte[] rawData)
{
    string conversion = Encoding.UTF8.GetString(rawData);
    ComandoEnvioFichero res = JsonSerializer.Deserialize<ComandoEnvioFichero>(conversion);
    return res;
}

IComando recibirComando(NetworkStream nws)
{
    List<byte> buffer = new List<byte>();
    //TODO: Tamaño de paquete parametrizable
    byte[] paqueteDatos = new byte[256];


    //Leemos los 4 primeros bytes para tener el tamaño  en bytes del comando a recibir
    UInt32 lecturaPendiente;
    int nBytes = 0;

    nBytes = nws.Read(paqueteDatos, 0, 4); //TODO: ver si el tamaño de un uint32 se puede poner por algun enum... aunque tampoco tiene mucho sentido
    lecturaPendiente = BitConverter.ToUInt32(paqueteDatos);

    int numeroLecturas = (int)Math.Ceiling((double)lecturaPendiente / paqueteDatos.Length);

    //Empezamos lectura n-1 veces, la ultima lectura la hacemos a mano al final, para evitar condicionales en el bucle principal (optimizacion)
    while ((nBytes = nws.Read(paqueteDatos, 0, paqueteDatos.Length)) != 0 && numeroLecturas > 1)
    {
        buffer.AddRange(paqueteDatos);
        numeroLecturas--;
    }

    
    if (numeroLecturas == 1)
    {
        string baulSerializado = Encoding.UTF8.GetString(buffer.ToArray());
        BaulTcp baul = JsonSerializer.Deserialize<BaulTcp>(baulSerializado);
        Type tipoComando = Type.GetType(baul.NombreComando);

        //Parametrizamos el generico de "Deserialize" utilizando reflection. recibimos un IComando generico
        MethodInfo method = Type.GetType("System.Text.Json.JsonSerializer").GetMethod("Deserialize").MakeGenericMethod(new Type[] { tipoComando });

        IComando comandoRecibido = (IComando)method.Invoke(null, new object[] { baul.ComandoSerializado });
        Console.WriteLine(comandoRecibido.GetType().ToString());
        return comandoRecibido;
    }
    else
    {
        //TODO: algo ha ido mal en la lectura
        return null;
    }


}