using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;
using static SoundventTypes;

namespace MusicEventSystem.Configs
{
    public class MusicDataConfigManager : ResourcesConfigManager<MusicData>
    {
        protected override ConfigSerializationSettings Settings { get; } = new(new() {ContractResolver = new MusicDataContractResolver()});
    }
    
    //TODO: Refactor for new Config logic
    public partial class MusicData : LocalDynamicConfig
    {
        //protected override string FileName => configName;
        public static MusicData Config => Manager.Config;
        public static MusicDataConfigManager Manager => ConfigMaster<MusicDataConfigManager>.Get($"MusicData/{configName}");
        
        public const float DefaultEndTime = 600;
        private static string configName = string.Empty;
        private static float lastCurrentTime;
        public static event Action TactTicked;
        public static NoteMusicData<ShortNoteTrackData> ShortTrackData => Config.shortNoteTrack;
        public static NoteMusicData<LongNoteTrackData> LongTrackData => Config.longNoteTrack;

        internal static string ConfigName
        {
            get => configName;
            set
            {
                //LoadOnNextAccess();
                configName = value;
            }
        }

        internal static float StartTime { get; set; }
        internal static float EndTime { get; set; } = DefaultEndTime;
        [JsonIgnore] internal float realEndTime;
        [JsonProperty(Order = 0)] internal float bpmStep = -1;
        [JsonProperty(Order = 1)] public NoteMusicData<ShortNoteTrackData> shortNoteTrack = new();
        [JsonProperty(Order = 2)] public NoteMusicData<LongNoteTrackData> longNoteTrack = new();
        
        protected override void SetDefault()
        {
            base.SetDefault();
            if (int.TryParse(configName, out var bpm))
            {
                bpmStep = 60f / bpm;
                Burger.Log($"Music config uses only BPM. BPM: {bpm}, Step: {bpmStep}");
                var notes = new List<ShortNoteData>();
                var time = StartTime - StartTime % bpmStep + bpmStep;
                
                for (; time <= EndTime; time += bpmStep)
                {
                    notes.Add(new ShortNoteData(time - StartTime));
                }

                realEndTime = time - StartTime;
                shortNoteTrack.Add(ShortI, new ShortNoteTrackData(ShortI, notes.ToArray()));
                
                return; 
            }

            Burger.Error($"Music config with name {ConfigName} not found");
        }

        protected override void OnSerializing()
        {
            base.OnSerializing();
            MusicDataContractResolver.current = this;
        }

        protected override void OnSerialized()
        {
            base.OnSerialized();
            shortNoteTrack.Init();
            longNoteTrack.Init();
            lastCurrentTime = 0;
        }

        internal static MusicData Load(string cfgName, float startTime, float endTime)
        {
            var lastCfgName = configName;
            var lastStartTime = StartTime;
            var lastEndTime = EndTime;
            //var lastInstance = instance;
            var config = new MusicData();
            SetConfig(config, cfgName, startTime, endTime);
            //SetConfig(lastInstance, lastCfgName, lastStartTime, lastEndTime);

            return config;
        }

        internal static void SetConfig(MusicData musicData, string cfgName, float startTime, float endTime)
        {
            configName = cfgName;
            StartTime = startTime;
            EndTime = endTime;
            //instance = musicData;
        }

        internal static void Update(in float currentTime) => Config.Internal_Update(currentTime);

        private void Internal_Update(in float currentTime)
        {
            if (currentTime >= lastCurrentTime + bpmStep)
            {
                TactTicked?.Invoke();
                lastCurrentTime = currentTime;
            }
            
            shortNoteTrack.Update(currentTime);
            longNoteTrack.Update(currentTime);
        }
    }
}