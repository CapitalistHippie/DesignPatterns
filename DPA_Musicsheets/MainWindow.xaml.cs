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
            ScoreStackPanel.Children.Clear();

            PSAMWPFControlLibrary.IncipitViewerWPF staff = new PSAMWPFControlLibrary.IncipitViewerWPF();
            //staff.Margin = staff.Margin.Top(10);
            Thickness margin = staff.Margin;
            margin.Top += 50;
            staff.Margin = margin;
            staff.Width = ScoreStackPanel.ActualWidth;
            staff.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
            staff.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, 4, 4));
            ScoreStackPanel.Children.Add(staff);

            int index = 1;

            foreach (Model.StaffSymbol symbol in score.Staves[1].Symbols)
            {
                index++;
                if (index % 10 == 0)
                {
                    staff = new PSAMWPFControlLibrary.IncipitViewerWPF();
                    staff.Width = ScoreStackPanel.ActualWidth;

                    ScoreStackPanel.Children.Add(staff);
                    staff.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
                    staff.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, 4, 4));
                }
                if (symbol is Model.Note)
                {
                    var note = symbol as Model.Note;
                    staff.AddMusicalSymbol(new Note(note.StepString, note.Alter, note.Octave, StaffSymbolFactory.Instance.GetMusicalSymbolDuration(note.Duration), NoteStemDirection.Up, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single }));
                }
            }
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
            foreach (PSAMWPFControlLibrary.IncipitViewerWPF staff in ScoreStackPanel.Children)
            {
                staff.Width = ScoreStackPanel.ActualWidth;
            }
        }
    }
}
