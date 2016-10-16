using DPA_Musicsheets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders.LilyTokenHandlers
{
    public interface ILilyTokenHandler
    {
        RelativeVariablesWrapper Handle(string token, string[] tokens, LilyPondStaffAdapter staff, ref int index);
    }
}
