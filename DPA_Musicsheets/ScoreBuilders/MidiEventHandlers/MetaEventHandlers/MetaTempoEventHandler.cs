using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MidiEventHandlers
{
    public class MetaTempoEventHandler : IMetaEventHandler
    {
        public void Handle(Score score, Staff activeStaff, MidiEvent midiEvent, int trackIndex)
        {
            byte[] bytes = midiEvent.MidiMessage.GetBytes();

            Tempo tempo = new Tempo();

            // Calculate the tempo in microseconds per beat.
            int msPerBeatTempo = (bytes[0] & 0xff) << 16 | (bytes[1] & 0xff) << 8 | (bytes[2] & 0xff);

            // Lets bring that down to beats per minute shall we?
            tempo.BeatsPerMinute = 60000000 / msPerBeatTempo;

            activeStaff.AddSymbol(tempo);
        }
    }
}
