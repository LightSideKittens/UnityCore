using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BeatHeroes
{
    public class Test1 : LocalDynamicConfig
    {
        private static readonly GameConfigManager<Test1> manager = GetManager("Main");
        public static Test1 Config => manager.Config;
        public static Test1 Get(string path) => GetManager(path).Config;
        public static void Delete(string path) => GetManager(path).Delete();
        public static GameConfigManager<Test1> GetManager(string path) => GameConfigManager<Test1>.Get(path);
        

        public string name;

        [JsonIgnore]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                Debug.Log(value);
                name = value;
            }
        }

        protected override void SetDefault()
        {
            base.SetDefault();
            Name = "Default";
        }
    }
    
    public class CreateText : MonoBehaviour
    {
        private static readonly string[] names = { "James", "Killian", "Huston", "Hugo", "Jack", "Jason", "Robert"};
        
        private void Start()
        {
            Test1.Config.Name = names.Random();
            Test1.Config.Name = names.Random();
            Test1.Get("NotMain1").Name = names.Random();
            Test1.Get("NotMain2").Name = names.Random();
            Test1.Get("NotMain3").Name = names.Random();
            Test1.Get("NotMain4");
            Test1.Delete("NotMain4");
        }
    }
}
