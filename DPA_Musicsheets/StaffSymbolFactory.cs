using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets
{
    public class StaffSymbolFactory
    {
        private static StaffSymbolFactory instance;
        private Dictionary<int, StaffSymbolDuration> staffSymbolDurationDictionary;
        private Dictionary<StaffSymbolDuration, int> intDurationDictionary; //staffSymbolDurationDictionary reversed
        private Dictionary<StaffSymbolDuration, PSAMControlLibrary.MusicalSymbolDuration> psamConvertDictionary;
        private Dictionary<int, String> keycodeDictionary;
        private Dictionary<int, Note> keyNoteMap;
        public Dictionary<string, string> lilyPondNoteDictionary;


        public static StaffSymbolFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new StaffSymbolFactory();
                return instance;
            }
        }

        public StaffSymbolFactory()
        {
            staffSymbolDurationDictionary = new Dictionary<int, StaffSymbolDuration>
            { 
                { 1, StaffSymbolDuration.WHOLE },
                { 2, StaffSymbolDuration.HALF },
                { 4, StaffSymbolDuration.QUARTER },
                { 8, StaffSymbolDuration.EIGTH },
                { 16, StaffSymbolDuration.SIXTEENTH },
                { 32, StaffSymbolDuration.THIRTY_SECOND },
                { 64, StaffSymbolDuration.SIXTY_FOURTH },
                { 128, StaffSymbolDuration.HUNDRED_TWENTY_EIGHTH }
            };
            intDurationDictionary = staffSymbolDurationDictionary.ToDictionary(x => x.Value, x => x.Key); //staffSymbolDurationDictionary reversed

            psamConvertDictionary = new Dictionary<StaffSymbolDuration, PSAMControlLibrary.MusicalSymbolDuration>
            {
                { StaffSymbolDuration.WHOLE, PSAMControlLibrary.MusicalSymbolDuration.Whole },
                { StaffSymbolDuration.HALF, PSAMControlLibrary.MusicalSymbolDuration.Half },
                { StaffSymbolDuration.QUARTER, PSAMControlLibrary.MusicalSymbolDuration.Quarter },
                { StaffSymbolDuration.EIGTH, PSAMControlLibrary.MusicalSymbolDuration.Eighth },
                { StaffSymbolDuration.SIXTEENTH, PSAMControlLibrary.MusicalSymbolDuration.Sixteenth },
                { StaffSymbolDuration.THIRTY_SECOND, PSAMControlLibrary.MusicalSymbolDuration.d32nd },
                { StaffSymbolDuration.SIXTY_FOURTH, PSAMControlLibrary.MusicalSymbolDuration.d64th },
                { StaffSymbolDuration.HUNDRED_TWENTY_EIGHTH, PSAMControlLibrary.MusicalSymbolDuration.d128th }
            };

            keycodeDictionary = new Dictionary<int, string>
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

            lilyPondNoteDictionary = new Dictionary<string, string>
            {
                {"c", "C"},
                {"cis", "C#"},
                {"d", "D"},
                {"dis", "D#"},
                {"e", "E"},
                {"f", "F"},
                {"fis", "F#"},
                {"g", "G"},
                {"gis", "G#"},
                {"a", "A"},
                {"as", "A#"},
                {"b", "B"},
            };

            keyNoteMap = new Dictionary<int, Note>();
        }

        public StaffSymbol ConstructSymbol(MetaMessage metaMessage)
        {
            byte[] bytes = metaMessage.GetBytes();

            switch (metaMessage.MetaType)
            {
                case MetaType.TimeSignature:
                    TimeSignature timeSignature = new TimeSignature();
                    timeSignature.Measure = bytes[0];
                    timeSignature.NumberOfBeats = (int)Math.Pow(2, bytes[1]); // fix

                    return timeSignature;
                case MetaType.Tempo:
                    Tempo tempo = new Tempo();

                    // Calculate the tempo in microseconds per beat.
                    int msPerBeatTempo = (bytes[0] & 0xff) << 16 | (bytes[1] & 0xff) << 8 | (bytes[2] & 0xff);

                    // Lets bring that down to beats per minute shall we?
                    tempo.BeatsPerMinute = 60000000 / msPerBeatTempo;

                    return tempo;
                default:
                    return null;
            }
        }

        public bool ContainsNoteKey(int keyCode)
        {
            return keyNoteMap.ContainsKey(keyCode);
        }

        public double SetNoteDuration(int keyCode, MidiEvent midiEvent, int ticksPerBeat, TimeSignature timeSignature)
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
                double noteLeft = percentageOfWholeNote % noteDuration; // TODO do something with this

                int convertDuration = (int)(1d / noteDuration);
                note.Duration = StaffSymbolFactory.Instance.GetStaffSymbolDuration(convertDuration);

                //keyNoteMap.Remove(keyCode);

                if (note == null)
                {
                    Console.WriteLine("fuuuu");
                }
                return realDuration;
            }
            else
            {
                return 0; //temp
            }
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
                if (keycodeDictionary[keyCodeStep].Contains("#"))
                {
                    alter++;
                    note.StepString = keycodeDictionary[keyCodeStep - 1];
                }
                else
                {
                    note.StepString = keycodeDictionary[keyCodeStep];
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

        public StaffSymbolDuration GetStaffSymbolDuration(int duration)
        {
            if (staffSymbolDurationDictionary.ContainsKey(duration))
            {
                return staffSymbolDurationDictionary[duration];
            }

            throw new KeyNotFoundException();
        }

        public int GetIntDuration(StaffSymbolDuration duration) 
        {
            if (intDurationDictionary.ContainsKey(duration))
            {
                return intDurationDictionary[duration];
            }

            throw new KeyNotFoundException();
        }

        public PSAMControlLibrary.MusicalSymbolDuration GetMusicalSymbolDuration(StaffSymbolDuration staffSymbolDuration)
        {
            if (psamConvertDictionary.ContainsKey(staffSymbolDuration))
            {
                return psamConvertDictionary[staffSymbolDuration];
            }

            throw new KeyNotFoundException();
        }

        // Not properly tested yet
        public void FindChordsAndFixThem(Model.Score score) {
            TimeSignature currentTimeSignature = null;
            Tempo tempo = null;
            int recordedStartTime = 0;
            List<Note> chord = new List<Note>();

            foreach (Staff staff in score.Staves)
            {
                for (int index = 0; index < staff.Symbols.Count; index++) // Potentially visitor pattern
                {
                    Model.StaffSymbol symbol = staff.Symbols[index];
                    if (symbol is Model.Note)
                    {
                        var currentNote = symbol as Note; // Visitor pattern instead
                        if (index > 0 && recordedStartTime == currentNote.StartTime)
                        {
                            if(!chord.Contains(staff.Symbols[index - 1])) 
                            {
                                chord.Add(staff.Symbols[index - 1] as Note);
                            }
                            chord.Add(currentNote);
                        }
                        else if (chord.Count > 2)
                        {
                            FixChordSequence(chord, staff, index, tempo, currentTimeSignature);
                            chord.Clear();
                        }
                    }
                    else if (symbol is TimeSignature)
                    {
                        currentTimeSignature = symbol as TimeSignature;
                    }
                    else if (symbol is Tempo)
                    {
                        tempo = symbol as Tempo;
                    }
                    else if (chord.Count > 2)
                    {
                        FixChordSequence(chord, staff, index, tempo, currentTimeSignature);
                        chord.Clear();
                    }
                }
            }
        }

        private void FixChordSequence(List<Note> chord, Staff staff, int index, Tempo tempo, TimeSignature timeSignature)
        {
            int indexClone = index;

            // startTime + deltaTicks = nextNote after chord
            bool loop = true;
            Note nextNote = null;
            while (loop) {
                StaffSymbol symbol = staff.Symbols[index];
                if (symbol is Model.Note) {
                    nextNote = symbol as Note;
                    loop = false;
                }
                index++;
            }

            int deltaTicks = nextNote.StartTime - chord[0].StartTime;
            Note noteDurationClone = new Note();

            // deltaTicks = duration -> lastNote in chord
            GetDoubleNoteDuration(noteDurationClone, tempo.BeatsPerMinute, deltaTicks, timeSignature);

            if (noteDurationClone.Duration == 0)
            {
                throw new Exception(); // this is not supposed to happen
            }

            //Get correct sequence for chord (only need to find which note to swap with the last note in the chord)
            for (int i = 0; i < chord.Count; i++)
            {
                if (chord[i].Duration == noteDurationClone.Duration)
                {
                    // found last note in Chord
                    Note lastChordNote = chord[chord.Count - 1];
                    if (lastChordNote != chord[i]) // swap notes
                    {
                        chord[chord.Count - 1] = chord[i];
                        chord[i] = lastChordNote;
                    }
                    break;
                }
            }

            for (int i = indexClone - chord.Count - 1, j = 0; i < indexClone; i++, j++)
            {
                staff.Symbols[i] = chord[j]; // swap correct sequence of the chord into the Symbols list
            }
        }
    }
}
