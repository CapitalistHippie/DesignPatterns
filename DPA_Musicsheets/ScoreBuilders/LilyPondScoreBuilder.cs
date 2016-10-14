using DPA_Musicsheets.MidiEventHandlers;
using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders
{
    public class LilyPondScoreBuilder : IScoreBuilder
    {
        public Score BuildScore(string filePath)
        {
            Score score = new Score();
            LilyPondStaffAdapter staff = new LilyPondStaffAdapter();

            string fileText = File.ReadAllText(filePath);
            string[] tokens = fileText.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Default relative note is c.
            string  relativeNote = "c";
            int     defaultOctave = 5;

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

            score.AddStaff(staff);
            return score;
        }
    }
}
