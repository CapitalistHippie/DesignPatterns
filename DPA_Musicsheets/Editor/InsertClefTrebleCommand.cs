using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DPA_Musicsheets.Editor
{
    class InsertTrebleClefCommand : CommandBase
    {
        // Constructor
        public InsertTrebleClefCommand(Receiver receiver) :
            base(receiver)
        {
        }

        public override void Execute()
        {
            receiver.WriteAtCaret("\\clef treble");
        }
    }

    public class InsertTrebleClefShortcutCommandConstructor : ShortcutCommandConstructorBase
    {
        public override CommandBase Handle(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.C))
                return new InsertTrebleClefCommand(receiver);
            return base.Handle(receiver, keyEventArgs);
        }
    }
}
