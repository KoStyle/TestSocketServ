using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ComandoEnvioFichero : IComando
    {
        private UInt32 crc32;
        private string fileName;
        private string fileBase64;

        public uint Crc32 { get => crc32; set => crc32 = value; }
        public string FileBase64 { get => fileBase64; set => fileBase64 = value; }
        public string FileName { get => fileName; set => fileName = value; }
    }
}
