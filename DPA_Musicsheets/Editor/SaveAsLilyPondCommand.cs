using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DPA_Musicsheets.Editor
{
    class SaveAsLilyPondCommand : CommandBase
    {
        private string content;

        // Constructor
        public SaveAsLilyPondCommand(Receiver receiver) :
            base(receiver)
        {
            content = receiver.GetContent();
        }

        public override void Execute()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "LilyPond Files (*.ly)|*.ly";

            if (saveFileDialog.ShowDialog() == true)
            {
                StreamWriter stream = new StreamWriter(saveFileDialog.FileName);
                stream.Write(content);
                stream.Close();
            }
        }
    }

    public class SaveAsLilyPondShortcutCommandConstructor : ShortcutCommandConstructorBase
    {
        public override CommandBase Handle(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && keyEventArgs.Key == Key.S)
                return new SaveAsLilyPondCommand(receiver);
            return base.Handle(receiver, keyEventArgs);
        }
    }
}
