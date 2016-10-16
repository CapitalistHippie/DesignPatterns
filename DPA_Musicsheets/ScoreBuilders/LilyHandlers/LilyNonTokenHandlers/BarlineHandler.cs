using DPA_Musicsheets.Model;
using DPA_Musicsheets.ScoreBuilders.LilyHandlers.LilyNonTokenHandlers;
using DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders.LilyHandlers
{
    public class BarlineHandler : ILilyNonTokenHandler
    {
        public RelativeVariablesWrapper Handle(string token, LilyPondStaffAdapter staff, RelativeVariablesWrapper wrapper)
        {
            staff.Symbols.Add(new Barline());

            return null;
        }
    }
}
