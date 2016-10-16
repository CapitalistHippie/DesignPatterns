using DPA_Musicsheets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Savepoints
{
    public class Savepoint
    {
        public Score score { get; private set; }
        public string lilyPondEditorText { get; private set; }

        public Savepoint OlderSavepoint { get; set; }
        public Savepoint NewerSavepoint { get; set; }
        
        public Savepoint(Score score, string lilyPondEditorText)
        {
            this.score = score;
            this.lilyPondEditorText = lilyPondEditorText;
        }
    }
}
