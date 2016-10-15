using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Rest : StaffSymbol
    {
        public StaffSymbolDuration Duration { get; set; }
        public int NumberOfDots { get; set; }

        public override void Accept(ScoreVisitor smVisitor, int index)
        {
            smVisitor.Visit(this);
        }
    }
}
