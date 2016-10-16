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
        public ObservableCollection<MidiTrack>  MidiTracks { get; private set; }

        private IScoreBuilder                   scoreBuilder    = new ScoreBuilder();
        private Model.Score                     currentScore;

        // Editor command pattern stuff
        Editor.Receiver                          editorReceiver;
        Editor.Invoker                           editorInvoker;

        public MainWindow()
        {
            this.MidiTracks = new ObservableCollection<MidiTrack>();

            InitializeComponent();
            DataContext = MidiTracks;

            editorReceiver = new Editor.Receiver(editorTextBox);
            editorInvoker = new Editor.Invoker(editorReceiver);
        }

        private void FillScoreViewer(Model.Score score)
        {
            sheetTabControl.Items.Clear();

            double width = sheetTabControl.ActualWidth - 75;

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

            sheetTabControl.Items.Add(ReturnTestTabItem());
        }

        private TabItem ReturnTestTabItem()
        {
            // Setup
            ScrollViewer scrollViewerTest = new ScrollViewer();
            StackPanel scoreStackPanelTest = new StackPanel(); // TODO fix width

            scrollViewerTest.Content = scoreStackPanelTest;

            PSAMWPFControlLibrary.IncipitViewerWPF staffTest = new PSAMWPFControlLibrary.IncipitViewerWPF();

            Thickness marginTest = staffTest.Margin;
            marginTest.Top += 50;
            staffTest.Margin = marginTest;
            staffTest.Width = sheetTabControl.ActualWidth;

            scoreStackPanelTest.Children.Add(staffTest);

            // Test Symbols

            staffTest.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
            staffTest.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, 4, 4));

            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start }));
            //staffTest.AddMusicalSymbol(new Note("C", 1, 5, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.End }));
            //staffTest.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }));

            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("C", 1, 5, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.Start, new List<NoteBeamType>() { NoteBeamType.Single }));

            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }));
            ////staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }));
            ////staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }));
            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }));

            //staffTest.AddMusicalSymbol(new Note("C", 0, 3, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staffTest.AddMusicalSymbol(new Note("C", 0, 2, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = false });

            //staffTest.AddMusicalSymbol(new Barline());

            //staffTest.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.Stop, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("E", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.Start, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 1 });
            //staffTest.AddMusicalSymbol(new Barline());

            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(
            //    new Note("E", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staffTest.AddMusicalSymbol(
            //    new Note("G", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staffTest.AddMusicalSymbol(new Barline());

            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Whole, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 1 });
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 1 });
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 1 });
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start}) { NumberOfDots = 0 });
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            //staffTest.AddMusicalSymbol(new Barline());

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });

            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Whole, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start }));
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.Continue }));
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End, NoteBeamType.End }));

            //staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start }));
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.Continue }));
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End, NoteBeamType.End }));

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d32nd, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start, NoteBeamType.Start }));
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d32nd, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.Continue, NoteBeamType.Continue }));
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d32nd, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End, NoteBeamType.End, NoteBeamType.End }));

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d64th, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d64th, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d64th, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d128th, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d128th, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.d128th, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 0 });

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }) { NumberOfDots = 0 });
            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }) { NumberOfDots = 0 });

            TabItem tabTest = new TabItem();
            tabTest.Header = "Test";
            tabTest.Content = scrollViewerTest;

            return tabTest;
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

                if (System.IO.Path.GetExtension(openFileDialog.FileName) == ".mid")
                    ShowMidiTracks(MidiReader.ReadMidi(filePathTextBox.Text));                
            }
        }

        private void ShowMidiTracks(IEnumerable<MidiTrack> midiTracks)
        {
            MidiTracks.Clear();
            foreach (var midiTrack in midiTracks)
            {
                MidiTracks.Add(midiTrack);
            }

            contentTabControl.SelectedIndex = 0;
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
            //if (textChangedTimer == null)
            //{
            //    textChangedTimer = new DispatcherTimer();
            //    textChangedTimer.Interval = TimeSpan.FromSeconds(1.5d); // 1.5 Seconds
            //    textChangedTimer.Tick += new EventHandler(OnTimedEvent);
            //}
            //else if (textChangedTimer != null)
            //{
            //    textChangedTimer.Stop();
            //    textChangedTimer.Start();
            //}
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
