using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public enum NoteStep
    {
        C = 'C',
        D = 'D',
        E = 'E',
        F = 'F',
        G = 'G',
        A = 'A',
        B = 'B'
    }

    public class Note : StaffSymbol
    {
        public NoteStep Step { get; set; }
        public int Alter { get; set; }
        public int Octave { get; set; }
        public StaffSymbolDuration Duration { get; set; }
        public int NumberOfDots { get; set; }
    }
}
