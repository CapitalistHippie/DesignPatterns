using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public class MetaTrackNameHandler : IMetaTypeHandler
    {
        public void Execute(Staff staff, MetaMessage metaMessage, int index)
        {
            staff.StaffName = index + " " + Encoding.Default.GetString(metaMessage.GetBytes());
        }
    }
}
