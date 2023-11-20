using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class LSGradient : ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Key
        {
            public float position;
            public Color color;

            public Key(float position, in Color color)
            {
                this.position = position;
                this.color = color;
            }
        }

        [SerializeField] private Key[] keys;
        private LSList<Key> keysList = new();

        public int Count => keysList.Count;
        
        public Key this[int index]
        {
            get => keysList[index];
            set => keysList[index] = value;
        }
        
        public ref Key this[Index index] => ref keysList[index];

        public IEnumerable<float> Positions
        {
            get
            {
                for (int i = 0; i < keysList.Count; i++)
                {
                    yield return keysList[i].position;
                }
            }
        }

        public LSGradient(params Key[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keysList.Add(in keys[i]);
            }
        }
        
        public void Add(float position, in Color color) => keysList.Add(new Key(position, color));
        public void SetPostion(int index, float position) => keysList[(Index)index].position = position;
        public void SetColor(int index, in Color color) => keysList[(Index)index].color = color;
        public void Remove(int index) => keysList.RemoveAt(index);
        public void Clear() => keysList.Clear();

        private void Sort()
        {
            keysList.Sort((a, b) => Math.Sign(b.position - a.position));
        }

        public void FillLegacy(Gradient gradient)
        {
            var colors = new GradientColorKey[keysList.Count];
            var alphas = new GradientAlphaKey[keysList.Count];

            for (int i = 0; i < keysList.Count; i++)
            {
                ref var key = ref keysList[(Index)i];
                colors[i].time = key.position;
                colors[i].color = key.color;
                
                alphas[i].time = key.position;
                alphas[i].alpha = key.color.a;
            }
            
            gradient.SetKeys(colors, alphas);
        }
        

        public Color Evaluate(float time)
        {
            if (keysList.Count == 0)
            {
                return Color.white;
            }

            if (keysList.Count == 1 || time < 0)
            {
                return keysList[0].color;
            }
            
            var de = keysList.OrderBy(x => x.position);
            
            if (time > 1)
            {
                return de.Last().color;
            }

            using var d = de.GetEnumerator();
            int start = 0;
            int end = keysList.Count - 1;

            d.MoveNext();
            Key startKey = d.Current;
            d.MoveNext();
            Key endKey = d.Current;

            while (start < end)
            {
                if (time >= startKey.position && time <= endKey.position)
                {
                    break;
                }

                startKey = endKey;
                d.MoveNext();
                endKey = d.Current;
                start++;
            }
            
            var startPos = startKey.position;
            float blendFactor = (time - startPos) / (endKey.position - startPos);
            return Color.Lerp(startKey.color, endKey.color, blendFactor);
        }

        public void OnBeforeSerialize()
        {
            keysList ??= new LSList<Key>();
            keys ??= Array.Empty<Key>();
            
            if (keys.Length != keysList.Count)
            {
                keys = new Key[keysList.Count];
            }
            
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keysList[i];
            }
        }

        public void OnAfterDeserialize()
        {
            keysList ??= new LSList<Key>();
            keysList.Clear();
            
            for (int i = 0; i < keys.Length; i++)
            { 
                keysList.Add(in keys[i]);
            }
        }
        
    }
}