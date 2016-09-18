using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Note : StaffSymbol
    {
        public int Step { get; set; }
        public string StepString { get; set; }
        public int Alter { get; set; }
        public int Octave { get; set; }
        public StaffSymbolDuration Duration { get; set; }
        public int NumberOfDots { get; set; }
        public int StartTime { get; set; }
    }
}
