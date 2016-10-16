using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DPA_Musicsheets.Editor
{
    class OpenFileCommand : CommandBase
    {
        private static ScoreBuilders.ScoreBuilder scoreBuilder = new ScoreBuilders.ScoreBuilder();

        public string selectedFilePath;

        // Constructor
        public OpenFileCommand(Receiver receiver) :
            base(receiver)
        {
        }

        public override void Execute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "All files (*.*)|*.*|Midi Files (*.mid)|*.mid|LilyPond Files (*.ly)|*.ly" };
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;

                string fileExtension = Path.GetExtension(openFileDialog.FileName);

                if (fileExtension == ".ly")
                {
                    receiver.OpenFile(openFileDialog.FileName);
                }
            }
        }
    }

    public class OpenFileShortcutCommandConstructor : ShortcutCommandConstructorBase
    {
        public override CommandBase Handle(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && keyEventArgs.Key == Key.O)
                return new OpenFileCommand(receiver);
            return base.Handle(receiver, keyEventArgs);
        }
    }
}
