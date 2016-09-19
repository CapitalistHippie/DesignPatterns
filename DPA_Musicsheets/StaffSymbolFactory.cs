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
        }

        public StaffSymbol ConstructSymbol(MetaMessage metaMessage)
        {
            byte[] bytes = metaMessage.GetBytes();

            switch (metaMessage.MetaType)
            {
                case MetaType.TimeSignature:
                    TimeSignature timeSignature = new TimeSignature();
                    timeSignature.Measure = bytes[0];
                    timeSignature.NumberOfBeats = (int)(1 / Math.Pow(bytes[1], -2));

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

        public StaffSymbol ConstructNote()
        {
            throw new NotImplementedException();
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
