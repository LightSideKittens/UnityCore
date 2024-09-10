using LSCore.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace LSCore.ConfigModule.Test
{
    public class TestConfig : LocalDynamicConfig
    {
        private static GameConfigManager<TestConfig> Manager => GetManager("Main");
        public static TestConfig Config => Manager.Config;
        public static TestConfig Get(string path) => GetManager(path).Config;
        public static void Delete(string path) => GetManager(path).Delete();
        public static GameConfigManager<TestConfig> GetManager(string path) => GameConfigManager<TestConfig>.Get(path);
        

        public string name1;
        public int value;

        [JsonIgnore]
        public string Name
        {
            get
            {
                return name1;
            }
            set
            {
                Debug.Log(value);
                name1 = value;
            }
        }

        static TestConfig()
        {
            Migrator.Type<TestConfig>.Add(x =>
            {
                var n = x["name"].ToString();
                x["name1"] = n;
                x["name"].Parent.Remove();
            });
            
            Manager.AddMigration(x =>
            {
                var n = x["value"].ToObject<int>();
                x["value"] = n * 2;
            });
            
            GetManager("NotMain1").AddMigration(x =>
            {
                var n = x["value"].ToObject<int>();
                x["value"] = n * 2;
            });
            
            GetManager("NotMain2").AddMigration(x =>
            {
                var n = x["value"].ToObject<int>();
                x["value"] = n * 2;
            });
                        
            GetManager("NotMain3").AddMigration(x =>
            {
                var n = x["value"].ToObject<int>();
                x["value"] = n * 2;
            });
            
            GetManager("NotMain4").AddMigration(x =>
            {
                var n = x["value"].ToObject<int>();
                x["value"] = n * 2;
            });
        }
        

        protected internal override void SetDefault()
        {
            base.SetDefault();
            Name = "Default";
            value = 1;
        }
    }
    
    public static class Tests
    {
        private static readonly string[] names = { "James", "Killian", "Huston", "Hugo", "Jack", "Jason", "Robert"};

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            TestConfig.Config.Name = names.Random();
            TestConfig.Config.Name = names.Random();
            TestConfig.Get("NotMain1").Name = names.Random();
            TestConfig.Get("NotMain2").Name = names.Random();
            TestConfig.Get("NotMain3").Name = names.Random();
            TestConfig.Get("NotMain4");
            TestConfig.Delete("NotMain4");
            
            Debug.Log(TestConfig.Config.Name);
            Debug.Log(TestConfig.Get("NotMain1").Name);
            Debug.Log(TestConfig.Get("NotMain2").Name);
            Debug.Log(TestConfig.Get("NotMain3").Name);
            Debug.Log(TestConfig.Get("NotMain4"));
        }
    }
}
