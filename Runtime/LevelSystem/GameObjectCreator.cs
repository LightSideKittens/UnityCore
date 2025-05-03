using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace LightSideCore.Runtime.LevelSystem
{
    public class GameObjectCreator : MonoBehaviour
    {
        public float delay = 5;
        public int count = 10000;
        public GameObject prefab;
        public GameObject prefab2;
        private static GameObject currentPrefab;

        public async void Awake()
        {
            if(currentPrefab == prefab) currentPrefab = prefab2;
            else currentPrefab = prefab;
            
            await Task.Delay(TimeSpan.FromSeconds(delay));
            
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                Instantiate(currentPrefab);
            }
            sw.Stop();
            Debug.Log(sw.ElapsedTicks);
            
            await Task.Delay(TimeSpan.FromSeconds(delay));
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}