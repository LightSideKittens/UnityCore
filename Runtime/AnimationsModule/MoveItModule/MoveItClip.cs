using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MoveIt;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MoveItClip : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public struct Data : IEquatable<Data>
    {
        public string handlerGuid;
        public List<HandlerEvaluateData> evaluators;

        public bool Equals(Data other)
        {
            return handlerGuid == other.handlerGuid;
        }

        public override int GetHashCode()
        {
            return handlerGuid.GetHashCode();
        }
    }

    [HideInInspector]
    public string guid;
    [HideInInspector] 
    [SerializeField] public List<Data> data = new();
    [HideInInspector]
    [SerializeField] public float length;
    
    public Dictionary<string, Dictionary<string, HandlerEvaluateData>> evaluatorsByHandlerGuids = new();

    public void OnBeforeSerialize()
    {

    }

    public void CreateGuid()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(guid))
        {
            var path = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                guid = AssetDatabase.AssetPathToGUID(path);
                this.ForceSave();
            }
        }
#endif
    }
    
    public void OnAfterDeserialize()
    {
        evaluatorsByHandlerGuids = new();
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            var guid = d.handlerGuid;
            if (!evaluatorsByHandlerGuids.TryGetValue(guid, out var curves))
            {
                curves = new Dictionary<string, HandlerEvaluateData>();
                evaluatorsByHandlerGuids.Add(guid, curves);
            }

            for (int j = 0; j < d.evaluators.Count; j++)
            {
                var evaluator = d.evaluators[j];
                curves.Add(evaluator.property, evaluator);
            }
        }
    }
    
    public void Add(Handler handler, HandlerEvaluateData evaluator)
    {
        if (!evaluatorsByHandlerGuids.TryGetValue(handler.guid, out var curves))
        {
            curves = new Dictionary<string, HandlerEvaluateData>();
            evaluatorsByHandlerGuids.Add(handler.guid, curves);
        }

        curves[evaluator.property] = evaluator;
        var d = data.FirstOrDefault(x => x.handlerGuid == handler.guid);
        var needToAdd = d.evaluators == null;
        
        if (needToAdd)
        {
            d.handlerGuid = handler.guid;
            d.evaluators = new List<HandlerEvaluateData>();
        }

        var index = d.evaluators.IndexOf(evaluator);
        
        if (index == -1)
        {
            d.evaluators.Add(evaluator);
        }
        else
        {
            d.evaluators[index] = evaluator;
        }

        if (needToAdd)
        {
            data.Add(d);
        }
    }
    
    public void Remove(string handlerGuid)
    {
        evaluatorsByHandlerGuids.Remove(handlerGuid);
        var d = data.FirstOrDefault(x => x.handlerGuid == handlerGuid);
        data.Remove(d);
    }

    public void Remove(Handler handler) => Remove(handler.guid);
    
    public void Remove(Handler handler, string propertyName)
    {
        var d = data.FirstOrDefault(x => x.handlerGuid == handler.guid);
        if (d.evaluators != null)
        {
            d.evaluators.Remove(new HandlerEvaluateData {property = propertyName});
            if (evaluatorsByHandlerGuids.TryGetValue(handler.guid, out var curves))
            {
                curves.Remove(propertyName);
            }
            if (d.evaluators.Count == 0)
            {
                data.Remove(d);
                evaluatorsByHandlerGuids.Remove(handler.guid);
            }
        }
    }
}