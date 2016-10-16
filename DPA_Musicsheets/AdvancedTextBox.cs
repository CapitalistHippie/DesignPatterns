using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DPA_Musicsheets
{
    public class AdvancedTextBox : TextBox
    {
        private DispatcherTimer textChangedTimer = new DispatcherTimer();

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (textChangedTimer == null)
            {
                textChangedTimer = new DispatcherTimer();
                textChangedTimer.Interval = TimeSpan.FromSeconds(1.5d); // 1.5 Seconds
                textChangedTimer.Tick += new EventHandler(OnTimedEvent);
            }
            else if (textChangedTimer != null)
            {
                textChangedTimer.Stop();
                textChangedTimer.Start();
            }
        }

        //TODO
        private void OnTimedEvent(object source, EventArgs e)
        {
            //textChangedTimer.Stop();
            //if (this.Text != null)
            //{
            //    // save point setten -> savepointkeeper

            //    currentScore = scoreBuilder.BuildScoreFromString(editorTextBox.Text); // temp
            //    FillScoreViewer(currentScore);
            //}
        }
    }
}
