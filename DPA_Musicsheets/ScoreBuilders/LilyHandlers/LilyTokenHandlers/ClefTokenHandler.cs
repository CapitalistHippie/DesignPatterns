using DPA_Musicsheets.Model;
using DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.ScoreBuilders
{
    public class ClefTokenHandler : ILilyTokenHandler
    {
        public RelativeVariablesWrapper Handle(string token, string[] tokens, LilyPondStaffAdapter staff, ref int index)
        {
            string cleffValue = tokens[++index];
            staff.AddClef(cleffValue);

            return null;
        }
    }
}
