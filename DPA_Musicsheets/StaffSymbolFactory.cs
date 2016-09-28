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
        private Dictionary<int, StaffSymbolDuration> durationDictionary;
        private Dictionary<StaffSymbolDuration, PSAMControlLibrary.MusicalSymbolDuration> psamConvertDictionary;
        private Dictionary<int, String> keycodeDictionary;
        private Dictionary<int, Note> keyNoteMap;
        

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
            durationDictionary = new Dictionary<int, StaffSymbolDuration>
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

            // Get the note duration and length.
            double percentageOfBeatNote = (double)deltaTicks / ticksPerBeat;
            double percentageOfWholeNote = percentageOfBeatNote * (1d / timeSignature.Measure);

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
            // if noteDuration = -1 throw error

            double noteLeft = percentageOfWholeNote % noteDuration; // TODO do something with this
            
            int convertDuration = (int)(1d / noteDuration);
            note.Duration = StaffSymbolFactory.Instance.GetDuration(convertDuration);
            
            keyNoteMap.Remove(keyCode);

            if (note == null)
            {
                Console.WriteLine("fuuuu");
            }

            return realDuration;
            //staff.Symbols.Add(note); //Temporary Cheat
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

        public StaffSymbolDuration GetDuration(int duration)
        {
            if (durationDictionary.ContainsKey(duration))
            {
                return durationDictionary[duration];
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
    }
}
