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
    public class MidiScoreBuilder : IScoreBuilder
    {
        /// <summary>
        /// Checks if the track is a control track. Which by our
        /// definition it is if there are no NoteOn or NoteOff commands in it
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        private bool IsControlTrack(Track track)
        {
            bool isit = !track.Iterator().Any(item =>
            {
                if (item.MidiMessage.MessageType == MessageType.Channel)
                {
                    var message = item.MidiMessage as ChannelMessage;
                    if (message.Command == ChannelCommand.NoteOn || message.Command == ChannelCommand.NoteOff)
                        return true;
                }
                return false;
            });

            return isit;
        }

        /// <summary>
        /// Merges specific global messages from a control track into other tracks.
        /// </summary>
        /// <param name="controlTrack"></param>
        /// <param name="tracks"></param>
        private void MergeControlTrack(Track controlTrack, IEnumerable<Track> tracks)
        {
            // Get the indexes of the events to merge and the events to erase.
            List<int> mergeEventIndexes = new List<int>();
            List<int> eraseEventIndexes = new List<int>();
            for (int i = 0; i < controlTrack.Iterator().Count(); i++)
            {
                var midiEvent = controlTrack.Iterator().ElementAt(i);

                // We only need to merge certain specific messages.
                if (midiEvent.MidiMessage.MessageType == MessageType.Meta)
                {
                    var message = midiEvent.MidiMessage as MetaMessage;
                    if (message.MetaType == MetaType.Tempo || message.MetaType == MetaType.TimeSignature)
                    {
                        mergeEventIndexes.Add(i);
                        eraseEventIndexes.Add(i);
                    }
                    else if (message.MetaType == MetaType.TrackName)
                    {
                        eraseEventIndexes.Add(i);
                    }
                }
            }

            // Merge the messages.
            foreach (var eventIndex in mergeEventIndexes)
            {
                var midiEvent = controlTrack.Iterator().ElementAt(eventIndex);

                foreach (var track in tracks)
                    track.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage); 
            }

            // Erase the merged and other unneccessary events from the control track.
            int offset = 0;
            foreach (var eventIndex in eraseEventIndexes)
                controlTrack.RemoveAt(eventIndex - offset++);
        }

        public Score BuildScoreFromFile(string filePath)
        {
            Score score = new Score();

            // Read the MIDI sequence.
            var midiSequence = new Sequence();
            midiSequence.Load(filePath);

            Dictionary<MessageType, IEventHandler> eventHandlers = new Dictionary<MessageType, IEventHandler>
            {
                { MessageType.Channel,  new ChannelEventHandler(score, midiSequence) },
                { MessageType.Meta,     new MetaEventHandler(score) },
            };

            // Get sets of normal tracks and control tracks.
            var normalTracks = midiSequence.Where(track => !IsControlTrack(track));
            var controlTracks = midiSequence.Where(track => IsControlTrack(track));

            // Merge the control tracks into the normal tracks.
            foreach (var controlTrack in controlTracks)
                MergeControlTrack(controlTrack, normalTracks);

            // Handle the midi events.
            for (int trackIndex = 0; trackIndex < midiSequence.Count; trackIndex++)
            {
                Staff staff = null;
                // Add a staff for this track if its not a control track.
                if (!IsControlTrack(midiSequence[trackIndex]))
                    score.AddStaff(staff = new Staff());

                foreach (var midiEvent in midiSequence[trackIndex].Iterator())
                {
                    if (eventHandlers.ContainsKey(midiEvent.MidiMessage.MessageType))
                    {
                        eventHandlers[midiEvent.MidiMessage.MessageType].
                            Handle(midiEvent, trackIndex);
                    }
                }

                // If the staff got no name just call it the track id.
                if (staff != null && (staff.StaffName == null || staff.StaffName == ""))
                    staff.StaffName = trackIndex.ToString();
            }

            return score;
        }

        public Score BuildScoreFromString(string lilyPondText)
        {
            // Not supposed to get at this point, ever. The midibuilder is, ofcourse not supposed to implement nor read lilyPondText, duh.
            throw new NotImplementedException();
        }
    }
}
