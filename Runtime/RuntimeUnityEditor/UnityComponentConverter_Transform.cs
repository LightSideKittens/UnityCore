using System;
using LSCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class UnityComponentConverter
{
    private static void WriteTransform(JToken jObj, object obj, JsonSerializer serializer)
    {
        Transform tr = (Transform)obj;
        jObj["position"] = tr.localPosition.ToJObject();
        jObj["rotation"] = tr.localRotation.ToJObject();
        jObj["scale"] = tr.localScale.ToJObject();
    }

    private static object ReadTransform(JToken jObj, Type objectType, object existingValue, JsonSerializer serializer)
    {
        Transform tr = currentComp.transform;
        tr.localPosition = jObj["position"].ToVector3();
        tr.localRotation = jObj["rotation"].ToQuaternion();
        tr.localScale = jObj["scale"].ToVector3();
        return tr;
    }
}