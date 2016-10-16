using DPA_Musicsheets.Model;
using DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders.LilyHandlers.LilyNonTokenHandlers
{
    public interface ILilyNonTokenHandler
    {
        RelativeVariablesWrapper Handle(string token, LilyPondStaffAdapter staff, RelativeVariablesWrapper wrapper);
    }
}
