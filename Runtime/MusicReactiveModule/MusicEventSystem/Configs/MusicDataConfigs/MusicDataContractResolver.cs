using System;
using Newtonsoft.Json.Serialization;

namespace MusicEventSystem.Configs
{
    internal class MusicDataContractResolver : DefaultContractResolver
    {
        internal static MusicData current;
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            
            if (objectType == typeof(MusicData.NoteMusicData<LongNoteTrackData>))
            {
                contract.Converter = new LongNoteConverter();
            }
            else if(objectType == typeof(MusicData.NoteMusicData<ShortNoteTrackData>))
            {
                contract.Converter = new ShortNoteConverter();
            }
			
            return contract;
        }
    }
}