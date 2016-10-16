using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DPA_Musicsheets.Editor
{
    public class Receiver
    {
        public delegate void TextChangedEvent();

        private TabControl              tabControl;
        private DispatcherTimer         textChangedTimer;
        private string                  filePath;
        private List<TextChangedEvent>  textChangedEvents   = new List<TextChangedEvent>();

        private TextBox GetActiveTabTextBox()
        {
            TabItem tabItem = tabControl.SelectedItem as TabItem;
            if (tabItem == null)
                return null;
            return tabItem.Content as TextBox;
        }

        private void OnActiveTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
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

            foreach (var @event in textChangedEvents)
            {
                @event.Invoke();
            }
        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            textChangedTimer.Stop();

            TextBox activeTextBox = GetActiveTabTextBox();
            if (activeTextBox != null)
            {
                int index = tabControl.SelectedIndex + 1;
                for (int i = tabControl.SelectedIndex + 1; i < tabControl.Items.Count; i++)
                {
                    tabControl.Items.RemoveAt(index);
                }
                ActivateTab(AddTab(activeTextBox.Text));
            }
        }

        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;

            if (e.RemovedItems.Count != 0)
            {
                TextBox removedTextBox = (e.RemovedItems[0] as TabItem).Content as TextBox;
                removedTextBox.TextChanged -= OnActiveTextBoxTextChanged;
            }

            TextBox textBox = GetActiveTabTextBox();
            if (textBox != null)
                textBox.TextChanged += OnActiveTextBoxTextChanged;
        }

        public Receiver(TabControl tabControl)
        {
            this.tabControl = tabControl;
            this.tabControl.SelectionChanged += OnTabControlSelectionChanged;
        }

        public int AddTab(string content)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = DateTime.Now.ToString("HH:mm:ss");
            TextBox textBox = new TextBox();
            tabItem.Content = textBox;
            textBox.Text = content;
            textBox.AcceptsReturn = true;
            textBox.AcceptsTab = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            return tabControl.Items.Add(tabItem);
        }

        public void ActivateTab(int index)
        {
            TextBox activeTextBox = GetActiveTabTextBox();
            if (activeTextBox != null)
                activeTextBox.TextChanged -= OnActiveTextBoxTextChanged;

            tabControl.SelectedIndex = index;

            activeTextBox = GetActiveTabTextBox();
            activeTextBox.TextChanged += OnActiveTextBoxTextChanged;
        }

        public void Undo()
        {
            TextBox activeBookmarkTextBox = GetActiveTabTextBox();
            if (activeBookmarkTextBox != null)
                activeBookmarkTextBox.Undo();
        }

        public void Redo()
        {
            TextBox activeTextBox = GetActiveTabTextBox();
            if (activeTextBox != null)
                activeTextBox.Redo();
        }

        public void WriteAtCaret(string text)
        {
            TextBox activeTextBox = GetActiveTabTextBox();
            if (activeTextBox != null)
                activeTextBox.Text.Insert(activeTextBox.CaretIndex, text);
        }

        public string GetContent()
        {
            TextBox activeTextBox = GetActiveTabTextBox();
            if (activeTextBox != null)
                return activeTextBox.Text;
            return "";
        }

        public void SetContent(string content)
        {
            TextBox activeTextBox = GetActiveTabTextBox();
            if (activeTextBox != null)
                activeTextBox.Text = content;
        }

        public void Clear()
        {
            tabControl.Items.Clear();
            filePath = "";
        }

        public void OpenFile(string filePath)
        {
            Clear();

            this.filePath = filePath;

            string fileContent = File.ReadAllText(filePath);
            ActivateTab(AddTab(fileContent));
        }

        public string GetFilePath()
        {
            return filePath;
        }

        public void AddTextChangedEvent(TextChangedEvent @event)
        {
            textChangedEvents.Add(@event);
        }
    }
}
