using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DPA_Musicsheets.Editor
{
    class SaveAsPdfCommand : CommandBase
    {
        private string content;

        // Constructor
        public SaveAsPdfCommand(Receiver receiver) :
            base(receiver)
        {
            content = receiver.GetContent();
        }

        public override void Execute()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(receiver.GetFilePath());

            if (saveFileDialog.ShowDialog() == true)
            {
                string lilyPondLocation = "C:\\Program Files (x86)\\LilyPond\\usr\\bin\\lilypond.exe";
                string sourceDirectory = "c:\\temp\\";
                string sourceFilePath = sourceDirectory + Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

                // Create the temp dir if it doesn't exist yet.
                Directory.CreateDirectory(sourceDirectory);

                StreamWriter stream = new StreamWriter(sourceFilePath + ".ly");
                stream.Write(content);
                stream.Close();

                var process = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = sourceDirectory,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = String.Format("--pdf \"{0}\"", sourceFilePath + ".ly"),
                        FileName = lilyPondLocation
                    }
                };

                process.Start();
                while (!process.HasExited)
                {
                    /* Wait for exit */
                }

                File.Copy(sourceFilePath + ".pdf", saveFileDialog.FileName, true);
            }
        }
    }

    public class SaveAsPdfShortcutCommandConstructor : ShortcutCommandConstructorBase
    {
        public override CommandBase Handle(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && keyEventArgs.Key == Key.S && Keyboard.IsKeyDown(Key.P))
                return new SaveAsPdfCommand(receiver);
            return base.Handle(receiver, keyEventArgs);
        }
    }
}
