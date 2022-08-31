
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
        public BaulTcp(IComando comando)
        {
            NombreComando = comando.GetType().Name;
            ComandoSerializado = JsonSerializer.Serialize((object)comando);
        }

        //Este constructor es necesario para la deserializacion con System.Text.Json 
        //(el otro falla porque tiene un parametro que no figura en la lista de propiedades de la clase)
        public BaulTcp() { } 

        public string NombreComando { get; set; }

        public string ComandoSerializado { get; set; }
    }
}
