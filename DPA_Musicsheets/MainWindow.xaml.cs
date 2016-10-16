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
        private OutputDevice                    outputDevice            = new OutputDevice(0);
        private MidiPlayer                      player;

        private IScoreBuilder                   scoreBuilder            = new ScoreBuilder();
        private IScoreBuilder                   lilyPondScoreBuilder    = new LilyPondScoreBuilder();

        private Model.Score                     currentScore            = null;

        // Editor command pattern stuff
        Editor.Receiver                         editorReceiver;
        Editor.Invoker                          editorInvoker;

        public MainWindow()
        {
            InitializeComponent();

            editorReceiver = new Editor.Receiver(editorTabControl);
            editorInvoker = new Editor.Invoker(editorReceiver);

            editorReceiver.AddTextChangedEvent(OnEditorTextChanged);
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
            editorReceiver.Clear();

            Editor.OpenFileCommand command = new Editor.OpenFileCommand(editorReceiver);

            editorInvoker.Invoke(command);

            // If nothing was opened it was not a lilypond file. In that case, throw it through the builder.
            if (editorReceiver.GetFilePath() == null || editorReceiver.GetFilePath() == "")
            {
                filePathTextBox.Text = command.selectedFilePath;

                currentScore = scoreBuilder.BuildScoreFromFile(filePathTextBox.Text);

                // If still nothing it was an invalid/unsupported file/file type.
                if (currentScore == null)
                {
                    MessageBox.Show("Invalid or unsupported file/file type.");
                    return;
                }
            }
            else
            {
                filePathTextBox.Text = editorReceiver.GetFilePath();

                currentScore = lilyPondScoreBuilder.BuildScoreFromString(editorReceiver.GetContent());
            }

            FillScoreViewer(currentScore);
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
                FillScoreViewer(currentScore);
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

        private void OnEditorTextChanged()
        {
            try
            {
                currentScore = lilyPondScoreBuilder.BuildScoreFromString(editorReceiver.GetContent());
                FillScoreViewer(currentScore);
            }
            catch (Exception e)
            {

            }
        }
    }
}
