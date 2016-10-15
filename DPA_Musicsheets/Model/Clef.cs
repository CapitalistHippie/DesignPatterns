using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public enum ClefType
    {
        G,
        C,
        F
    }

    public class Clef : StaffSymbol
    {
        public ClefType Type { get; set; }

        public override void Accept(ScoreVisitor smVisitor, int index)
        {
            smVisitor.Visit(this);
        }
    }
}
