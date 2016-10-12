using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public class MetaInstrumentNameHandler : IMetaTypeHandler
    {
        public void Execute(Staff staff, MetaMessage metaMessage, int index)
        {
            staff.InstrumentName = Encoding.Default.GetString(metaMessage.GetBytes());
        }
    }
}
