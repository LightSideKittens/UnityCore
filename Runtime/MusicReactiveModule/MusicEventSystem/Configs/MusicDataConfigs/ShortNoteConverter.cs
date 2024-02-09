using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static MusicEventSystem.Configs.MusicDataContractResolver;
using ShortNoteTrack = MusicEventSystem.Configs.MusicData.NoteMusicData<ShortNoteTrackData>;

namespace MusicEventSystem.Configs
{
    internal class ShortNoteConverter : JsonConverter<ShortNoteTrack>
    {
        public override void WriteJson(JsonWriter writer, ShortNoteTrack value, JsonSerializer serializer)
        {
            var obj = new JObject();
            var tracks = new JObject();
            obj["shortTracks"] = tracks;

            foreach (var track in value)
            {
                var arr = new JArray();
                tracks[track.Key] = arr;
                var notes = track.Value.notes;
                for (int i = 0; i < notes.Length; i++)
                {
                    decimal startTime = Math.Round((decimal)notes[i].startTime, 3);
                    arr.Add(startTime);
                }
            }
            
            obj.WriteTo(writer);
        }

        public override ShortNoteTrack ReadJson(JsonReader reader, Type objectType, ShortNoteTrack existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            existingValue = new ShortNoteTrack();
            
            JObject obj = JObject.Load(reader);
            var prop = obj.Property("shortTracks");

            foreach (var track in (JObject)prop.Value)
            {
                var notes = new List<ShortNoteData>();
                var arr = (JArray)track.Value;
                var noteData = new ShortNoteData();

                for (int i = 0; i < arr.Count; i++)
                {
                    var jvalue = (JValue) arr[i];
                    var time = (float)Convert.ChangeType(jvalue.Value, typeof(float), CultureInfo.InvariantCulture);

                    if (time < MusicData.StartTime)
                    {
                        continue;
                    }
                    
                    if (time >= MusicData.EndTime)
                    {
                        break;
                    }

                    time -= MusicData.StartTime;
                    noteData.startTime = time;
                    notes.Add(noteData);
                    
                    if (time > current.realEndTime)
                    {
                        current.realEndTime = time;
                    }
                    
                }

                var trackData = new ShortNoteTrackData(track.Key, notes.ToArray());
                existingValue.Add(track.Key, trackData);
            }
            
            return existingValue;
        }
    }
}