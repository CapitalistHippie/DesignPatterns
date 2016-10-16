using DPA_Musicsheets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ScoreBuilders
{
    public interface IScoreBuilder
    {
        Score BuildScoreFromFile(string filePath);
        Score BuildScoreFromString(string editorText);
    }
}
