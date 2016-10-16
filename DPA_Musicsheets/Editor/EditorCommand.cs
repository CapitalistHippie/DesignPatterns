using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

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
        private TextBox editor;

        public Receiver(TextBox editor)
        {
            this.editor = editor;
        }

        public void Undo()
        {
            editor.Undo();
        }

        public void Redo()
        {
            editor.Redo();
        }

        public void WriteAtCaret(string text)
        {
            editor.Text.Insert(editor.CaretIndex, text);
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

    public class CommandFactory
    {
        private static CommandFactory instance;

        private CommandFactory()
        {
        }

        public static CommandFactory GetInstance()
        {
            if (instance == null)
                instance = new CommandFactory();
            return instance;
        }

        public CommandBase Construct(KeyEventArgs keyEventArgs)
        {
            // See who can handle constructing a command from this keyevent (chain of responsibility).
            //throw new NotImplementedException();
            return null;
        }
    }
}
