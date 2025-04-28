using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Debug = UnityEngine.Debug;

public partial class MoveIt
{
    private static Dictionary<string, GenericBinding> cachedBindings = new();
    private static List<BoundProperty> floatPropsList = new();
    private static List<BoundProperty> discretePropsList = new();
    private static List<HandlerEvaluateData> discreteEvaluators = new();
    
    private NativeArray<BoundProperty> floatProps;
    private NativeArray<BoundProperty> discreteProps;
    
    private NativeArray<float> floatValues;
    private NativeArray<int> discreteValues;
    private bool wasBinded = false;
    
    private void BindCurrent()
    {
        wasBinded = true;
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];

            if (handler.TryGetPropBindingData(out var obj, out var go))
            {
                var bdpd = handler.evaluators;
                NativeArray<GenericBinding> bindings = new(bdpd.Count, Allocator.Temp);
                
                for (int j = 0; j < bdpd.Count; j++)
                {
                    var pd = bdpd[j];
                    var fullPropPath = string.Concat(handler.fullTypeName, pd.property);
                    if (!cachedBindings.TryGetValue(fullPropPath, out var binding))
                    {
                        GenericBindingUtility.CreateGenericBinding(obj, pd.property, go, pd.isRef, out binding);
                        cachedBindings[fullPropPath] = binding;
                    }
                    bindings[j] = binding;

                    if (pd.isRef)
                    {
                        discreteEvaluators.Add(pd);
                    }
                }
                    
                GenericBindingUtility.BindProperties(
                    go,
                    bindings,
                    out var floatPs,
                    out var discretePs,
                    Allocator.Temp);
                
                for (int j = 0; j < floatPs.Length; j++)
                {
                    floatPropsList.Add(floatPs.Read(j));
                }
                
                for (int j = 0; j < discretePs.Length; j++)
                {
                    var p = discretePs.Read(j);
                    if (p.version == 0)
                    {
                        
var sw = new Stopwatch();
sw.Start();
                        
                        var type = obj.GetType();
                        var access = PathAccessorCache.Get(type, discreteEvaluators[j].property);
sw.Stop();
Debug.Log(sw.ElapsedTicks);
sw.Restart(); 

                        var test = access.Get(obj);
                        access.Set(obj, default);
                        
sw.Stop();
Debug.Log(sw.ElapsedTicks);

                        Debug.Log(test);

                    }
                    discretePropsList.Add(p);
                }
            }
        }
        
        floatProps = floatPropsList.ToNativeArray(Allocator.Persistent);
        discreteProps = discretePropsList.ToNativeArray(Allocator.Persistent);
        floatValues = new NativeArray<float>(floatProps.Length, Allocator.Persistent);
        discreteValues = new NativeArray<int>(discreteProps.Length, Allocator.Persistent);
        
        floatPropsList.Clear();
        discretePropsList.Clear();
        discreteEvaluators.Clear();
    }

    private void UnBindCurrent()
    {
        if (wasBinded)
        {
            wasBinded = false;
            GenericBindingUtility.UnbindProperties(floatProps);
            GenericBindingUtility.UnbindProperties(discreteProps);
            floatProps.Dispose();
            discreteProps.Dispose();
            floatValues.Dispose();
            discreteValues.Dispose();
        }
    }
}