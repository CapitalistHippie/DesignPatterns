using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class TimeSignature : Symbol
    {
        public int NumberOfBeats { get; set; }
        public int Measure { get; set; }
    }
}
