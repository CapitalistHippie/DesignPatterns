using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Rest : Symbol
    {
        public SymbolDuration Duration { get; set; }
        public int NumberOfDots { get; set; }
    }
}
