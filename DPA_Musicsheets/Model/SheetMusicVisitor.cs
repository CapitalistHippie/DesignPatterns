using PSAMControlLibrary;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace DPA_Musicsheets.Model
{
    public class SheetMusicVisitor
    {
        private Staff staff;
        private IncipitViewerWPF incipitViewer;
        private StackPanel scoreStackPanel;
        private double width;

        private Clef currentClef;
        private TimeSignature currentTimeSignature;

        private double amountEights; // 15 eights = approximately 1 staff
        private NoteStemDirection noteStemDirection;
        private int amountNoteBeams;
        private bool continueNoteBeam;

        private Dictionary<ClefType, PSAMControlLibrary.ClefType> psamClefTypeConversionDictionary;
        private Dictionary<StaffSymbolDuration, double> eightsConversionDurationDictionary;

        public SheetMusicVisitor(Staff staff, IncipitViewerWPF incipitViewer, StackPanel scoreStackPanel, double width)
        {
            this.staff = staff;
            this.incipitViewer = incipitViewer;
            this.scoreStackPanel = scoreStackPanel;
            
            amountEights = 0;

            currentClef = new Clef();
            currentClef.Type = ClefType.G; // default

            psamClefTypeConversionDictionary = new Dictionary<ClefType, PSAMControlLibrary.ClefType> {
                { ClefType.C, PSAMControlLibrary.ClefType.CClef},
                { ClefType.F, PSAMControlLibrary.ClefType.FClef},
                { ClefType.G, PSAMControlLibrary.ClefType.GClef},
            };

            eightsConversionDurationDictionary = new Dictionary<StaffSymbolDuration, double> {
                { StaffSymbolDuration.EIGTH, 1 },
                { StaffSymbolDuration.QUARTER, 1.25 },
                { StaffSymbolDuration.HALF, 2 },
                { StaffSymbolDuration.WHOLE, 4 },
                { StaffSymbolDuration.SIXTEENTH, 1 },
                { StaffSymbolDuration.THIRTY_SECOND, 1 },
                { StaffSymbolDuration.SIXTY_FOURTH, 1 },
                { StaffSymbolDuration.HUNDRED_TWENTY_EIGHTH, 1 },
            };
        }

        public void CheckIfNewStaffNeeded()
        {
            if (amountEights >= 45)
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
            //NoteTieType noteTieType = NoteTieType.None;
            NoteBeamType noteBeamType = GetNoteBeamType(note, index);
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


                    // Original plan for chords, hmm...
                    //if (nextNote.StartTime != 0 && nextNote.StartTime == currentNote.StartTime)
                    //{
                    //    chord = true;
                    //}
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
    }
}
