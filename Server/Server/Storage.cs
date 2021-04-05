using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Storage
    {
        public double TotalAvailableSpace { get; set; }
        public double TotalSizeOfDrive { get; set; }
        public string RootDirectory { get; set; }
    }
}
