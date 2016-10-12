using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public class MetaTimeSignatureHandler : IMetaTypeHandler
    {
        public void Execute(Staff staff, MetaMessage metaMessage, int index)
        {
            //if (index == 0) // Control Track
            //{
            //    if (firstTimeSignature)
            //    {
            //        timeSignature = (TimeSignature)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
            //        staff.Symbols.Add(timeSignature);
            //        firstTimeSignature = false;
            //    }
            //    else skip these frigging false timeSignatures disrupting time and space
            //}
            //else
            //{
            //    timeSignature = (TimeSignature)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
            //    staff.Symbols.Add(timeSignature);
            //    firstTimeSignature = false;
            //}

            //throw new NotImplementedException();
        }
    }
}
