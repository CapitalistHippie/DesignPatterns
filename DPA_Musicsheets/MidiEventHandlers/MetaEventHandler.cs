using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.MidiEventHandlers
{
    public class MetaEventHandler : IEventHandler, IScoreObserver
    {
        private Score score;
        private Staff activeStaff;

        private Dictionary<MetaType, IMetaEventHandler> metaEventHandlers = new Dictionary<MetaType, IMetaEventHandler>
        {
            { MetaType.TrackName,       new MetaTrackNameEventHandler() },
            { MetaType.InstrumentName,  new MetaInstrumentEventNameHandler() },
            { MetaType.Tempo,           new MetaTempoEventHandler() },
            { MetaType.TimeSignature,   new MetaTimeSignatureEventHandler() },
        };

        public MetaEventHandler(Score score)
        {
            this.score = score;
            score.AddObserver(this);
        }

        public void Handle(MidiEvent midiEvent, int trackIndex)
        {
            var metaMessage = midiEvent.MidiMessage as MetaMessage;
            
            if (metaEventHandlers.ContainsKey(metaMessage.MetaType))
                metaEventHandlers[metaMessage.MetaType].Handle(score, activeStaff, midiEvent, trackIndex);
        }

        public void OnStaffAdded(Staff staff)
        {
            activeStaff = staff;
        }
    }
}
