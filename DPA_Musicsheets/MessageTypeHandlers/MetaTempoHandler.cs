using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public class MetaTempoHandler : IMetaTypeHandler
    {
        public void Execute(Staff staff, MetaMessage metaMessage, int index)
        {
            staff.AddSymbol(StaffSymbolFactory.Instance.ConstructSymbol(metaMessage));
        }
    }
}
