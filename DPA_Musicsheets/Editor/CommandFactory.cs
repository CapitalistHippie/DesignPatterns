using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DPA_Musicsheets.Editor
{
    public class CommandFactory
    {
        private static CommandFactory instance;
        private static ShortcutCommandConstructorBase shortcutCommandConstructor;

        private CommandFactory()
        {
            shortcutCommandConstructor = new UndoShortcutCommandConstructor();
            shortcutCommandConstructor.AddHandler(new RedoShortcutCommandConstructor());
            shortcutCommandConstructor.AddHandler(new SaveAsPdfShortcutCommandConstructor());
            shortcutCommandConstructor.AddHandler(new SaveAsLilyPondShortcutCommandConstructor());
            shortcutCommandConstructor.AddHandler(new OpenFileShortcutCommandConstructor());
            shortcutCommandConstructor.AddHandler(new InsertTrebleClefShortcutCommandConstructor());
        }

        public static CommandFactory GetInstance()
        {
            if (instance == null)
                instance = new CommandFactory();
            return instance;
        }

        public CommandBase Construct(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            // Construct a command by the shortcut chain of responsibility.
            return shortcutCommandConstructor.Handle(receiver, keyEventArgs);
        }
    }

    public abstract class ShortcutCommandConstructorBase
    {
        ShortcutCommandConstructorBase nextHandler;

        public ShortcutCommandConstructorBase(ShortcutCommandConstructorBase nextHandler = null)
        {
            this.nextHandler = nextHandler;
        }

        public virtual CommandBase Handle(Receiver receiver, KeyEventArgs keyEventArgs)
        {
            if (nextHandler != null)
                return nextHandler.Handle(receiver, keyEventArgs);
            return null;
        }

        public void AddHandler(ShortcutCommandConstructorBase handler)
        {
            if (nextHandler == null)
                nextHandler = handler;
            else
                nextHandler.AddHandler(handler);
        }
    }
}
