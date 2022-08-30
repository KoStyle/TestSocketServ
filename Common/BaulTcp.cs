
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Common
{
    public class BaulTcp
    {
        private readonly string _nombreComando;
        private readonly string _comandoSerializado;

        public BaulTcp(IComando comando)
        {
            _nombreComando = comando.GetType().Name;
            _comandoSerializado = JsonSerializer.Serialize(comando);
        }

        public string NombreComando => _nombreComando;

        public string ComandoSerializado => _comandoSerializado;
    }
}
