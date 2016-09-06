using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets
{
    public class ScoreBuilder
    {
        private static ScoreBuilder instance;

        public static ScoreBuilder Instance
        {
            get
            {
                if (instance == null)
                    instance = new ScoreBuilder();
                return instance;
            }
        }

        public Score BuildScoreFromMidi(String filePath)
        {
            Score score = new Score();
               
            // Read the MIDI sequence.
            var midiSequence = new Sequence();
            midiSequence.Load(filePath);

            // Create a new staff for each track in the sequence.
            for (int i = 0; i < midiSequence.Count; i++)
            {
                Staff staff = new Staff();
                staff.Number = i;

                Track track = midiSequence[i];

                foreach (var midiEvent in track.Iterator())
                {
                    // Elke messagetype komt ook overeen met een class. Daarom moet elke keer gecast worden.
                    switch (midiEvent.MidiMessage.MessageType)
                    {
                        // ChannelMessages zijn de inhoudelijke messages.
                        case MessageType.Channel:
                            var channelMessage = midiEvent.MidiMessage as ChannelMessage;
                            // Data1: De keycode. 0 = laagste C, 1 = laagste C#, 2 = laagste D etc.
                            // 160 is centrale C op piano.
                            //trackLog.Messages.Add(String.Format("Keycode: {0}, Command: {1}, absolute time: {2}, delta time: {3}"
                            //    , channelMessage.Data1, channelMessage.Command, midiEvent.AbsoluteTicks, midiEvent.DeltaTicks));
                            break;
                        case MessageType.SystemExclusive:
                            break;
                        case MessageType.SystemCommon:
                            break;
                        case MessageType.SystemRealtime:
                            break;
                        // Meta zegt iets over de track zelf.
                        case MessageType.Meta:
                            var metaMessage = midiEvent.MidiMessage as MetaMessage;
                            //trackLog.Messages.Add(GetMetaString(metaMessage));
                            switch (metaMessage.MetaType)
                            {
                                case MetaType.TrackName:
                                    staff.Name = Encoding.Default.GetString(metaMessage.GetBytes());
                                    break;
                                case MetaType.TimeSignature:
                                    byte[] bytes = metaMessage.GetBytes();
                                    TimeSignature timeSignature = new TimeSignature();
                                    timeSignature.Measure = bytes[0];
                                    timeSignature.NumberOfBeats = (int)(1 / Math.Pow(bytes[1], -2));
                                    staff.Symbols.Add(timeSignature);
                                    break;
                            }
                            break;
                        default:
                            //trackLog.Messages.Add(String.Format("MidiEvent {0}, absolute ticks: {1}, deltaTicks: {2}", midiEvent.MidiMessage.MessageType, midiEvent.AbsoluteTicks, midiEvent.DeltaTicks));
                            break;
                    }
                }

                score.Staves.Add(staff);
            }

            return score;
        }
    }
}
