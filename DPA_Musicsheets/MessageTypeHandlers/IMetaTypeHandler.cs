using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    interface IMetaTypeHandler
    {
        void Execute(Staff staff, MetaMessage metaMessage, int index);
    }
}
