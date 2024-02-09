using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Async;
using MusicEventSystem.Configs;

namespace BeatHeroes
{
    public class ShortNoteListener : IDisposable
    {
        public event Action NoteIn;
        public event Action Completed;
        private float offset;
        private int tracksCount;
        private int completedTracksCount;
        private HashSet<string> keys = new();

        public static ShortNoteListener Listen(float offset = 0)
        {
            var listener = new ShortNoteListener();
            listener.offset = Playlist.MusicDelay + offset;

            foreach (var track in MusicData.ShortTrackData.Values)
            {
                listener.TryListenTrack(track);
            }

            return listener;
        }

        public static ShortNoteListener Listen(string key, float offset = 0)
        {
            var listener = new ShortNoteListener();
            listener.offset = Playlist.MusicDelay + offset;
            listener.TryListenTrackByKey(key);
            return listener;
        }
        
        public static ShortNoteListener Listen(string[] keys, float offset = 0)
        {
            var listener = new ShortNoteListener();
            listener.offset = Playlist.MusicDelay + offset;
            
            for (int i = 0; i < keys.Length; i++)
            {
                listener.TryListenTrackByKey(keys[i]);
            }

            return listener;
        }

        public ShortNoteListener AllMusic()
        {
            Playlist.MusicChanged += () =>
            {
                foreach (var key in keys)
                {
                    TryListenTrackByKey(key);
                }
            };
            
            return this;
        }

        private void TryListenTrackByKey(string key)
        {
            var shortTrack = MusicData.ShortTrackData;
            if(!shortTrack.TryGetValue(key, out var track)) return;

            TryListenTrack(track);
        }

        private void TryListenTrack(ShortNoteTrackData track)
        {
            if (track.notes.Length > 0)
            {
                tracksCount++;
                keys.Add(track.Name);
                track.NoteIn += OnNoteIn;
                track.Completed += OnComplete;
            }
        }
        
        public ShortNoteListener OnNoteIn(Action action)
        {
            NoteIn += action;
            return this;
        }
        
        public ShortNoteListener OnComplete(Action action)
        {
            Completed += action;
            return this;
        }

        private void OnNoteIn()
        {
            Wait.Delay(offset, InvokeNoteIn);
        }

        private void InvokeNoteIn() => NoteIn?.Invoke();

        private void OnComplete()
        {
            Wait.Delay(offset, OnCompleteStoped);
        }

        private void OnCompleteStoped()
        {
            completedTracksCount++;

            if (completedTracksCount >= tracksCount)
            {
                Completed?.Invoke();
            }
        }

        public void Dispose()
        {
            NoteIn = null;
            Completed = null;
        }
    }
}