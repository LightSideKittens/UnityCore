using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class AnimationCurves : ScriptableObject
    {
        public const string SwingKick = nameof(SwingKick);
        public const string Hanging = nameof(Hanging);

        public static IEnumerable<string> Names
        {
            get
            {
                yield return SwingKick;
                yield return Hanging;
            }
        }

        [Serializable]
        private record CurveData
        {
            [ValueDropdown("CurvesNames")]
            public string name;
            public AnimationCurve curve;

            public static IEnumerable<string> CurvesNames => Names;
        }

        [TableList] [SerializeField] private List<CurveData> curvesData;
        private readonly Dictionary<string, AnimationCurve> curvesDataDict = new();
        private static Func<AnimationCurves> getter;
        private static AnimationCurves instance;

        static AnimationCurves()
        {
            getter = StaticConstructor;
        }

        private static AnimationCurves StaticConstructor()
        {
            instance = Resources.Load<AnimationCurves>("LGCoreAnimationCurves");
            instance.Init();
            getter = GetInstance;
            return instance;
        }

        private static AnimationCurves GetInstance() => instance;

        private void Init()
        {
            for (int i = 0; i < curvesData.Count; i++)
            {
                var data = curvesData[i];
                curvesDataDict.Add(data.name, data.curve);
            }
        }

        public static AnimationCurve Get(string name)
        {
            return getter().curvesDataDict[name];
        }
    }
}