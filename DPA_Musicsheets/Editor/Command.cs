using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

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
