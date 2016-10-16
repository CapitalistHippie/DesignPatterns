using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MidiEventHandlers
{
    public class MetaTimeSignatureEventHandler : IMetaEventHandler
    {
        public void Handle(Score score, Staff activeStaff, MidiEvent midiEvent, int trackIndex)
        {
            byte[] bytes = midiEvent.MidiMessage.GetBytes();

            TimeSignature timeSignature = new TimeSignature();
            timeSignature.Measure = bytes[0];
            timeSignature.NumberOfBeats = (int)Math.Pow(2, bytes[1]);

            Clef hardcodedClef = new Clef();
            hardcodedClef.Type = ClefType.G; // default
            activeStaff.AddSymbol(hardcodedClef);

            activeStaff.AddSymbol(timeSignature);
        }
    }
}
