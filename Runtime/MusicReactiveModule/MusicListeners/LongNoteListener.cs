using System;
using LSCore.Async;
using MusicEventSystem.Configs;

namespace BeatHeroes
{
    public class LongNoteListener
    {
        public event Action Started;
        public event Action Ended;
        public event Action Completed;
        private float offset;

        public static LongNoteListener Listen(string key, float offset = 0)
        {
            var listener = new LongNoteListener();
            listener.offset = Playlist.MusicDelay + offset;
            var shortTrack = MusicData.LongTrackData;
            var track = shortTrack[key];
            track.NoteIn += listener.OnStart;
            track.NoteOut += listener.OnEnd;
            track.Completed += listener.OnComplete;
            
            return listener;
        }

        private void OnStart()
        {
            Wait.Delay(offset, OnStartStoped);
        }

        private void OnStartStoped()
        {
            Started?.Invoke();
        }
        
        private void OnEnd()
        {
            Wait.Delay(offset, OnEndStoped);
        }

        private void OnEndStoped()
        {
            Ended?.Invoke();
        }
        
        private void OnComplete()
        {
            Wait.Delay(offset, OnCompleteStoped);
        }

        private void OnCompleteStoped()
        {
            Completed?.Invoke();
        }
    }
}