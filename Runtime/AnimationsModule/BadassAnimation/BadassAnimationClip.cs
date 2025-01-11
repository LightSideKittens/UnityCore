using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BadassAnimationClip : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public struct Data : IEquatable<Data>
    {
        public string handlerGuid;
        public List<NameToCurve> namesToCurves;

        public bool Equals(Data other)
        {
            return handlerGuid == other.handlerGuid;
        }

        public override int GetHashCode()
        {
            return handlerGuid.GetHashCode();
        }
    }
    
    [Serializable]
    public struct NameToCurve : IEquatable<NameToCurve>
    {
        public string propertyName;
        public BadassCurve curve;

        public bool Equals(NameToCurve other)
        {
            return propertyName == other.propertyName;
        }

        public override int GetHashCode()
        {
            return propertyName.GetHashCode();
        }
    }
    
    [HideInInspector] 
    [SerializeField] public List<Data> data = new();
    public Dictionary<string, Dictionary<string, BadassCurve>> namesToCurvesByHandlerGuids = new();
    
    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        namesToCurvesByHandlerGuids = new();
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            var guid = d.handlerGuid;
            if (!namesToCurvesByHandlerGuids.TryGetValue(guid, out var curves))
            {
                curves = new Dictionary<string, BadassCurve>();
                namesToCurvesByHandlerGuids.Add(guid, curves);
            }

            for (int j = 0; j < d.namesToCurves.Count; j++)
            {
                var nameToCurve = d.namesToCurves[j];
                curves.Add(nameToCurve.propertyName, nameToCurve.curve);
            }
        }
    }
    
    public void Add(BadassAnimation.Handler handler, string propertyName, BadassCurve curve)
    {
        handler.AddEvaluator(propertyName, curve);
        
        if (!namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
        {
            curves = new Dictionary<string, BadassCurve>();
            namesToCurvesByHandlerGuids.Add(handler.guid, curves);
        }

        curves[propertyName] =  curve;
        var d = data.FirstOrDefault(x => x.handlerGuid == handler.guid);
        var needToAdd = d.namesToCurves == null;
        
        if (needToAdd)
        {
            d.handlerGuid = handler.guid;
            d.namesToCurves = new List<NameToCurve>();
        }
        
        var nameToCurve = new NameToCurve
        {
            propertyName = propertyName,
            curve = curve
        };

        var index = d.namesToCurves.IndexOf(nameToCurve);
        
        if (index == -1)
        {
            d.namesToCurves.Add(nameToCurve);
        }
        else
        {
            d.namesToCurves[index] = nameToCurve;
        }

        if (needToAdd)
        {
            data.Add(d);
        }
    }
    
    public void Remove(string handlerGuid)
    {
        namesToCurvesByHandlerGuids.Remove(handlerGuid);
        var d = data.FirstOrDefault(x => x.handlerGuid == handlerGuid);
        data.Remove(d);
    }

    public void Remove(BadassAnimation.Handler handler) => Remove(handler.guid);
    
    public void Remove(BadassAnimation.Handler handler, string propertyName)
    {
        namesToCurvesByHandlerGuids.Remove(handler.guid);
        var d = data.FirstOrDefault(x => x.handlerGuid == handler.guid);
        if (d.namesToCurves != null)
        {
            d.namesToCurves.Remove(new NameToCurve {propertyName = propertyName});
            if (d.namesToCurves.Count == 0)
            {
                data.Remove(d);
            }
        }
    }
}