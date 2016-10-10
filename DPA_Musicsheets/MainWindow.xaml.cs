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
        private OutputDevice                    outputDevice = new OutputDevice(0);
        private MidiPlayer                      player;
        public ObservableCollection<MidiTrack>  MidiTracks { get; private set; }

        public MainWindow()
        {
            this.MidiTracks = new ObservableCollection<MidiTrack>();

            InitializeComponent();
            DataContext = MidiTracks;
            
        }

        private void FillPSAMViewer(Model.Score score)
        {
            ContentSheetControl.Items.Clear();

            Model.TimeSignature currentTimeSignature = null;

            foreach(Model.Staff staff in score.Staves) {
                ScrollViewer scrollViewer = new ScrollViewer();
                StackPanel scoreStackPanel = new StackPanel(); // TODO fix width
                
                scrollViewer.Content = scoreStackPanel;

                PSAMWPFControlLibrary.IncipitViewerWPF incipitViewer = new PSAMWPFControlLibrary.IncipitViewerWPF();

                Thickness margin = incipitViewer.Margin;
                margin.Top += 50;
                incipitViewer.Margin = margin;
                incipitViewer.Width = ContentSheetControl.ActualWidth;

                scoreStackPanel.Children.Add(incipitViewer);

                TabItem tab = new TabItem();
                tab.Header = staff.StaffName;
                tab.Content = scrollViewer;
                ContentSheetControl.Items.Add(tab);

                if (currentTimeSignature != null)
                {
                    incipitViewer.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                    incipitViewer.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, (uint)currentTimeSignature.Measure, (uint)currentTimeSignature.NumberOfBeats));
                }

                

                int index = 1;

                bool continueNoteBeam = false;
                int amountNoteBeams = 1;

                NoteStemDirection noteStemDirection = NoteStemDirection.Up; // default

                for (int i = 0; i < staff.Symbols.Count; i++)
                {
                    Model.StaffSymbol symbol = staff.Symbols[i];
                    if (index >= 6)
                    {
                        index = 1;
                        incipitViewer = new PSAMWPFControlLibrary.IncipitViewerWPF();
                        incipitViewer.Width = ContentSheetControl.ActualWidth;

                        scoreStackPanel.Children.Add(incipitViewer);
                        if (currentTimeSignature != null)
                        {
                            incipitViewer.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                            incipitViewer.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, (uint)currentTimeSignature.Measure, (uint)currentTimeSignature.NumberOfBeats));
                        }

                    }
                    if (symbol is Model.Note) // TODO Visitor pattern
                    {
                        var currentNote = symbol as Model.Note; // Visitor pattern instead

                        NoteTieType noteTieType = NoteTieType.None; // TODO later
                        NoteBeamType noteBeamType = NoteBeamType.Single; // default
                        
                        bool chord = false; // IsChord

                        if (!continueNoteBeam)
                        {
                            if (currentNote.Octave - 1 >= 5)
                            {
                                noteStemDirection = NoteStemDirection.Down;
                            }
                            else
                            {
                                noteStemDirection = NoteStemDirection.Up;
                            }
                        }



                        Model.StaffSymbol nextSymbol = staff.Symbols[i+1];

                        if (nextSymbol is Model.Note)
                        {
                            var nextNote = nextSymbol as Model.Note;

                            if (continueNoteBeam)
                            {
                                if (amountNoteBeams >= 3)
                                {
                                    noteBeamType = NoteBeamType.End;
                                    amountNoteBeams = 1;
                                    continueNoteBeam = false;
                                }
                                else if (nextNote.Duration == currentNote.Duration)
                                {
                                    noteBeamType = NoteBeamType.Continue;
                                    amountNoteBeams++;
                                }
                                else
                                {
                                    noteBeamType = NoteBeamType.End;
                                    amountNoteBeams = 1;
                                    continueNoteBeam = false;
                                }
                            }
                            else
                            {
                                if (nextNote.Duration == currentNote.Duration && 
                                    StaffSymbolFactory.Instance.GetIntDuration(currentNote.Duration) > 4) // everything with a shorter duration than quarter notes uses beams, quarter and higher don't
                                {
                                    noteBeamType = NoteBeamType.Start;
                                    continueNoteBeam = true;
                                }
                            }

                            if (nextNote.StartTime == currentNote.StartTime)
                            {
                                chord = true;
                            }
                        }
                        else if (continueNoteBeam)
                        {
                            noteBeamType = NoteBeamType.End;
                            amountNoteBeams = 1;
                            continueNoteBeam = false;
                        }

                        //staff.
                        if (currentNote.Duration != 0) //TODO temp, dirteh (to avoid errors for now)
                        {
                            //incipitViewer.AddMusicalSymbol(new Note(currentNote.StepString, currentNote.Alter, currentNote.Octave - 1, StaffSymbolFactory.Instance.GetMusicalSymbolDuration(currentNote.Duration), noteStemDirection, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = currentNote.NumberOfDots, IsChordElement = chord });
                            incipitViewer.AddMusicalSymbol(new Note(currentNote.StepString, currentNote.Alter, currentNote.Octave - 1, StaffSymbolFactory.Instance.GetMusicalSymbolDuration(currentNote.Duration), noteStemDirection, NoteTieType.None, new List<NoteBeamType>() { noteBeamType }) { NumberOfDots = currentNote.NumberOfDots, IsChordElement = chord });
                        }

                    }
                    if (symbol is Model.Barline) // Visitor pattern
                    {
                        incipitViewer.AddMusicalSymbol(new Barline());
                        index++;
                    }
                    if (symbol is Model.TimeSignature) // Visitor pattern
                    {

                        currentTimeSignature = symbol as Model.TimeSignature;
                        incipitViewer.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                        incipitViewer.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, (uint) currentTimeSignature.Measure, (uint) currentTimeSignature.NumberOfBeats));
                    }
                }
            }

            ScrollViewer scrollViewerTest = new ScrollViewer();
            StackPanel scoreStackPanelTest = new StackPanel(); // TODO fix width

            scrollViewerTest.Content = scoreStackPanelTest;

            PSAMWPFControlLibrary.IncipitViewerWPF staffTest = new PSAMWPFControlLibrary.IncipitViewerWPF();

            Thickness marginTest = staffTest.Margin;
            marginTest.Top += 50;
            staffTest.Margin = marginTest;
            staffTest.Width = ContentSheetControl.ActualWidth;

            scoreStackPanelTest.Children.Add(staffTest);

            TabItem tabTest = new TabItem();
            tabTest.Header = "Test";
            tabTest.Content = scrollViewerTest;
            ContentSheetControl.Items.Add(tabTest);

            staffTest.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
            staffTest.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, 4, 4));

            staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start }));
            staffTest.AddMusicalSymbol(new Note("C", 1, 5, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.End }));
            staffTest.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.Start, new List<NoteBeamType>() { NoteBeamType.End }));
            staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start }));
            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }));
            //staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue }));
            staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.End }));

            staffTest.AddMusicalSymbol(new Note("C", 0, 3, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            staffTest.AddMusicalSymbol(new Note("C", 0, 2, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            staffTest.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = false });

            staffTest.AddMusicalSymbol(new Barline());

            staffTest.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.Stop, new List<NoteBeamType>() { NoteBeamType.Single }));
            staffTest.AddMusicalSymbol(new Note("E", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.Start, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 1 });
            staffTest.AddMusicalSymbol(new Barline());

            staffTest.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            staffTest.AddMusicalSymbol(
                new Note("E", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            staffTest.AddMusicalSymbol(
                new Note("G", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            staffTest.AddMusicalSymbol(new Barline());

        }

        private void OnOpenButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Midi Files(.mid)|*.mid|LilyPond Files(.ly)|*.ly" };
            if (openFileDialog.ShowDialog() == true)
            {
                // Show the file path in the text box.
                FilePathTextBox.Text = openFileDialog.FileName;

                string extension = System.IO.Path.GetExtension(openFileDialog.FileName);

                switch (extension)
                {
                    case ".mid":
                        // Show the MIDI tracks content.
                        ShowMidiTracks(MidiReader.ReadMidi(FilePathTextBox.Text));

                        // Load score and display for our viewing pleasure.
                        Model.Score score = ScoreBuilder.Instance.BuildScoreFromMidi(FilePathTextBox.Text);
                        FillPSAMViewer(score);
                        break;
                    case ".ly":
                        // Build a score from the LilyPond.
                        ScoreBuilder.Instance.BuildScoreFromLilyPond(FilePathTextBox.Text);
                        break;
                }
            }
        }

        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (player != null)
            {
                player.Dispose();
            }

            player = new MidiPlayer(outputDevice);
            player.Play(FilePathTextBox.Text);
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            if (player != null)
                player.Dispose();
        }

        private void ShowMidiTracks(IEnumerable<MidiTrack> midiTracks)
        {
            MidiTracks.Clear();
            foreach (var midiTrack in midiTracks)
            {
                MidiTracks.Add(midiTrack);
            }

            ContentTabControl.SelectedIndex = 0;
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
            //foreach (PSAMWPFControlLibrary.IncipitViewerWPF staff in ScoreStackPanel.Children)
            //{
            //    staff.Width = ScoreStackPanel.ActualWidth;
            //}
        }
    }
}
