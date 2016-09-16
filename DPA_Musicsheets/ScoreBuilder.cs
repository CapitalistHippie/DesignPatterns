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
        private Dictionary<int, String> keycodeDictionary;

        public static ScoreBuilder Instance
        {
            get
            {
                if (instance == null)
                    instance = new ScoreBuilder();
                return instance;
            }
        }

        private ScoreBuilder()
        {
            keycodeDictionary = new Dictionary<int, string>
            {
                {0, "C"},
                {1, "C#"},
                {2, "D"},
                {3, "D#"},
                {4, "E"},
                {5, "F"},
                {6, "F#"},
                {7, "G"},
                {8, "G#"},
                {9, "A"},
                {10, "A#"},
                {11, "B"},
            };
        }

        public Score BuildScoreFromMidi(String filePath)
        {
            Score score = new Score();
               
            // Read the MIDI sequence.
            var midiSequence = new Sequence();
            midiSequence.Load(filePath);

            int ticksPerBeat = midiSequence.Division;

            Tempo tempo = null;
            TimeSignature timeSignature = null;

            // Create a new staff for each track in the sequence.
            //for (int i = 0; i < midiSequence.Count; i++)
            //{
            //    Staff staff = new Staff();
            //    staff.StaffNumber = i;

            //    Track track = midiSequence[i];

            //    Dictionary<int, Note> keyNoteMap = new Dictionary<int, Note>();

            //    foreach (var midiEvent in track.Iterator())
            //    {
            //        switch (midiEvent.MidiMessage.MessageType)
            //        {
            //            // ChannelMessages zijn de inhoudelijke messages.
            //            case MessageType.Channel:
            //                var channelMessage = midiEvent.MidiMessage as ChannelMessage;

            //                int keyCode = channelMessage.Data1;

            //                if (channelMessage.Command == ChannelCommand.NoteOn)
            //                {
            //                    Note note = new Note();
            //                    note.StartTime = midiEvent.AbsoluteTicks;

            //                    int keyCodeStep = keyCode;
            //                    int octave = 0;
            //                    while (keyCodeStep > 11)
            //                    {
            //                        keyCodeStep -= 12;
            //                        octave++;
            //                    }
            //                    note.Step = keyCodeStep;
            //                    note.Octave = octave;

            //                    // Get Note Alter (Sharps)
            //                    int alter = 0;
            //                    if (keycodeDictionary[keyCodeStep].Contains("#"))
            //                    {
            //                        alter++;
            //                    }
            //                    note.Alter = alter;


            //                    keyNoteMap.Add(keyCode, note);
            //                }
            //                else if (channelMessage.Command == ChannelCommand.NoteOff)
            //                {
            //                    Note note = keyNoteMap[keyCode];

            //                    int deltaTicks = midiEvent.AbsoluteTicks - note.StartTime;

            //                    // Get the note duration and length.
            //                    double noteDuration = (double)deltaTicks / ticksPerBeat;
            //                    double noteLength = noteDuration * (1d / timeSignature.Measure);

            //                    keyNoteMap.Remove(keyCode);
            //                }

            //                break;
            //            case MessageType.SystemExclusive:
            //                break;
            //            case MessageType.SystemCommon:
            //                break;
            //            case MessageType.SystemRealtime:
            //                break;
            //            case MessageType.Meta:
            //                var metaMessage = midiEvent.MidiMessage as MetaMessage;
            //                switch (metaMessage.MetaType)
            //                {
            //                    case MetaType.TrackName:
            //                        staff.StaffName = Encoding.Default.GetString(metaMessage.GetBytes());
            //                        break;
            //                    case MetaType.InstrumentName:
            //                        staff.InstrumentName = Encoding.Default.GetString(metaMessage.GetBytes());
            //                        break;
            //                    case MetaType.Tempo:
            //                        tempo = (Tempo)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
            //                        staff.Symbols.Add(tempo);
            //                        break;
            //                    case MetaType.TimeSignature:
            //                        timeSignature = (TimeSignature)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
            //                        staff.Symbols.Add(timeSignature);
            //                        break;
            //                    default:
            //                        staff.Symbols.Add(StaffSymbolFactory.Instance.ConstructSymbol(metaMessage));
            //                        break;
            //                }
            //                break;
            //        }
            //    }

            //    score.Staves.Add(staff);
            //}

            return score;
        }
    }
}
