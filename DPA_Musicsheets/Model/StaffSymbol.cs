using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public abstract class StaffSymbol
    {
        public abstract void Accept(ScoreVisitor smVisitor, int index);
    }
}
