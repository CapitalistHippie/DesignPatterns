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

        public static StaffSymbolFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new StaffSymbolFactory();
                return instance;
            }
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
    }
}
