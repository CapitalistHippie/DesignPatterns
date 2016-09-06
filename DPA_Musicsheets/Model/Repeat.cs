using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public enum RepeatType
    {
        FORWARD,
        BACKWARD
    }

    public class Repeat : StaffSymbol
    {
        public RepeatType Type { get; set; }
    }
}
