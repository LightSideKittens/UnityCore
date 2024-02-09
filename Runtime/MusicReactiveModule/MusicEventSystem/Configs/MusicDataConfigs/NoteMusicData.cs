using System;
using System.Collections.Generic;

namespace MusicEventSystem.Configs
{
    public partial class MusicData
    {
        [Serializable]
        public class NoteMusicData<T1> : Dictionary<string, T1> where T1 : BaseTrackData
        {
            private Dictionary<string, T1> tempTracks;
            private Action remover;

            public void Init()
            {
                tempTracks = new Dictionary<string, T1>(this);
                remover = CleanRemover;

                foreach (var (key, value) in tempTracks)
                {
                    value.Completed += () => remover += () => tempTracks.Remove(key);
                }
            }

            public void Update(in float currentTime)
            {
                foreach (var track in tempTracks.Values)
                {
                    track.Update(currentTime);
                }

                remover();
            }

            private void CleanRemover() => remover = CleanRemover;
        }
    }
}