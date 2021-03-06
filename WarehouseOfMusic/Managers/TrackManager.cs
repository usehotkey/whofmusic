﻿using WomAudioComponent;

namespace WarehouseOfMusic.Managers
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    public class TrackManager
    {
        /// <summary>
        /// Notes on play
        /// </summary>
        public Queue<ToDoNote> OnPlayNotes;

        /// <summary>
        /// Notes which play in a one time play
        /// </summary>
        public Queue<ToDoNote> OneTimePlayNotes;

        /// <summary>
        /// Played notes
        /// </summary>
        public List<ToDoNote> PlayedNotes;

        /// <summary>
        /// Instrument for play
        /// </summary>
        public WaveformType Instrument;

        /// <summary>
        /// True if all notes of track is played
        /// </summary>
        public bool IsTrackEnd
        {
            get
            {
                return PlayedNotes.Count == 0 && OnPlayNotes.Count == 0;
            }
        }

        /// <summary>
        /// Control for playing notes of track
        /// </summary>
        public TrackManager(ToDoTrack onPlayTrack, int initialTact)
        {
            Instrument = onPlayTrack.Instrument.Waveform;
            OnPlayNotes = new Queue<ToDoNote>();
            PlayedNotes = new List<ToDoNote>();
            foreach (var note in
                    onPlayTrack.Samples.OrderBy(x => x.InitialTact)
                        .Select(sample => sample.Notes.Where(x => x.Tact >= initialTact).OrderBy(x => x.Tact).ThenBy(x => x.Position))
                        .SelectMany(notes => notes))
            {
                OnPlayNotes.Enqueue(note);
            }
        }
    }
}