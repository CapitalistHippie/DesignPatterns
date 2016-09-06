using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Score
    {
        public List<Staff> Staves { get; set; }

        public Score()
        {
            Staves = new List<Staff>();
        }
    }
}
