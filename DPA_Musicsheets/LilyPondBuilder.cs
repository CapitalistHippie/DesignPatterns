using DPA_Musicsheets.Model;
using Sanford.Multimedia.Midi;
using System;
using System.IO;

namespace DPA_Musicsheets
{
    public class LilyPondBuilder
    {
        private static LilyPondBuilder instance;

        public static LilyPondBuilder Instance
        {
            get
            {
                if (instance == null)
                    instance = new LilyPondBuilder();
                return instance;
            }
        }

        private LilyPondBuilder()
        {
        }
    }
}
