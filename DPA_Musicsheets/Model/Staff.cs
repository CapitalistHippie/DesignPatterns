using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Staff
    {
        public String Name { get; set; }
        public int Number { get; set; }

        public List<StaffSymbol> Symbols { get; set; }

        public Staff()
        {
            Symbols = new List<StaffSymbol>();
        }
    }
}
