using DPA_Musicsheets.Model;
using DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPA_Musicsheets.ScoreBuilders
{
    public class RepeatTokenHandler : ILilyTokenHandler
    {
        public RelativeVariablesWrapper Handle(string token, string[] tokens, LilyPondStaffAdapter staff, ref int index)
        {
            string repeatType = tokens[++index];
            switch (repeatType)
            {
                case "volta":
                    int repeatCount = int.Parse(tokens[++index]);
                    //staff.Symbols.Add(new Repeat { Type = RepeatType.FORWARD });
                    break;
            }

            return null;
        }
    }
}
