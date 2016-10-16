using DPA_Musicsheets.MidiEventHandlers;
using DPA_Musicsheets.Model;
using DPA_Musicsheets.ScoreBuilders.LilyHandlers;
using DPA_Musicsheets.ScoreBuilders.LilyHandlers.LilyNonTokenHandlers;
using DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers;
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
        
        public Score BuildScoreFromString(string text)
        {
            return BuildScore(text);
        }

        public Score BuildScoreFromFile(string filePath)
        {
            string fileText = File.ReadAllText(filePath);
            return BuildScore(fileText);
        }

        private Score BuildScore(string text)
        {
            Score score = new Score();
            LilyPondStaffAdapter staff = new LilyPondStaffAdapter();

            string[] tokens = text.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Default relative step is c.
            string relativeStep = "c";
            int relativeOctave = DEFAULT_RELATIVE_OCTAVE;

            Dictionary<string, ILilyTokenHandler> lilyTokenHandlers = new Dictionary<string, ILilyTokenHandler>
            {
                { "relative",   new RelativeTokenHandler()      },
                { "clef"    ,   new ClefTokenHandler()          },
                { "time"    ,   new TimeTokenHandler()          },
                { "tempo"   ,   new TempoTokenHandler()         },
                { "repeat"  ,   new RepeatTokenHandler()        },
            };

            Dictionary<string, ILilyNonTokenHandler> lilyNonTokenHandlers = new Dictionary<string, ILilyNonTokenHandler>
            {
                { "|",          new BarlineHandler()            },
                { "{",          new NonTokenHandler()           },
                { "}",          new NonTokenHandler()           },
                { "note",       new NoteHandler()               },
            };

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                RelativeVariablesWrapper wrapper;

                if (token.StartsWith("\\")) // tokens
                {
                    string key = token.Substring(1);

                    if (lilyTokenHandlers.ContainsKey(key)) {
                        wrapper = lilyTokenHandlers[key].Handle(token, tokens, staff, ref i);
                        if (wrapper != null)
                        {
                            relativeStep = wrapper.relativeStep;
                            relativeOctave = wrapper.relativeOctave;
                        }
                    }
                    else
                    {
                        return null; // Invalid token -> error
                    }
                }
                else // Remaining nonTokens
                {
                    if (lilyNonTokenHandlers.ContainsKey(token)) { // at this point the token isn't a token anymore tho
                        lilyNonTokenHandlers[token].Handle(token, staff, null);
                    }
                    else // -> it's not any of the previous we've found -> it's a Note (non)Token. Lets parse it.
                    {
                        wrapper = new RelativeVariablesWrapper();
                        wrapper.relativeStep = relativeStep;
                        wrapper.relativeOctave = relativeOctave;

                        wrapper = lilyNonTokenHandlers["note"].Handle(token, staff, wrapper);
                        if (wrapper != null)
                        {
                            relativeStep = wrapper.relativeStep;
                            relativeOctave = wrapper.relativeOctave;
                        }
                        else
                        {
                            return null; // Invalid note -> error
                        }
                    }
                }
            }

            score.AddStaff(staff);
            return score;
        }
    }
}
