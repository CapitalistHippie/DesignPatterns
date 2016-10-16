using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Editor
{
    class UndoCommand : CommandBase
    {
        // Constructor
        public UndoCommand(Receiver receiver) :
            base(receiver)
        {
        }

        public override void Execute()
        {
            receiver.Undo();
        }
    }
}
