using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public interface IMessageTypeHandler
    {
        void Execute(MidiEvent midiEvent, Staff staff, double newBar, int ticksPerBeat, TimeSignature timeSignature, int index);
    }
}
