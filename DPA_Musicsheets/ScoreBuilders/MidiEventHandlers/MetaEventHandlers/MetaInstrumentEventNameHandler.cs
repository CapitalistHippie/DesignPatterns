using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MidiEventHandlers
{
    public class MetaInstrumentEventNameHandler : IMetaEventHandler
    {
        public void Handle(Score score, Staff activeStaff, MidiEvent midiEvent, int trackIndex)
        {
            activeStaff.InstrumentName = Encoding.Default.GetString(midiEvent.MidiMessage.GetBytes());
        }
    }
}
