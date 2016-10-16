using DPA_Musicsheets.MidiEventHandlers;
using DPA_Musicsheets.Model;
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
    public class ScoreBuilder : IScoreBuilder
    {
        private Dictionary<string, IScoreBuilder> scoreBuilders = new Dictionary<string, IScoreBuilder>
        {
            { ".mid",   new MidiScoreBuilder() },
            { ".ly",    new LilyPondScoreBuilder() }
        };

        public Score BuildScoreFromFile(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);

            if (!scoreBuilders.ContainsKey(fileExtension))
                return null;

            return scoreBuilders[fileExtension].BuildScoreFromFile(filePath);
        }

        public Score BuildScoreFromString(string lilyPondText)
        {
            return scoreBuilders[".ly"].BuildScoreFromString(lilyPondText);

            //throw new NotImplementedException();
        }
    }
}
