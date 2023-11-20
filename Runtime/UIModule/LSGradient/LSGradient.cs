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

        private static Key defaultKey = new() { position = 0, color = Color.white };

        [SerializeField] private Key[] keys;
        private LSList<Key> keysList = new();
        
        public int Count => keysList.Count;

        public Key this[int index]
        {
            get
            {
                if (keysList.TryGet(index, out var value))
                {
                    return value;
                }

                return defaultKey;
            }
            set
            {
                keysList[index] = value;
                Sort();
            }
        }

        public IEnumerable<float> Positions
        {
            get
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    yield return keys[i].position;
                }
            }
        }

        public LSGradient(params Key[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keysList.Add(in keys[i]);
            }

            Sort();
        }

        public void Add(float position, in Color color)
        {
            keysList.Add(new Key(position, color));
            Sort();
        }

        public void SetPostion(int index, float position)
        {
            keysList[(Index)index].position = position;
            Sort();
        }

        public void SetColor(int index, in Color color)
        {
            keysList[(Index)index].color = color;
            Sort();
        }

        public void Remove(int index)
        {
            keysList.RemoveAt(index);
            Sort();
        }

        public void Clear()
        {
            keysList.Clear();
            keys = new[] { defaultKey };
            keysList.Add(defaultKey);
        }

        private void Sort()
        {
            var count = keysList.Count;
            if (keys == null || keys.Length != count)
            {
                if (count == 0)
                {
                    keys = new[] { defaultKey };
                    keysList.Add(defaultKey);
                    return;
                }
                
                keys = new Key[count];
            }

            int i = 0;
            foreach (var key in keysList.OrderBy(x => x.position))
            {
                keys[i++] = key;
            }
        }

        public void FillLegacy(Gradient gradient)
        {
            GradientColorKey[] colors;
            GradientAlphaKey[] alphas;
            var count = keysList.Count;

            if (count > 0)
            {
                colors = new GradientColorKey[keysList.Count];
                alphas = new GradientAlphaKey[keysList.Count];

                for (int i = 0; i < count; i++)
                {
                    ref var key = ref keysList[(Index)i];
                    colors[i].time = key.position;
                    colors[i].color = key.color;

                    alphas[i].time = key.position;
                    alphas[i].alpha = key.color.a;
                }
            }
            else
            {
                colors = new GradientColorKey[1];
                alphas = new GradientAlphaKey[1];
                colors[0].color = defaultKey.color;
                colors[0].time = defaultKey.position;
                alphas[0].alpha = defaultKey.color.a;
                alphas[0].time = defaultKey.position;
            }

            gradient.SetKeys(colors, alphas);
        }


        public Color Evaluate(float time)
        {
            int end = keys.Length - 1;
            if (end == -1) return Color.white;
            if (end == 0) return keys[0].color;

            Key startKey = keys[0];
            Key endKey = keys[^1];

            if (time <= startKey.position) return startKey.color;
            if (time >= endKey.position) return endKey.color;

            int start = 0;

            endKey = keys[1];

            while (start < end)
            {
                if (time >= startKey.position && time <= endKey.position)
                {
                    break;
                }

                startKey = endKey;
                endKey = keys[++start];
            }

            var startPos = startKey.position;
            float blendFactor = (time - startPos) / (endKey.position - startPos);
            return Color.Lerp(startKey.color, endKey.color, blendFactor);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            keysList ??= new LSList<Key>();
            keys ??= new[] { defaultKey };
            keysList.Clear();

            for (int i = 0; i < keys.Length; i++)
            {
                keysList.Add(in keys[i]);
            }
        }
    }
}