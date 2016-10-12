using DPA_Musicsheets.MessageTypeHandlers;
using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DPA_Musicsheets
{
    public class ScoreBuilder
    {
        private static ScoreBuilder instance;
        private Dictionary<MessageType, IMessageTypeHandler> messageTypeDictionary;
        
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
            messageTypeDictionary = new Dictionary<MessageType, IMessageTypeHandler>
            {
                { MessageType.Channel   , new ChannelMessageHandler()   },
                { MessageType.Meta      , new MetaMessageHandler()      },
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

            bool firstTimeSignature = true;

            //Create a new staff for each track in the sequence.
            for (int i = 0; i < midiSequence.Count; i++)
            {
                Staff staff = new Staff();
                staff.StaffNumber = i;

                Track track = midiSequence[i];

                double newBar = 0;
                if (timeSignature != null)
                {
                    newBar += ticksPerBeat * 4 * ((double)timeSignature.Measure / (double)timeSignature.NumberOfBeats);
                }

                Dictionary<int, Note> keyNoteMap = new Dictionary<int, Note>();

                foreach (var midiEvent in track.Iterator())
                {
                    if (messageTypeDictionary.ContainsKey(midiEvent.MidiMessage.MessageType)) {
                        messageTypeDictionary[midiEvent.MidiMessage.MessageType].Execute(midiEvent, staff, newBar, ticksPerBeat, timeSignature, i);
                    }
                    
                    //switch (midiEvent.MidiMessage.MessageType)
                    //{
                        
                    //    case MessageType.Meta:
                    //        var metaMessage = midiEvent.MidiMessage as MetaMessage;
                    //        switch (metaMessage.MetaType)
                    //        {
                    //            case MetaType.TrackName:
                    //                staff.StaffName = i + " " + Encoding.Default.GetString(metaMessage.GetBytes());
                    //                break;
                    //            case MetaType.InstrumentName:
                    //                staff.InstrumentName = Encoding.Default.GetString(metaMessage.GetBytes());
                    //                break;
                    //            case MetaType.Tempo:
                    //                tempo = (Tempo)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
                    //                staff.Symbols.Add(tempo);
                    //                break;
                    //            case MetaType.TimeSignature:
                    //                if (i == 0) // Control Track
                    //                {
                    //                    if (firstTimeSignature)
                    //                    {
                    //                        timeSignature = (TimeSignature)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
                    //                        staff.Symbols.Add(timeSignature);
                    //                        firstTimeSignature = false;
                    //                    }
                    //                    // else skip these frigging false timeSignatures disrupting time and space
                    //                }
                    //                else
                    //                {
                    //                    timeSignature = (TimeSignature)StaffSymbolFactory.Instance.ConstructSymbol(metaMessage);
                    //                    staff.Symbols.Add(timeSignature);
                    //                    firstTimeSignature = false;
                    //                }
                    //                break;
                    //            default:
                    //                staff.Symbols.Add(StaffSymbolFactory.Instance.ConstructSymbol(metaMessage));
                    //                break;
                    //        }
                    //        break;
                    //}
                }
                if (staff.StaffName == null)
                {
                    staff.StaffName = i.ToString();
                }
                score.Staves.Add(staff);
            }

            return score;
        }

        public Score BuildScoreFromLilyPond(String filePath)
        {
            Score score = new Score();
            LilyPondStaffAdapter staff = new LilyPondStaffAdapter();

            string fileText = File.ReadAllText(filePath);
            string[] tokens = fileText.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Default relative note is c.
            string  relativeNote = "c";
            int     defaultOctave = 6;

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                if (token.StartsWith("\\"))
                {
                    string key = token.Substring(1);
                    switch (key)
                    {
                        case "relative":
                            string relativeNoteValue = tokens[++i];
                            relativeNote = relativeNoteValue;
                            break;
                        case "clef":
                            string cleffValue = tokens[++i];
                            staff.AddClef(cleffValue);
                            break;
                        case "time":
                            string timeValue = tokens[++i];
                            staff.AddTimeSignature(timeValue);
                            break;
                        case "tempo":
                            string tempoValue = tokens[++i];
                            staff.AddTempo(tempoValue);
                            break;
                        case "repeat":
                            string repeatType = tokens[++i];
                            switch (repeatType)
                            {
                                case "volta":
                                    int repeatCount = int.Parse(tokens[++i]);
                                    //staff.Symbols.Add(new Repeat { Type = RepeatType.FORWARD });
                                    break;
                            }
                            break;
                        case "alternative":
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (token)
                    {
                        case "|":
                            staff.Symbols.Add(new Barline());
                            continue;
                        case "{":
                            continue;
                        case "}":
                            continue;
                    }

                    // If it's not any of the previous we've found a note. Lets parse it.

                    // Get the note type (g, fis etc...)
                    string noteType = Regex.Match(token, "[a-z]+").Value;

                    // Get the note duration.
                    int noteDuration = Int32.Parse(Regex.Match(token, "[0-9]+").Value);
                    StaffSymbolDuration duration = StaffSymbolFactory.Instance.GetStaffSymbolDuration(noteDuration);

                    // Check if it is a rest.
                    if (noteType == "r")
                    {
                        Rest rest = new Rest();
                        rest.Duration = duration;
                        staff.Symbols.Add(rest);
                    }
                    else
                    {
                        // Get the octave.
                        int octave = defaultOctave;
                        octave += (1 * token.Count(x => x == '\''));
                        octave -= (1 * token.Count(x => x == ','));

                        Note note = new Note();
                        note.Duration = duration;
                        note.Octave = octave;
                        note.StepString = StaffSymbolFactory.Instance.lilyPondNoteDictionary[noteType];
                        note.NumberOfDots = token.Count(x => x == '.');

                        staff.Symbols.Add(note);
                    }
                }
            }

            score.Staves.Add(staff);
            return score;
        }
    }
}
