using System.Collections.Generic;
using System.IO;
using LSCore.NativeUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.Support
{
    public class TestCamera : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private RawImage prefab;
        
        private void Awake()
        {
            button.onClick.AddListener(OnOpenCameraButton);
        }
        private void OnOpenCameraButton()
        {
            var emojis = Emoji.ParseEmojis(inputField.text, Path.Combine(Application.persistentDataPath, "Emojis"));
            
            for (int i = 0; i < emojis.Length; i++)
            {
                var filePath = emojis[i].imagePath;
                Debug.Log($"{(emojis[i].index, emojis[i].length)}");
                
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(filePath));
                var instance = Instantiate(prefab, prefab.transform.parent).transform.GetChild(0).GetComponent<RawImage>();
                var aspect = instance.GetComponent<AspectRatioFitter>();
                float aspectRatio = (float)tex.width / tex.height;
                aspect.aspectRatio = aspectRatio;
                instance.texture = tex;
                texture2Ds.Add(tex);
                Debug.Log("Loaded");
            }
        }
        
        private List<Texture2D> texture2Ds = new();
        
    }
}