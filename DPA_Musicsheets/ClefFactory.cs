using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets
{
    public class ClefFactory
    {
        private static ClefFactory instance;

        public static ClefFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClefFactory();
                return instance;
            }
        }

        private static readonly Dictionary<ClefType, string> lilyPondClefToClef = new Dictionary<ClefType, string>
        {
            { ClefType.G, "treble" },
            { ClefType.C, "tenor" },
            { ClefType.F, "bass" }
        };

        private static readonly Dictionary<string, ClefType> clefToLilyPondClef = new Dictionary<string, ClefType>
        {
            { "treble", ClefType.G },
            { "tenor", ClefType.C },
            { "bass", ClefType.F }
        };

        public Clef ConstructFromLilyPondClef(string lilyPondClef)
        {
            Clef clef = new Clef();
            clef.Type = clefToLilyPondClef[lilyPondClef];
            return clef;
        }

        public string ConstructLilyPondClef(ClefType type)
        {
            return lilyPondClefToClef[type];
        }
    }
}
