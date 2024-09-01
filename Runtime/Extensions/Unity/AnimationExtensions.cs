using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class AnimationExtensions
    {
        public static IEnumerable<AnimationState> States(this Animation animation)
        {
            foreach (AnimationState state in animation)
            {
                yield return state;
            }
        }
        
        public static void Invoke(this AnimationEvent animEvent, Object obj)
        {
            var method = obj.GetType().GetMethod(animEvent.functionName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                method.Invoke(obj, null);
            }
            else if (parameters.Length == 1)
            {
                var paramType = parameters[0].ParameterType;

                if (paramType == typeof(string))
                {
                    method.Invoke(obj, new object[] { animEvent.stringParameter });
                }
                else if (paramType == typeof(float))
                {
                    method.Invoke(obj, new object[] { animEvent.floatParameter });
                }
                else if (paramType == typeof(int))
                {
                    method.Invoke(obj, new object[] { animEvent.intParameter });
                }
                else if (paramType == typeof(Object))
                {
                    method.Invoke(obj, new object[] { animEvent.objectReferenceParameter });
                }
            }
        }
    }
}