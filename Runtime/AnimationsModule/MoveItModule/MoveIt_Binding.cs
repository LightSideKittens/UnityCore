using System.Collections.Generic;
using LSCore.Extensions.Unity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

public partial class MoveIt
{
    public interface IPropertyHandler
    {
        void HandleAnimatedProperty(Handler handler, HandlerEvaluator evaluator);
    }
    
    private static Dictionary<string, GenericBinding> cachedBindings = new();
    private static HandlerEvaluator[] refEvaluatorsArr = new HandlerEvaluator[1000];
    private static List<(Object obj, GameObject go, Handler handler)> buffer = new();
    private NativeArray<BoundProperty> floatProps;
    private NativeArray<BoundProperty> discreteProps;
    
    private NativeArray<float> floatValues;
    private NativeArray<int> discreteValues;
    private bool isBound = false;
    
    private void BindCurrent()
    {
        isBound = true;
        int floatPropsIndex = 0;
        int discretePropsIndex = 0;
        buffer.Clear();
        
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];

            if (handler.TryGetPropBindingData(out var obj, out var go))
            {
                var bdpd = handler.evaluators;
                buffer.Add((obj, go, handler));

                for (int j = 0; j < bdpd.Count; j++)
                {
                    var pd = bdpd[j];
                    var isRef = pd.propertyType is PropertyType.Ref;
                    var isFloat = pd.IsFloat;
                    var isDiscrete = isRef || !isFloat;

                    if (isFloat)
                    {
                        floatPropsIndex++;
                    }

                    if (isDiscrete)
                    {
                        discretePropsIndex++;
                    }
                }
            }
        }
        
        floatProps = new NativeArray<BoundProperty>(floatPropsIndex, Allocator.Persistent);
        discreteProps = new NativeArray<BoundProperty>(discretePropsIndex, Allocator.Persistent);
        floatValues = new NativeArray<float>(floatProps.Length, Allocator.Persistent);
        discreteValues = new NativeArray<int>(discreteProps.Length, Allocator.Persistent);

        floatPropsIndex = 0;
        discretePropsIndex = 0;
        
        for (int i = 0; i < buffer.Count; i++)
        {
            var (obj, go, handler) = buffer[i];
            
            var bdpd = handler.evaluators;
            NativeArray<GenericBinding> bindings = new(bdpd.Count, Allocator.Temp);
            int discreteEvaluatorsIndex = 0;
            
            for (int j = 0; j < bdpd.Count; j++)
            {
                var pd = bdpd[j];
                var isRef = pd.propertyType is PropertyType.Ref;
                
                var fullPropPath = string.Concat(handler.fullTypeName, pd.property);
                if (!cachedBindings.TryGetValue(fullPropPath, out var binding))
                {
                    var result = GenericBindingUtility.CreateGenericBinding(obj, pd.rawProperty, go, isRef, out binding);
                    cachedBindings[fullPropPath] = binding;
                }
                
                bindings[j] = binding;

                if (isRef || pd.propertyType is PropertyType.Enum)
                {
                    refEvaluatorsArr[discreteEvaluatorsIndex] = pd;
                    discreteEvaluatorsIndex++;
                }
            }
            
            GenericBindingUtility.BindProperties(
                go,
                bindings,
                out var floatPs,
                out var discretePs,
                Allocator.Temp);
            
            bindings.Dispose();
            
            for (int j = 0; j < floatPs.Length; j++)
            {
                floatProps.Write(floatPropsIndex, floatPs.Read(j));
                floatPropsIndex++;
            }
            
            for (int j = 0; j < discretePs.Length; j++)
            {
                var p = discretePs.Read(j);
                if (p.version == 0)
                {
                    var evaluator = refEvaluatorsArr[j];
                    var type = obj.GetType();
                    var access = PathAccessorCache.Get(type, evaluator.property);
                    evaluator.get = access.Get;
                    evaluator.set = access.Set;
                }

                discreteProps.Write(discretePropsIndex, p);
                discretePropsIndex++;
            }
        }
    }

    private void UnBindCurrent()
    {
        if (isBound)
        {
            isBound = false;
            GenericBindingUtility.UnbindProperties(floatProps);
            GenericBindingUtility.UnbindProperties(discreteProps);
            floatProps.Dispose();
            discreteProps.Dispose();
            floatValues.Dispose();
            discreteValues.Dispose();
        }
    }
}