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
        private Dictionary<StaffSymbolDuration, PSAMControlLibrary.MusicalSymbolDuration> psamConvertDictionary;
        private Dictionary<int, String> keycodeDictionary;
        public Dictionary<string, string> lilyPondNoteDictionary;
        private Dictionary<StaffSymbolDuration, int> intDurationDictionary; //staffSymbolDurationDictionary reversed


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

            intDurationDictionary = staffSymbolDurationDictionary.ToDictionary(x => x.Value, x => x.Key); //staffSymbolDurationDictionary reversed
        }

        public StaffSymbolDuration GetStaffSymbolDuration(int duration)
        {
            if (staffSymbolDurationDictionary.ContainsKey(duration))
            {
                return staffSymbolDurationDictionary[duration];
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

        public int GetIntDuration(StaffSymbolDuration duration)
        {
            if (intDurationDictionary.ContainsKey(duration))
            {
                return intDurationDictionary[duration];
            }

            throw new KeyNotFoundException();
        }
    }
}
