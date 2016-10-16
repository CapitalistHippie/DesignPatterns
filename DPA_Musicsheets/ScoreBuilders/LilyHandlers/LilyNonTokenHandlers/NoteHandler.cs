using DPA_Musicsheets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders.LilyHandlers.LilyNonTokenHandlers
{
    public class NoteHandler : ILilyNonTokenHandler
    {
        private static readonly Dictionary<string, string> NOTE_CONVERSION_TABLE = new Dictionary<string, string>
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

        public LilyTokenHandlers.RelativeVariablesWrapper Handle(string token, Model.LilyPondStaffAdapter staff, LilyTokenHandlers.RelativeVariablesWrapper wrapper)
        {
            // TODO! : Catch not valid notes/tokens -> Return null Score. Do not save this score as a Savepoint/Memento/State

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
                int octave = wrapper.relativeOctave + OctaveOffset(wrapper.relativeStep[0], step[0]);
                octave += (1 * token.Count(x => x == '\''));
                octave -= (1 * token.Count(x => x == ','));
                wrapper.relativeStep = step;
                wrapper.relativeOctave = octave;

                Note note = new Note();
                note.Duration = duration;
                note.Octave = octave;
                note.StepString = stepString;
                note.NumberOfDots = token.Count(x => x == '.');

                staff.Symbols.Add(note);
            }

            return wrapper;
        }

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
    }
}
