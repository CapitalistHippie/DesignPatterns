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
        private static readonly int                         DEFAULT_RELATIVE_OCTAVE = 4;
        private static readonly Dictionary<string, string>  NOTE_CONVERSION_TABLE   = new Dictionary<string, string>
        {
            { "c",   "C" },
            { "cis", "C#" },
            { "d",   "D" },
            { "dis", "D#" },
            { "e",   "E" },
            { "f",   "F" },
            { "fis", "F#" },
            { "g",   "G" },
            { "gis", "G#" },
            { "a",   "A" },
            { "as",  "A#" },
            { "b",   "B" },
        };

        private static readonly Dictionary<char, int> NOTE_NUMBER_CONVERSION_TABLE = new Dictionary<char, int>
        {
            { 'c', 1 },
            { 'd', 2 },
            { 'e', 3 },
            { 'f', 4 },
            { 'g', 5 },
            { 'a', 6 },
            { 'b', 7 },
        };

        private int OctaveOffset(char relativeStep, char step)
        {
            // ASCII to number
            int relativeStepNumber = NOTE_NUMBER_CONVERSION_TABLE[relativeStep];
            int stepNumber = NOTE_NUMBER_CONVERSION_TABLE[step];

            int difference = relativeStepNumber - stepNumber;

            if (difference < -3)
                return -1;
            if (difference > 3)
                return 1;
            return 0;
        }

        public Score BuildScoreFromString(string lilyPondText)
        {
            return BuildScore(lilyPondText);
        }

        public Score BuildScoreFromFile(string filePath)
        {
            string fileText = File.ReadAllText(filePath);
            return BuildScore(fileText);
        }

        private Score BuildScore(string lilyPondText)
        {
            Score score = new Score();
            LilyPondStaffAdapter staff = new LilyPondStaffAdapter();

            string[] tokens = lilyPondText.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Default relative step is c.
            string relativeStep = "c";
            int relativeOctave = DEFAULT_RELATIVE_OCTAVE;

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                if (token.StartsWith("\\"))
                {
                    string key = token.Substring(1);
                    switch (key)
                    {
                        case "relative":
                            string newRelative = tokens[++i];
                            relativeStep = Regex.Match(newRelative, "[a-z]+").Value;
                            relativeOctave = DEFAULT_RELATIVE_OCTAVE + (1 * newRelative.Count(x => x == '\''));
                            relativeOctave -= (1 * newRelative.Count(x => x == ','));
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
                    string step = Regex.Match(token, "[a-z]+").Value;

                    // Get the note duration.
                    int noteDuration = Int32.Parse(Regex.Match(token, "[0-9]+").Value);
                    StaffSymbolDuration duration = StaffSymbolFactory.Instance.GetStaffSymbolDuration(noteDuration);

                    // Check if it is a rest.
                    if (step == "r")
                    {
                        Rest rest = new Rest();
                        rest.Duration = duration;
                        staff.Symbols.Add(rest);
                    }
                    else
                    {
                        string stepString = NOTE_CONVERSION_TABLE[step];

                        // Get the octave.
                        int octave = relativeOctave + OctaveOffset(relativeStep[0], step[0]);
                        octave += (1 * token.Count(x => x == '\''));
                        octave -= (1 * token.Count(x => x == ','));
                        relativeStep = step;
                        relativeOctave = octave;

                        Note note = new Note();
                        note.Duration = duration;
                        note.Octave = octave;
                        note.StepString = stepString;
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
