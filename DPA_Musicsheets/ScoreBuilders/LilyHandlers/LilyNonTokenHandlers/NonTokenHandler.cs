using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders.LilyHandlers.LilyNonTokenHandlers
{
    public class NonTokenHandler : ILilyNonTokenHandler
    {
        public LilyTokenHandlers.RelativeVariablesWrapper Handle(string token, Model.LilyPondStaffAdapter staff, LilyTokenHandlers.RelativeVariablesWrapper wrapper)
        {
            // Do Nothing, it's either { or }
            return null;
        }
    }
}
