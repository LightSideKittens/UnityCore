using TMPro;
using UnityEngine;
using View;

namespace BeatHeroes
{
    public class CreateText : MonoBehaviour
    {
        [SerializeField] private NativeTextMeshPro prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private TMP_InputField inputField;


        private void Start()
        {
            var text = Instantiate(prefab, parent);
            text.text = inputField.text;
        }
    }
}
