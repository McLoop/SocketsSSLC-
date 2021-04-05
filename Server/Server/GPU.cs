using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GPU
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string AdapterRAM { get; set; }
        public string AdapterDACType { get; set; }
        public string DriverVersion { get; set; }
    }
}
