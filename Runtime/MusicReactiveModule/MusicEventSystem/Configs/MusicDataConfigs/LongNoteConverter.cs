using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static MusicEventSystem.Configs.MusicDataContractResolver;
using LongNoteTrack = MusicEventSystem.Configs.MusicData.NoteMusicData<LongNoteTrackData>;

namespace MusicEventSystem.Configs
{
    internal class LongNoteConverter : JsonConverter<LongNoteTrack>
    {
        public override void WriteJson(JsonWriter writer, LongNoteTrack value, JsonSerializer serializer)
        {
            var obj = new JObject();
            var tracks = new JObject();
            obj["longTracks"] = tracks;

            foreach (var track in value)
            {
                var arr = new JArray();
                tracks[track.Key] = arr;
                var notes = track.Value.notes;
                for (int i = 0; i < notes.Length; i++)
                {
                    var note = notes[i];
                    arr.Add(note.startTime);
                    arr.Add(note.endTime);
                }
            }
            
            obj.WriteTo(writer);
        }

        public override LongNoteTrack ReadJson(JsonReader reader, Type objectType, LongNoteTrack existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            existingValue = new LongNoteTrack();
            
            JObject obj = JObject.Load(reader);
            var prop = obj.Property("longTracks");

            foreach (var track in (JObject)prop.Value)
            {
                var notes = new List<LongNoteData>();
                var arr = (JArray)track.Value;

                var noteData = new LongNoteData();

                for (int i = 0; i < arr.Count; i++)
                {
                    var jvalue = (JValue) arr[i];
                    var time = (float)Convert.ChangeType(jvalue.Value, typeof(float), CultureInfo.InvariantCulture);
                    
                    if (i % 2 == 1)
                    {
                        noteData.endTime = time;
                        notes.Add(noteData);
                        
                        if (time > current.realEndTime)
                        {
                            current.realEndTime = time;
                        }
                    }
                    else
                    {
                        noteData.startTime = time;
                    }
                    
                    if (noteData.startTime < MusicData.StartTime)
                    {
                        continue;
                    }
                    
                    if (noteData.startTime >= MusicData.EndTime)
                    {
                        break;
                    }
                }
                
                var trackData = new LongNoteTrackData(track.Key, notes.ToArray());
                existingValue.Add(track.Key, trackData);
            }
            
            return existingValue;
        }
    }
}