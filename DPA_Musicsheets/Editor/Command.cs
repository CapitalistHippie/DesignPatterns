using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DPA_Musicsheets.Editor
{
    public abstract class CommandBase
    {
        protected Receiver receiver;

        // Constructor
        public CommandBase(Receiver receiver)
        {
            this.receiver = receiver;
        }

        public abstract void Execute();
    }

    public class Receiver
    {
        private TextBox textBox;

        public Receiver(TextBox textBox = null)
        {
            this.textBox = textBox;
        }

        public void SetTextBox(TextBox textBox)
        {
            this.textBox = textBox;
        }

        public void Undo()
        {
            textBox.Undo();
        }

        public void Redo()
        {
            textBox.Redo();
        }

        public void WriteAtCaret(string text)
        {
            textBox.Text.Insert(textBox.CaretIndex, text);
        }

        public string GetContent()
        {
            return textBox.Text;
        }
    }

    public class Invoker
    {
        private Receiver receiver;

        public Invoker(Receiver receiver)
        {
            this.receiver = receiver;
        }

        public void Invoke(CommandBase command)
        {
            command.Execute();
        }
    }
}
