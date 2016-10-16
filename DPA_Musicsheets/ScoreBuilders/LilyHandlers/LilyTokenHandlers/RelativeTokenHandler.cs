using DPA_Musicsheets.Model;
using DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DPA_Musicsheets.ScoreBuilders
{
    public class RelativeTokenHandler : ILilyTokenHandler
    {
        private static readonly int DEFAULT_RELATIVE_OCTAVE = 4;

        public RelativeVariablesWrapper Handle(string token, string[] tokens, LilyPondStaffAdapter staff, ref int index)
        {
            RelativeVariablesWrapper wrapper = new RelativeVariablesWrapper();

            string newRelative = tokens[++index]; //++i -> toevoegen en dan de nieuwe gebruiken, i++ oude gebruiken dan nieuwe toevoegen
            wrapper.relativeStep = Regex.Match(newRelative, "[a-z]+").Value;
            wrapper.relativeOctave = DEFAULT_RELATIVE_OCTAVE + (1 * newRelative.Count(x => x == '\''));
            wrapper.relativeOctave -= (1 * newRelative.Count(x => x == ','));

            return wrapper;
        }
    }
}
