using DPA_Musicsheets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Savepoints
{
    public class SavepointKeeper
    {
        private Savepoint currentSavepoint;

        public SavepointKeeper()
        {

        }

        public void SetNewSavePoint(Score score, string lilyPondEditorText)
        {
            Savepoint newSavepoint = new Savepoint(score, lilyPondEditorText);
            
            if (currentSavepoint != null)
            {
                newSavepoint.OlderSavepoint = currentSavepoint;
                currentSavepoint.NewerSavepoint = newSavepoint;
            }

            currentSavepoint = newSavepoint;
        }

        public Savepoint GetOlderSavepoint()
        {
            currentSavepoint = currentSavepoint.OlderSavepoint;
            return currentSavepoint;
        }

        public Savepoint GetNewerSavepoint()
        {
            currentSavepoint = currentSavepoint.NewerSavepoint;
            return currentSavepoint;
        }
    }
}
