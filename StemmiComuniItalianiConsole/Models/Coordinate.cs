using System;
using System.Collections.Generic;
using System.Text;

namespace StemmiComuniItalianiConsole.Models
{
    public class Coordinate
    {
        public string CodAmm { get; set; }
        public string CAP { get; set; }
        public string NomeComune { get; set; }
        public dynamic latitude { get; set; }
        public dynamic longitude { get; set; }
    }
}
