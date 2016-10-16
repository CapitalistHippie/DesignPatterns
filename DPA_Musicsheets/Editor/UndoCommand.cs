using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

    public class UndoShortcutCommandConstructor : ShortcutCommandConstructorBase
    {
        public override CommandBase Handle(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && keyEventArgs.Key == Key.Y)
                return new UndoCommand(receiver);
            return base.Handle(receiver, keyEventArgs);
        }
    }
}
