﻿using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.MessageTypeHandlers
{
    public class ChannelMessageHandler : IMessageTypeHandler
    {
        public void Execute(MidiEvent midiEvent, Staff staff, double newBar, int ticksPerBeat, TimeSignature timeSignature, int index)
        {
            var channelMessage = midiEvent.MidiMessage as ChannelMessage;

            int keyCode = channelMessage.Data1;

            // Note already exists, setNoteDuration
            if (StaffSymbolFactory.Instance.ContainsNoteKey(keyCode) && (channelMessage.Data2 == 0 || channelMessage.Command == ChannelCommand.NoteOff))
            {
                double noteDuration = StaffSymbolFactory.Instance.SetNoteDuration(keyCode, midiEvent, ticksPerBeat, timeSignature);
                if (midiEvent.AbsoluteTicks >= newBar) // New Bar Line
                {
                    staff.Symbols.Add(new Barline());
                    newBar += ticksPerBeat * 4 * ((double)timeSignature.Measure / (double)timeSignature.NumberOfBeats);
                }
            }
            // Create new Note
            else if (channelMessage.Command == ChannelCommand.NoteOn && channelMessage.Data2 > 0)
            {
                if (midiEvent.DeltaTicks > 0) // Found a rest -> construct rest symbol
                {
                    StaffSymbol rest = StaffSymbolFactory.Instance.ConstructRest(midiEvent, ticksPerBeat, timeSignature);
                    staff.Symbols.Add(rest); // TODO
                    if (midiEvent.AbsoluteTicks >= newBar) // New Bar Line
                    {
                        staff.Symbols.Add(new Barline());
                        newBar += ticksPerBeat * 4 * ((double)timeSignature.Measure / (double)timeSignature.NumberOfBeats);
                    }
                }

                StaffSymbol note = StaffSymbolFactory.Instance.ConstructNote(keyCode, midiEvent);
                if (note != null)
                {
                    staff.Symbols.Add(note);
                }
                else
                {
                    Console.WriteLine("Error: Null note");
                }
            }
        }
    }
}
