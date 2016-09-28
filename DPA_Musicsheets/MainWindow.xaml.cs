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

            foreach(Model.Staff stave in score.Staves) {
                ScrollViewer scrollViewer = new ScrollViewer();
                StackPanel scoreStackPanel = new StackPanel(); // TODO fix width
                
                scrollViewer.Content = scoreStackPanel;

                PSAMWPFControlLibrary.IncipitViewerWPF staff = new PSAMWPFControlLibrary.IncipitViewerWPF();

                

                Thickness margin = staff.Margin;
                margin.Top += 50;
                staff.Margin = margin;
                staff.Width = ContentSheetControl.ActualWidth;

                scoreStackPanel.Children.Add(staff);

                TabItem tab = new TabItem();
                tab.Header = stave.StaffName;
                tab.Content = scrollViewer;
                ContentSheetControl.Items.Add(tab);

                if (currentTimeSignature != null)
                {
                    staff.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                    staff.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, (uint)currentTimeSignature.Measure, (uint)currentTimeSignature.NumberOfBeats));
                }

                int index = 1;

                foreach (Model.StaffSymbol symbol in stave.Symbols)
                {
                    if (index >= 6)
                    {
                        index = 1;
                        staff = new PSAMWPFControlLibrary.IncipitViewerWPF();
                        staff.Width = ContentSheetControl.ActualWidth;

                        scoreStackPanel.Children.Add(staff);
                        if (currentTimeSignature != null)
                        {
                            staff.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                            staff.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, (uint)currentTimeSignature.Measure, (uint)currentTimeSignature.NumberOfBeats));
                        }

                    }
                    if (symbol is Model.Note)
                    {
                        var note = symbol as Model.Note;
                        //staff.
                        if (note.Duration != 0) //TODO temp
                        {
                            if (note.Octave - 1 >= 5)
                            {
                                staff.AddMusicalSymbol(new Note(note.StepString, note.Alter, note.Octave - 1, StaffSymbolFactory.Instance.GetMusicalSymbolDuration(note.Duration), NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = note.NumberOfDots });
                            }
                            else
                            {
                                staff.AddMusicalSymbol(new Note(note.StepString, note.Alter, note.Octave - 1, StaffSymbolFactory.Instance.GetMusicalSymbolDuration(note.Duration), NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = note.NumberOfDots });
                            }
                        }

                    }
                    if (symbol is Model.Barline)
                    {
                        staff.AddMusicalSymbol(new Barline());
                        index++;
                    }
                    if (symbol is Model.TimeSignature)
                    {
                        currentTimeSignature = symbol as Model.TimeSignature;
                        staff.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                        staff.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, (uint) currentTimeSignature.Measure, (uint) currentTimeSignature.NumberOfBeats));
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
