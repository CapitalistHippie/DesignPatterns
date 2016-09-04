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

    public class Repeat : Symbol
    {
        public RepeatType Type { get; set; }
    }
}
