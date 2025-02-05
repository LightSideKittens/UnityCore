using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TransformConverter : BaseUnityObjectConverter
{
    public TransformConverter(UnityObjectReferenceConverter referenceConverter) : base(referenceConverter) { }

    protected override void OnSerialize(JToken jObj, object obj)
    {
        Transform tr = (Transform)obj;
        jObj["position"] = tr.localPosition.ToJObject();
        jObj["rotation"] = tr.localRotation.ToJObject();
        jObj["scale"] = tr.localScale.ToJObject();
    }

    public override void Populate(JToken jObj, object obj)
    {
        Transform tr = (Transform)obj;
        tr.localPosition = jObj["position"].ToVector3();
        tr.localRotation = jObj["rotation"].ToQuaternion();
        tr.localScale = jObj["scale"].ToVector3();
    }
}