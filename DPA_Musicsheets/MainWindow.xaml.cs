using DPA_Musicsheets.ScoreBuilders;
using Microsoft.Win32;
using PSAMControlLibrary;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DPA_Musicsheets
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // De OutputDevice is een midi device of het midikanaal van je PC.
        // Hierop gaan we audio streamen.
        // DeviceID 0 is je audio van je PC zelf.
        private OutputDevice                    outputDevice    = new OutputDevice(0);
        private MidiPlayer                      player;
        private DispatcherTimer                 textChangedTimer;

        private IScoreBuilder                   scoreBuilder    = new ScoreBuilder();
        private Model.Score                     currentScore;

        // Editor command pattern stuff
        Editor.Receiver                          editorReceiver;
        Editor.Invoker                           editorInvoker;

        public MainWindow()
        {
            InitializeComponent();

            editorReceiver = new Editor.Receiver(editorTextBox);
            editorInvoker = new Editor.Invoker(editorReceiver);
        }

        private void FillScoreViewer(Model.Score score)
        {
            sheetTabControl.Items.Clear();

            double width = sheetTabControl.ActualWidth - 75;

            if (width > 0)
            {
                for (int i = 0; i < score.GetAmountOfStaves(); i++)
                {
                    Model.Staff staff = score.GetStaff(i);

                    ScrollViewer scrollViewer = new ScrollViewer();
                    StackPanel scoreStackPanel = new StackPanel(); // TODO fix width

                    scrollViewer.Content = scoreStackPanel;

                    PSAMWPFControlLibrary.IncipitViewerWPF incipitViewer = new PSAMWPFControlLibrary.IncipitViewerWPF();

                    Thickness margin = incipitViewer.Margin;
                    margin.Top += 50;
                    incipitViewer.Margin = margin;
                    incipitViewer.Width = width;

                    scoreStackPanel.Children.Add(incipitViewer);

                    Model.ScoreVisitor smVisitor = new Model.ScoreVisitor(staff, incipitViewer, scoreStackPanel, width);

                    for (int j = 0; j < staff.Symbols.Count; j++)
                    {
                        Model.StaffSymbol symbol = staff.Symbols[j];
                        smVisitor.CheckIfNewStaffNeeded();
                        symbol.Accept(smVisitor, j);
                    }

                    TabItem tab = new TabItem();
                    tab.Header = staff.StaffName;
                    tab.Content = scrollViewer;
                    sheetTabControl.Items.Add(tab);
                }
            }

            sheetTabControl.SelectedIndex = 0;
        }

        private void OnOpenButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Midi Files(.mid)|*.mid|LilyPond Files(.ly)|*.ly" };
            if (openFileDialog.ShowDialog() == true)
            {
                // Show the file path in the text box.
                filePathTextBox.Text = openFileDialog.FileName;
                Model.Score score = scoreBuilder.BuildScoreFromFile(filePathTextBox.Text);

                if (score == null)
                {
                    MessageBox.Show("Unsupported file type.");
                    return;
                }

                string fileExtension = System.IO.Path.GetExtension(filePathTextBox.Text);

                if (fileExtension == ".ly")
                {
                    string fileText = File.ReadAllText(filePathTextBox.Text);
                    editorTextBox.Text = fileText;
                }

                FillScoreViewer(score);
                currentScore = score;
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            outputDevice.Close();
            if (player != null)
            {
                player.Dispose();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (currentScore != null)
            {
                FillScoreViewer(currentScore); // so dirteh :')
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Editor.CommandBase command = Editor.CommandFactory.GetInstance().Construct(editorReceiver, e);
            if (command != null)
                editorInvoker.Invoke(command);
        }

        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (player != null)
            {
                player.Dispose();
            }

            player = new MidiPlayer(outputDevice);
            player.Play(filePathTextBox.Text);
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            if (player != null)
                player.Dispose();
        }

        private void OnEditorUndoButtonClick(object sender, RoutedEventArgs e)
        {
            editorInvoker.Invoke(new Editor.UndoCommand(editorReceiver));
        }

        private void OnEditorRedoButtonClick(object sender, RoutedEventArgs e)
        {
            editorInvoker.Invoke(new Editor.RedoCommand(editorReceiver));
        }

        private void OnEditorSaveButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnEditorSaveAsButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnEditorTextBoxTextChanged(object sender, TextChangedEventArgs e)
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
        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            textChangedTimer.Stop();
            if (editorTextBox.Text != null)
            {
                currentScore = scoreBuilder.BuildScoreFromString(editorTextBox.Text); // temp
                FillScoreViewer(currentScore);
            }
        }
    }
}
