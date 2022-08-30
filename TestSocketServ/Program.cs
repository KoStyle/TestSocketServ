// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using Common;
using Force.Crc32;
using System.Text.Json;
using System.Text;

string directorio = @"C:\Users\konom\OneDrive\Escritorio\bausura";
string monofichero = @"C:\Users\konom\Source\Repos\TestSocketServ\TestSocketServ\el_huevito.txt";
string directorioTest = @"C:\Users\konom\OneDrive\Documentos\ficheros_prueba_tcp";

//string[] filetitos = Directory.GetFiles(directorio);
byte[] monoficheroBytes = null;
//Creamos el listener
Int32 port = 13000;
IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
IPAddress ipAddr = ipHost.AddressList[3];
TcpListener tcpListener = null;

try
{
    tcpListener = new(ipAddr, port);
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
        monoficheroBytes = packageFile(fichero);
        tipistrim.Write(monoficheroBytes, 0, monoficheroBytes.Length);
        bytesTotales = bytesTotales + monoficheroBytes.Length;
    }
    Thread.Sleep(3000); //Sacudimos las gotitas        
    Console.WriteLine(String.Format("Se ha intentado enviar un total de {0} bytes, bon voyage!", bytesTotales));
    tipistrim.Close();


}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
    Console.WriteLine("Pensabas que iba a hacer algo con la excepcion? ja");
    Console.WriteLine("Linguo HA muerto");
}

/**
 * Esta funcion se encarga de componer un objeto comando a partir de la ruta de un fichero.
 * */
byte[] packageFile(string path)
{
    byte[] binaryFile = File.ReadAllBytes(path);
    ComandoEnvioFichero fileJson = new ComandoEnvioFichero { Crc32 = 0, FileBase64 = Convert.ToBase64String(binaryFile) };
    fileJson.Crc32 = Crc32Algorithm.Compute(binaryFile, 0, binaryFile.Length);
    string json = JsonSerializer.Serialize(fileJson);
    return Encoding.UTF8.GetBytes(json);
}