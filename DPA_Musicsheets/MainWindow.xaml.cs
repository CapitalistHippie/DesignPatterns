using Microsoft.Win32;
using PSAMControlLibrary;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            staff.ClearMusicalIncipit();

            // Clef = sleutel
            staff.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
            staff.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, 4, 4));
            /* 
                The first argument of Note constructor is a string representing one of the following names of steps: A, B, C, D, E, F, G. 
                The second argument is number of sharps (positive number) or flats (negative number) where 0 means no alteration. 
                The third argument is the number of an octave. 
                The next arguments are: duration of the note, stem direction and type of tie (NoteTieType.None if the note is not tied). 
                The last argument is a list of beams. If the note doesn't have any beams, it must still have that list with just one 
                    element NoteBeamType.Single (even if duration of the note is greater than eighth). 
                    To make it clear how beamlists work, let's try to add a group of two beamed sixteenths and eighth:
                        Note s1 = new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start});
                        Note s2 = new Note("C", 1, 5, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.End });
                        Note e = new Note("D", 0, 5, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.None,new List<NoteBeamType>() { NoteBeamType.End });
                        viewer.AddMusicalSymbol(s1);
                        viewer.AddMusicalSymbol(s2);
                        viewer.AddMusicalSymbol(e); 
            */

            int index = 1;

            foreach (DPA_Musicsheets.Model.StaffSymbol symbol in score.Staves[1].Symbols)
            {
                index++;
                if (index > 10)
                {
                    
                    //staff = new IncipitViewer();
                }
                if (symbol is DPA_Musicsheets.Model.Note)
                {
                    var note = (DPA_Musicsheets.Model.Note)symbol;
                    staff.AddMusicalSymbol(new Note(note.StepString, note.Alter, note.Octave, StaffSymbolFactory.Instance.GetMusicalSymbolDuration(note.Duration), NoteStemDirection.Down, NoteTieType.Stop, new List<NoteBeamType>() { NoteBeamType.Single }));
                }
            }

            //staff.AddMusicalSymbol(new Note("A", 0, 4, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Start, NoteBeamType.Start }));
            //staff.AddMusicalSymbol(new Note("C", 1, 5, MusicalSymbolDuration.Sixteenth, NoteStemDirection.Down, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Continue, NoteBeamType.End }));
            //staff.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Eighth, NoteStemDirection.Down, NoteTieType.Start, new List<NoteBeamType>() { NoteBeamType.End }));
            //staff.AddMusicalSymbol(new Barline());

            //staff.AddMusicalSymbol(new Note("D", 0, 5, MusicalSymbolDuration.Whole, NoteStemDirection.Down, NoteTieType.Stop, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staff.AddMusicalSymbol(new Note("E", 0, 4, MusicalSymbolDuration.Quarter, NoteStemDirection.Up, NoteTieType.Start, new List<NoteBeamType>() { NoteBeamType.Single }) { NumberOfDots = 1 });
            //staff.AddMusicalSymbol(new Barline());

            //staff.AddMusicalSymbol(new Note("C", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
            //staff.AddMusicalSymbol(
            //    new Note("E", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staff.AddMusicalSymbol(
            //    new Note("G", 0, 4, MusicalSymbolDuration.Half, NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }) { IsChordElement = true });
            //staff.AddMusicalSymbol(new Barline());
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
                        // Load the LilyPond.
                        LilyPondBuilder.Instance.BuildLilyPondFromMidi(FilePathTextBox.Text);
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

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outputDevice.Close();
            if (player != null)
            {
                player.Dispose();
            }
        }
    }
}
