using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public class MetaMessageHandler : IMessageTypeHandler
    {
        private Dictionary<MetaType, IMetaTypeHandler> metaTypeDictionary;

        public MetaMessageHandler()
        {
            metaTypeDictionary = new Dictionary<MetaType, IMetaTypeHandler> {
                { MetaType.TrackName        , new MetaTrackNameHandler()        },
                { MetaType.InstrumentName   , new MetaInstrumentNameHandler()   },
                { MetaType.Tempo            , new MetaTempoHandler()            },   
                { MetaType.TimeSignature    , new MetaTimeSignatureHandler()    },
            };
        }

        public void Execute(Sanford.Multimedia.Midi.MidiEvent midiEvent, Model.Staff staff, double newBar, int ticksPerBeat, int index)
        {
            var metaMessage = midiEvent.MidiMessage as MetaMessage;
            
            if(metaTypeDictionary.ContainsKey(metaMessage.MetaType)) {
                metaTypeDictionary[metaMessage.MetaType].Execute(staff, metaMessage, index);
            }
        }
    }
}
