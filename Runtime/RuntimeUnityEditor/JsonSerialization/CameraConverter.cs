using System;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.JsonSerialization
{
    public class CameraConverter : BaseUnityObjectConverter
    {
        public CameraConverter(UnityObjectReferenceConverter referenceConverter) : base(referenceConverter)
        {
        }

        protected override void OnSerialize(JToken jObj, object obj)
        {
            Camera cam = (Camera)obj;
            jObj["clearFlags"] = (int)cam.clearFlags;
            jObj["backgroundColor"] = cam.backgroundColor.ToJObject();
            jObj["cullingMask"] = cam.cullingMask;
            jObj["depth"] = cam.depth;
            jObj["fieldOfView"] = cam.fieldOfView;
            jObj["nearClipPlane"] = cam.nearClipPlane;
            jObj["farClipPlane"] = cam.farClipPlane;
            jObj["orthographic"] = cam.orthographic;
            jObj["orthographicSize"] = cam.orthographicSize;
            jObj["renderingPath"] = (int)cam.renderingPath;
            jObj["targetTexture"] = SerializeReference(cam.targetTexture);
        }

        public override void Populate(JToken jObj, object obj)
        {
            Camera cam = (Camera)obj;
            cam.clearFlags = (CameraClearFlags)jObj["clearFlags"].ToInt();
            cam.backgroundColor = jObj["backgroundColor"].ToColor();
            cam.cullingMask = jObj["cullingMask"].ToInt();
            cam.depth = jObj["depth"].ToFloat();
            cam.fieldOfView = jObj["fieldOfView"].ToFloat();
            cam.nearClipPlane = jObj["nearClipPlane"].ToFloat();
            cam.farClipPlane = jObj["farClipPlane"].ToFloat();
            cam.orthographic = jObj["orthographic"].ToBool();
            cam.orthographicSize = jObj["orthographicSize"].ToFloat();
            cam.renderingPath = (RenderingPath)jObj["renderingPath"].ToInt();
            cam.targetTexture = DeserializeReference<RenderTexture>(jObj["targetTexture"], cam.targetTexture);
        }
    }
}