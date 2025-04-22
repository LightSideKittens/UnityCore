using System.Collections.Generic;
using LSCore.Extensions;
using Unity.Collections;
using UnityEngine.Animations;

public partial class MoveIt
{
    private static List<BoundProperty> floatPropsList = new();
    private static List<BoundProperty> discretePropsList = new();
    
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

            if (handler.TryGetPropBindingData(out var bd))
            {
                var bdpd = bd.propData;
                NativeArray<GenericBinding> bindings = new(bdpd.Length, Allocator.Temp);
                
                for (int j = 0; j < bdpd.Length; j++)
                {
                    var pd = bdpd[i];
                    GenericBindingUtility.CreateGenericBinding(bd.obj, pd.propName, bd.go, pd.isRef, out var d);
                    bindings[j] = d;
                }
                
                GenericBindingUtility.BindProperties(
                    bd.go,
                    bindings,
                    out var floatPs,
                    out var discretePs,
                    Allocator.Temp);
            
                floatPropsList.AddRange(floatPs);
                discretePropsList.AddRange(discretePs);
            }
        }
        
        floatProps = floatPropsList.ToNativeArray(Allocator.Persistent);
        discreteProps = discretePropsList.ToNativeArray(Allocator.Persistent);
        floatValues = new NativeArray<float>(floatProps.Length, Allocator.Persistent);
        discreteValues = new NativeArray<int>(discreteProps.Length, Allocator.Persistent);
        
        floatPropsList.Clear();
        discretePropsList.Clear();
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