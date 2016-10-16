using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.MidiEventHandlers
{
    // Rests & Notes
    public class ChannelEventHandler : IEventHandler, IScoreObserver, IStaffObserver
    {
        private Score score;
        private Staff activeStaff;
        private TimeSignature activeTimeSignature;

        private int ticksPerBeat;
        private double newBar;
        private bool firstTimeSignature = true;

        private Dictionary<int, Note> keyNoteMap = new Dictionary<int, Note>();
        private Dictionary<int, string> keyCodeDictionary = new Dictionary<int, string>
            {
                {0, "C"},
                {1, "C#"},
                {2, "D"},
                {3, "D#"},
                {4, "E"},
                {5, "F"},
                {6, "F#"},
                {7, "G"},
                {8, "G#"},
                {9, "A"},
                {10, "A#"},
                {11, "B"},
            };

        public ChannelEventHandler(Score score, Sequence midiSequence)
        {
            this.score = score;
            score.AddObserver(this);

            ticksPerBeat = midiSequence.Division;
        }

        public void Handle(MidiEvent midiEvent, int trackIndex)
        {
            var channelMessage = midiEvent.MidiMessage as ChannelMessage;

            int keyCode = channelMessage.Data1;

            // Note already exists, setNoteDuration
            if (keyNoteMap.ContainsKey(keyCode) && (channelMessage.Data2 == 0 || channelMessage.Command == ChannelCommand.NoteOff))
            {
                double noteDuration = SetNoteDuration(keyCode, midiEvent, ticksPerBeat, activeTimeSignature);
                if (midiEvent.AbsoluteTicks >= newBar) // New Bar Line
                {
                    activeStaff.AddSymbol(new Barline());
                    newBar += ticksPerBeat * 4 * ((double)activeTimeSignature.Measure / activeTimeSignature.NumberOfBeats);
                }
            }
            // Create new Note
            else if (channelMessage.Command == ChannelCommand.NoteOn && channelMessage.Data2 > 0)
            {
                if (midiEvent.DeltaTicks > 0) // Found a rest -> construct rest symbol
                {
                    StaffSymbol rest = ConstructRest(midiEvent, ticksPerBeat, activeTimeSignature);
                    activeStaff.AddSymbol(rest); // TODO
                    if (midiEvent.AbsoluteTicks >= newBar) // New Bar Line
                    {
                        activeStaff.AddSymbol(new Barline());
                        newBar += ticksPerBeat * 4 * ((double)activeTimeSignature.Measure / activeTimeSignature.NumberOfBeats);
                    }
                }

                StaffSymbol note = ConstructNote(keyCode, midiEvent);
                if (note != null)
                {
                    activeStaff.AddSymbol(note);
                }
                else
                {
                    Console.WriteLine("Error: Null note");
                }
            }
        }

        

        public StaffSymbol ConstructRest(MidiEvent midiEvent, int ticksPerBeat, TimeSignature timeSignature)
        {
            Rest rest = new Rest();
            Note emptyNote = new Note();

            double duration = GetDoubleNoteDuration(emptyNote, ticksPerBeat, midiEvent.DeltaTicks, timeSignature);

            rest.Duration = emptyNote.Duration;
            rest.NumberOfDots = emptyNote.NumberOfDots;

            return rest;
        }

        public StaffSymbol ConstructNote(int keyCode, MidiEvent midiEvent)
        {
            if (!keyNoteMap.ContainsKey(keyCode))
            {
                Note note = new Note();
                note.StartTime = midiEvent.AbsoluteTicks;

                int keyCodeStep = keyCode % 12;
                int octave = keyCode / 12;

                note.Step = keyCodeStep;
                note.Octave = octave;

                // Get Note Alter (Sharps)
                int alter = 0;
                if (keyCodeDictionary[keyCodeStep].Contains("#"))
                {
                    alter++;
                    note.StepString = keyCodeDictionary[keyCodeStep - 1];
                }
                else
                {
                    note.StepString = keyCodeDictionary[keyCodeStep];
                }
                note.Alter = alter;

                keyNoteMap.Add(keyCode, note);

                return note;
            }
            else
            {
                Console.WriteLine("Need solution! Maybe ignore this one because it's part of the same note? Check Absolute time");
                return null;
            }
        }

        private double GetDoubleNoteDuration(Note note, int ticksPerBeat, int deltaTicks, TimeSignature timeSignature)
        {
            // Get the note duration and length.
            double percentageOfBeatNote = (double)deltaTicks / ticksPerBeat;
            double percentageOfWholeNote = percentageOfBeatNote * (1d / 4);

            double noteDuration = -1;
            double realDuration = -1;

            // Find the first note with the appropriate duration that fits as closely as possible
            for (int noteLength = 128; noteLength >= 1; noteLength /= 2)
            {
                double absoluteNoteLength = (1.0 / noteLength);

                if (percentageOfWholeNote <= absoluteNoteLength)
                {
                    noteDuration = absoluteNoteLength;
                    if (percentageOfWholeNote <= absoluteNoteLength / 2 * 1.5)
                    {
                        realDuration = absoluteNoteLength / 2 * 1.5; // note with dot
                        noteDuration = absoluteNoteLength / 2;
                        note.NumberOfDots = 1;
                    }
                    else
                    {
                        realDuration = noteDuration;
                    }
                    break;
                }
            }
            if (noteDuration != -1) //temp
            {
                //double noteLeft = percentageOfWholeNote % noteDuration; // TODO do something with this

                int convertDuration = (int)(1d / noteDuration);
                note.Duration = StaffSymbolFactory.Instance.GetStaffSymbolDuration(convertDuration);

                //keyNoteMap.Remove(keyCode);

                if (note == null)
                {
                    Console.WriteLine("Oh-oh.");
                }
                return realDuration;
            }
            else
            {
                return 0; //temp
            }
        }

        private double SetNoteDuration(int keyCode, MidiEvent midiEvent, int ticksPerBeat, TimeSignature timeSignature)
        {
            Note note = keyNoteMap[keyCode];
            int deltaTicks = midiEvent.AbsoluteTicks - note.StartTime;
            double realDuration = GetDoubleNoteDuration(note, ticksPerBeat, deltaTicks, timeSignature);

            if (realDuration > 0)
            {
                keyNoteMap.Remove(keyCode); //hmmm, a chance the note doesn't get deleted
            }
            else
            {
                Console.WriteLine("That's a problem."); // throw error?
                return 0; // temp something
            }

            return realDuration;
        }

        

        public void OnStaffAdded(Staff staff)
        {
            if (activeStaff != null)
                activeStaff.RemoveObserver(this);
            activeStaff = staff;
            activeStaff.AddObserver(this);
        }

        public void OnSymbolAdded<T>(T symbol) where T : StaffSymbol
        {
            if (typeof(T) == typeof(TimeSignature))
            {
                activeTimeSignature = symbol as TimeSignature;
                if (firstTimeSignature)
                {
                    newBar = ticksPerBeat * 4 * ((double)activeTimeSignature.Measure / activeTimeSignature.NumberOfBeats);
                    firstTimeSignature = false;
                }
            }
        }
    }
}
