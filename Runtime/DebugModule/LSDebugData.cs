using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore
{
    public interface ILSDebugData
    {
        string Country { get; set; }
        string Environment { get; set; }
        bool LogWindowsActivity { get; set; }
    }
    
    public partial class LSDebugData : BaseDebugData<LSDebugData>, ILSDebugData
    {
        public static ILSDebugData Data => Config;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SRDebug.Instance.AddOptionContainer(Config);
        }
        

        [Category("LS Debug Data")]
        [Preserve]
        [JsonProperty] 
        public string Country { get; set; }

        [Category("LS Debug Data")]
        [Preserve]
        [JsonProperty] 
        public string Environment { get; set; } = LSConsts.Env.Dev;
    
        [Category("LS Debug Data")]
        [Preserve]
        [JsonProperty] 
        public bool LogWindowsActivity { get; set; }

        protected override void OnLoaded() 
        {
            base.OnLoaded();
            if (string.IsNullOrEmpty(Country))
            {
                Country = "World";
            }
        }
    }
}