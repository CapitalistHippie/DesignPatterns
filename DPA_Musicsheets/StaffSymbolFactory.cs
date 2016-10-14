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
        //public void FindChordsAndFixThem(Model.Score score) {
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
        //}

        //private void FixChordSequence(List<Note> chord, Staff staff, int index, Tempo tempo, TimeSignature timeSignature)
        //{
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
        //}

        internal StaffSymbol ConstructRest(MidiEvent midiEvent)
        {
            throw new NotImplementedException();
        }
    }
}
