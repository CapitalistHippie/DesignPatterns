using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Barline : StaffSymbol
    {
        public override void Accept(SheetMusicVisitor smVisitor, int index)
        {
            smVisitor.Visit(this);
        }
    }
}
