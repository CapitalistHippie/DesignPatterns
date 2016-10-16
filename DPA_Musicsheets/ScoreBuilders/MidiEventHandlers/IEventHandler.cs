using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.MidiEventHandlers
{
    public interface IEventHandler
    {
        void Handle(MidiEvent midiEvent, int trackIndex);
    }
}
