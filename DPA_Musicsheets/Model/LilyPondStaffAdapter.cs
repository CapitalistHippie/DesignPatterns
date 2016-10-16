using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class LilyPondStaffAdapter : Staff
    {
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

        public static ClefType FromLilyPondClef(string lilyPondClef)
        {
            return clefToLilyPondClef[lilyPondClef];
        }

        public static string FromClef(ClefType type)
        {
            return lilyPondClefToClef[type];
        }

        public void AddClef(string clef)
        {
            Symbols.Add(new Clef { Type = FromLilyPondClef(clef) });
        }

        public void AddTimeSignature(string timeSignature)
        {
            Symbols.Add(new TimeSignature
            {
                NumberOfBeats = Int32.Parse(timeSignature.Substring(0, timeSignature.IndexOf('/'))),
                Measure = Int32.Parse(timeSignature.Substring(timeSignature.IndexOf('/') + 1))
            });
        }

        public void AddTempo(string tempo)
        {
            Symbols.Add(new Tempo
            {
                BeatsPerMinute = Int32.Parse(tempo.Substring(tempo.IndexOf('=') + 1))
            });
        }
    }
}
