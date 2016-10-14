using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class TimeSignature : StaffSymbol
    {
        public int NumberOfBeats { get; set; }
        public int Measure { get; set; }

        public override void Accept(SheetMusicVisitor smVisitor, int index)
        {
            smVisitor.Visit(this);
        }
    }
}
