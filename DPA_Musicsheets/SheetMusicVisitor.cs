using PSAMControlLibrary;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace DPA_Musicsheets.Model
{
    public class ScoreVisitor
    {
        private Staff staff;
        private IncipitViewerWPF incipitViewer;
        private StackPanel scoreStackPanel;
        private double width;

        private Clef currentClef;
        private TimeSignature currentTimeSignature;

        private double amountEights; // 15 eights = approximately 1 staff
        private double maxAmountOfEights;

        private NoteStemDirection noteStemDirection;
        private int amountNoteBeams;
        private bool continueNoteBeam;

        private static Dictionary<ClefType, PSAMControlLibrary.ClefType> psamClefTypeConversionDictionary = new Dictionary<ClefType, PSAMControlLibrary.ClefType>
        {
            { ClefType.C, PSAMControlLibrary.ClefType.CClef },
            { ClefType.F, PSAMControlLibrary.ClefType.FClef },
            { ClefType.G, PSAMControlLibrary.ClefType.GClef },
        };

        private static Dictionary<StaffSymbolDuration, double> eightsConversionDurationDictionary = new Dictionary<StaffSymbolDuration, double>
        {
            { StaffSymbolDuration.EIGTH, 1 },
            { StaffSymbolDuration.QUARTER, 1.25 },
            { StaffSymbolDuration.HALF, 2 },
            { StaffSymbolDuration.WHOLE, 4 },
            { StaffSymbolDuration.SIXTEENTH, 1 },
            { StaffSymbolDuration.THIRTY_SECOND, 1 },
            { StaffSymbolDuration.SIXTY_FOURTH, 1 },
            { StaffSymbolDuration.HUNDRED_TWENTY_EIGHTH, 1 },
        };        

        public ScoreVisitor(Staff staff, IncipitViewerWPF incipitViewer, StackPanel scoreStackPanel, double width)
        {
            this.staff = staff;
            this.incipitViewer = incipitViewer;
            this.scoreStackPanel = scoreStackPanel;
            this.width = width;
            
            amountEights = 0;
            maxAmountOfEights = (45d / 683 * 45d) * (width / 45d); //config, no more than 45 on a bar

            currentClef = new Clef();
            currentClef.Type = ClefType.G; // default            
        }

        public void CheckIfNewStaffNeeded()
        {
            if (amountEights >= maxAmountOfEights)
            {
                CreateNewStaff();
                amountEights = 0;
            }
        }

        private void CreateNewStaff()
        {
            incipitViewer = new PSAMWPFControlLibrary.IncipitViewerWPF();
            incipitViewer.Width = width;

            scoreStackPanel.Children.Add(incipitViewer);
            if (currentClef != null)
            {
                Visit(currentClef);
            }
        }

        private double GetDuration(StaffSymbolDuration duration, int amountOfDots)
        {
            double doubleDuration = eightsConversionDurationDictionary[duration];
            if (amountOfDots > 0)
            {
                doubleDuration *= 2;
            }

            return doubleDuration;
        }

        public void Visit(Barline barline)
        {
            incipitViewer.AddMusicalSymbol(new PSAMControlLibrary.Barline());
            amountEights += 3;
        }


        public void Visit(Clef clef) // TODO
        {
            if(psamClefTypeConversionDictionary.ContainsKey(clef.Type)) {
                incipitViewer.AddMusicalSymbol(new PSAMControlLibrary.Clef(psamClefTypeConversionDictionary[clef.Type], 2)); //hardcoded -> fix
            }
            else
            {
                throw new Exception(); // Clef is supposed to exist
            }
            amountEights++;
        }

        public void Visit(Note note, int index)
        {
            bool chord = false; // IsChord

            if (!continueNoteBeam) // TODO doesn't look very pretty
            {
                if (note.Octave - 1 >= 5)
                {
                    noteStemDirection = NoteStemDirection.Down;
                }
                else
                {
                    noteStemDirection = NoteStemDirection.Up;
                }
            }

            //NoteTieType noteTieType = NoteTieType.None;
            NoteBeamType noteBeamType = GetNoteBeamType(note, index);
            
            incipitViewer.AddMusicalSymbol(new PSAMControlLibrary.Note(
                                            note.StepString, 
                                            note.Alter, 
                                            note.Octave - 1, 
                                            StaffSymbolFactory.Instance.GetMusicalSymbolDuration(note.Duration), 
                                            noteStemDirection, 
                                            NoteTieType.None, 
                                            new List<NoteBeamType>() { noteBeamType }) 
                                            { NumberOfDots = note.NumberOfDots, 
                                                IsChordElement = chord });

            amountEights += GetDuration(note.Duration, note.NumberOfDots);
        }

        public void Visit(Repeat repeat)
        {
            throw new NotImplementedException();
        }

        public void Visit(Rest rest)
        {
            incipitViewer.AddMusicalSymbol(new PSAMControlLibrary.Rest(StaffSymbolFactory.Instance.GetMusicalSymbolDuration(rest.Duration)));
            amountEights += GetDuration(rest.Duration, rest.NumberOfDots);
        }

        public void Visit(Tempo tempo) //TODO
        {
            TextBox textBox = new TextBox();
            textBox.Text = "Tempo = " + tempo.BeatsPerMinute;

            //throw new NotImplementedException();
        }

        public void Visit(TimeSignature timeSignature)
        {
            this.currentTimeSignature = timeSignature;

            //visit(currentClef); // temp derp
            incipitViewer.AddMusicalSymbol(new PSAMControlLibrary.TimeSignature(TimeSignatureType.Numbers, (uint)currentTimeSignature.Measure, (uint)currentTimeSignature.NumberOfBeats));
            amountEights++;

            //throw new NotImplementedException();
        }
        
        private NoteBeamType GetNoteBeamType(Note currentNote, int index)
        {
            NoteBeamType noteBeamType = NoteBeamType.Single; // default

            if (staff.Symbols.Count > index + 1)
            {
                Model.StaffSymbol nextSymbol = staff.Symbols[index + 1];

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
                        else if (currentNote.Duration == StaffSymbolDuration.EIGTH && nextNote.Duration == StaffSymbolDuration.EIGTH &&
                            amountEights + GetDuration(currentNote.Duration, currentNote.NumberOfDots) + GetDuration(nextNote.Duration, nextNote.NumberOfDots) < maxAmountOfEights)
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
                        if (currentNote.Duration == StaffSymbolDuration.EIGTH && nextNote.Duration == StaffSymbolDuration.EIGTH &&
                            StaffSymbolFactory.Instance.GetIntDuration(currentNote.Duration) > 4) // everything with a shorter duration than quarter notes uses beams, quarter and higher don't
                        {
                            noteBeamType = NoteBeamType.Start;
                            continueNoteBeam = true;
                        }
                    }
                }
                else if (continueNoteBeam)
                {
                    noteBeamType = NoteBeamType.End;
                    amountNoteBeams = 1;
                    continueNoteBeam = false;
                }
            }

            return noteBeamType;
        }

        // Not properly tested yet
        public void FindChordsAndFixThem(Model.Score score) {
        //    TimeSignature currentTimeSignature = null;
        //    Tempo tempo = null;
        //    int recordedStartTime = 0;
        //    List<Note> chord = new List<Note>();

        //    foreach (Staff staff in score.Staves)
        //    {
        //        for (int index = 0; index < staff.Symbols.Count; index++) // Potentially visitor pattern
        //        {
        //            Model.StaffSymbol symbol = staff.Symbols[index];
        //            if (symbol is Model.Note)
        //            {
        //                var currentNote = symbol as Note; // Visitor pattern instead
        //                if (index > 0 && recordedStartTime == currentNote.StartTime)
        //                {
        //                    if(!chord.Contains(staff.Symbols[index - 1])) 
        //                    {
        //                        chord.Add(staff.Symbols[index - 1] as Note);
        //                    }
        //                    chord.Add(currentNote);
        //                }
        //                else if (chord.Count > 2)
        //                {
        //                    FixChordSequence(chord, staff, index, tempo, currentTimeSignature);
        //                    chord.Clear();
        //                }
        //            }
        //            else if (symbol is TimeSignature)
        //            {
        //                currentTimeSignature = symbol as TimeSignature;
        //            }
        //            else if (symbol is Tempo)
        //            {
        //                tempo = symbol as Tempo;
        //            }
        //            else if (chord.Count > 2)
        //            {
        //                FixChordSequence(chord, staff, index, tempo, currentTimeSignature);
        //                chord.Clear();
        //            }
        //        }
        //    }
        }

        // Not properly tested yet
        private void FixChordSequence(List<Note> chord, Staff staff, int index, Tempo tempo, TimeSignature timeSignature)
        {
        //    int indexClone = index;

        //    // startTime + deltaTicks = nextNote after chord
        //    bool loop = true;
        //    Note nextNote = null;
        //    while (loop) {
        //        StaffSymbol symbol = staff.Symbols[index];
        //        if (symbol is Model.Note) {
        //            nextNote = symbol as Note;
        //            loop = false;
        //        }
        //        index++;
        //    }

        //    int deltaTicks = nextNote.StartTime - chord[0].StartTime;
        //    Note noteDurationClone = new Note();

        //    // deltaTicks = duration -> lastNote in chord
        //    GetDoubleNoteDuration(noteDurationClone, tempo.BeatsPerMinute, deltaTicks, timeSignature);

        //    if (noteDurationClone.Duration == 0)
        //    {
        //        throw new Exception(); // this is not supposed to happen
        //    }

        //    //Get correct sequence for chord (only need to find which note to swap with the last note in the chord)
        //    for (int i = 0; i < chord.Count; i++)
        //    {
        //        if (chord[i].Duration == noteDurationClone.Duration)
        //        {
        //            // found last note in Chord
        //            Note lastChordNote = chord[chord.Count - 1];
        //            if (lastChordNote != chord[i]) // swap notes
        //            {
        //                chord[chord.Count - 1] = chord[i];
        //                chord[i] = lastChordNote;
        //            }
        //            break;
        //        }
        //    }

        //    for (int i = indexClone - chord.Count - 1, j = 0; i < indexClone; i++, j++)
        //    {
        //        staff.Symbols[i] = chord[j]; // swap correct sequence of the chord into the Symbols list
        //    }
        }
    }
}
